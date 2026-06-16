using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using SubProcessDemo.Common.Ipc;
using SubProcessDemo.Common.Models;

namespace SubProcessDemo.Host.Pool;

/// <summary>
/// 策略①：进程预热池管理器。
/// 提前创建子进程并完成 CLR 初始化，使其处于 Ready 待命状态。
/// 当需要某个模块时，直接向已就绪的进程发送激活指令。
/// </summary>
public class ProcessPoolManager : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, PooledProcess> _pool = new();
    private readonly string _workerExePath;
    private readonly int _parentPid;

    public event Action<string>? OnLog;
    public event Action<string, WorkerProcessInfo>? OnWorkerStateChanged;

    public IReadOnlyCollection<WorkerProcessInfo> Workers =>
        _pool.Values.Select(p => p.ToInfo()).ToList().AsReadOnly();

    public ProcessPoolManager(string workerExePath)
    {
        _workerExePath = workerExePath;
        _parentPid = Environment.ProcessId;
    }

    /// <summary>
    /// 预热 N 个子进程
    /// </summary>
    public async Task WarmUpAsync(int count = 3)
    {
        Log($"开始预热 {count} 个子进程...");

        var tasks = new List<Task>();
        for (int i = 0; i < count; i++)
        {
            var workerId = $"Worker-{(char)('A' + i)}";
            tasks.Add(WarmUpOneAsync(workerId));
        }

        await Task.WhenAll(tasks);
        Log($"预热池初始化完成，{_pool.Values.Count(p => p.State == WorkerState.Ready)} 个进程就绪");
    }

    private async Task WarmUpOneAsync(string workerId)
    {
        var pipeName = $"SubProcessDemo_{workerId}_{Guid.NewGuid():N}";
        var sw = Stopwatch.StartNew();

        // 创建管道 Server（先监听，等 Worker 连入）
        var channelTask = SimpleIpcChannel.CreateServerAsync(pipeName);

        // 启动子进程
        var startInfo = new ProcessStartInfo
        {
            FileName = _workerExePath,
            Arguments = $"--warmup --pipe={pipeName} --parent-pid={_parentPid}",
            UseShellExecute = false,
        };

        var process = Process.Start(startInfo);
        if (process == null)
        {
            Log($"[{workerId}] 启动失败");
            return;
        }

        // 绑定到 Job Object
        JobObjectHelper.AddProcess(process);

        var pooled = new PooledProcess
        {
            WorkerId = workerId,
            Process = process,
            PipeName = pipeName,
            State = WorkerState.WarmingUp,
            CreatedTime = DateTime.Now
        };
        _pool[workerId] = pooled;
        NotifyStateChanged(workerId);

        Log($"[{workerId}] PID={process.Id}, 管道={pipeName[..20]}..., 等待预热完成...");

        // 等待 Worker 连接管道
        var channel = await channelTask;
        pooled.Channel = channel;

        // 监听 Worker 消息
        channel.MessageReceived += async msg =>
        {
            switch (msg.Type)
            {
                case IpcMessageType.Ready:
                    sw.Stop();
                    pooled.State = WorkerState.Ready;
                    pooled.WarmupElapsedMs = sw.Elapsed.TotalMilliseconds;
                    Log($"[{workerId}] ✅ Ready（预热耗时 {pooled.WarmupElapsedMs:F0}ms）");
                    NotifyStateChanged(workerId);
                    break;

                case IpcMessageType.ModuleActivated:
                    pooled.State = WorkerState.Active;
                    var payload = JsonSerializer.Deserialize<JsonElement>(msg.Payload ?? "{}");
                    var activationMs = payload.TryGetProperty("ActivationMs", out var v) ? v.GetDouble() : 0;
                    pooled.ActivationElapsedMs = activationMs;
                    pooled.AssignedModule = payload.TryGetProperty("ModuleName", out var mn) ? mn.GetString() : null;
                    Log($"[{workerId}] 🔴 Active — 模块 {pooled.AssignedModule}（激活耗时 {activationMs:F0}ms）");
                    NotifyStateChanged(workerId);
                    break;

                case IpcMessageType.ServiceCall:
                    await HandleServiceCallFromWorker(workerId, msg);
                    break;

                case IpcMessageType.HealthReport:
                    Log($"[{workerId}] 健康检查: {msg.Payload}");
                    break;
            }
        };

        channel.ErrorOccurred += ex =>
        {
            pooled.State = WorkerState.Faulted;
            Log($"[{workerId}] ❌ 管道错误: {ex.Message}");
            NotifyStateChanged(workerId);
        };
    }

    /// <summary>
    /// 激活一个 Ready 进程来承载指定模块
    /// </summary>
    public async Task<(bool success, double activationMs)> ActivateAsync(string moduleName)
    {
        var pooled = _pool.Values
            .FirstOrDefault(p => p.State == WorkerState.Ready && p.AssignedModule == null);

        if (pooled == null)
        {
            Log("池中无可用 Ready 进程");
            return (false, 0);
        }

        var sw = Stopwatch.StartNew();
        pooled.State = WorkerState.Active;
        pooled.AssignedModule = moduleName;
        NotifyStateChanged(pooled.WorkerId);

        await pooled.Channel!.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.ActivateModule,
            CallId = Guid.NewGuid().ToString("N"),
            Payload = moduleName
        });

        // 等待 Worker 报告 ModuleActivated（最多 10 秒）
        for (int i = 0; i < 100 && pooled.ActivationElapsedMs == 0; i++)
            await Task.Delay(100);

        sw.Stop();
        var elapsed = pooled.ActivationElapsedMs > 0 ? pooled.ActivationElapsedMs : sw.Elapsed.TotalMilliseconds;
        return (true, elapsed);
    }

    /// <summary>
    /// 冷启动对比：直接启动新进程加载模块
    /// </summary>
    public async Task<double> ColdStartAsync(string moduleName)
    {
        var pipeName = $"SubProcessDemo_Cold_{Guid.NewGuid():N}";
        var sw = Stopwatch.StartNew();

        Log($"冷启动：直接创建新进程加载 [{moduleName}]...");

        // 创建管道
        var channelTask = SimpleIpcChannel.CreateServerAsync(pipeName);

        var startInfo = new ProcessStartInfo
        {
            FileName = _workerExePath,
            Arguments = $"--cold --module={moduleName} --pipe={pipeName}",
            UseShellExecute = false,
        };

        var process = Process.Start(startInfo);
        if (process == null) return 0;

        JobObjectHelper.AddProcess(process);

        var channel = await channelTask;
        var tcs = new TaskCompletionSource<double>();

        channel.MessageReceived += msg =>
        {
            if (msg.Type == IpcMessageType.ColdStartResult)
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(msg.Payload ?? "{}");
                var totalMs = payload.TryGetProperty("TotalMs", out var v) ? v.GetDouble() : sw.Elapsed.TotalMilliseconds;
                tcs.TrySetResult(totalMs);
            }
        };

        // 等待结果或进程退出
        var resultTask = tcs.Task;
        var timeoutTask = Task.Delay(30000);
        var completed = await Task.WhenAny(resultTask, timeoutTask);

        sw.Stop();

        if (completed == resultTask)
        {
            var ms = await resultTask;
            Log($"冷启动完成：{ms:F0}ms");
            return ms;
        }

        Log($"冷启动超时，估算耗时 {sw.ElapsedMilliseconds}ms");
        return sw.Elapsed.TotalMilliseconds;
    }

    /// <summary>是否有 Ready 状态的进程</summary>
    public bool HasReadyProcess => _pool.Values.Any(p => p.State == WorkerState.Ready);

    /// <summary>是否有 Active 状态的进程</summary>
    public bool HasActiveProcess => _pool.Values.Any(p => p.State == WorkerState.Active);

    /// <summary>向任意一个 Active 状态的 Worker 发送消息</summary>
    public async Task<(bool success, string workerId)> SendToAnyActiveWorkerAsync(IpcMessage msg)
    {
        var pooled = _pool.Values.FirstOrDefault(p => p.State == WorkerState.Active);
        if (pooled?.Channel == null)
            return (false, "");

        await pooled.Channel.SendAsync(msg);
        return (true, pooled.WorkerId);
    }

    /// <summary>获取指定名称的 Worker 发送服务调用响应</summary>
    public async Task SendToWorkerAsync(string workerId, IpcMessage msg)
    {
        if (_pool.TryGetValue(workerId, out var pooled) && pooled.Channel != null)
            await pooled.Channel.SendAsync(msg);
    }

    /// <summary>处理来自 Worker 的服务代理调用</summary>
    public event Func<string, IpcMessage, Task>? ServiceCallReceived;

    private async Task HandleServiceCallFromWorker(string workerId, IpcMessage msg)
    {
        if (ServiceCallReceived != null)
            await ServiceCallReceived(workerId, msg);
    }

    /// <summary>向所有 Worker 发送关闭指令</summary>
    public async Task ShutdownAllAsync()
    {
        foreach (var pooled in _pool.Values)
        {
            if (pooled.Channel != null)
            {
                try
                {
                    await pooled.Channel.SendAsync(new IpcMessage { Type = IpcMessageType.Shutdown });
                }
                catch { /* 忽略 */ }
            }
        }

        // 等待进程退出
        await Task.Delay(500);

        foreach (var pooled in _pool.Values)
        {
            if (!pooled.Process.HasExited)
            {
                try { pooled.Process.Kill(); } catch { }
            }
        }
    }

    /// <summary>重置：关闭所有进程并清空池</summary>
    public async Task ResetAsync()
    {
        await ShutdownAllAsync();
        foreach (var kvp in _pool)
        {
            if (kvp.Value.Channel != null)
                await kvp.Value.Channel.DisposeAsync();
            _pool.TryRemove(kvp.Key, out _);
        }
    }

    private void Log(string msg) => OnLog?.Invoke(msg);
    private void NotifyStateChanged(string workerId)
    {
        if (_pool.TryGetValue(workerId, out var p))
            OnWorkerStateChanged?.Invoke(workerId, p.ToInfo());
    }

    public async ValueTask DisposeAsync()
    {
        await ShutdownAllAsync();
        foreach (var p in _pool.Values)
        {
            if (p.Channel != null)
                await p.Channel.DisposeAsync();
        }
    }

    // ── 内部类型 ──

    private class PooledProcess
    {
        public string WorkerId { get; set; } = "";
        public Process Process { get; set; } = null!;
        public string PipeName { get; set; } = "";
        public WorkerState State { get; set; } = WorkerState.WarmingUp;
        public string? AssignedModule { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public double WarmupElapsedMs { get; set; }
        public double ActivationElapsedMs { get; set; }
        public SimpleIpcChannel? Channel { get; set; }

        public WorkerProcessInfo ToInfo() => new()
        {
            WorkerId = WorkerId,
            Pid = Process?.Id ?? 0,
            PipeName = PipeName,
            State = State,
            AssignedModule = AssignedModule,
            CreatedTime = CreatedTime,
            WarmupElapsedMs = WarmupElapsedMs,
            ActivationElapsedMs = ActivationElapsedMs,
        };
    }
}
