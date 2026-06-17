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
    private volatile bool _disposing;

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

        // 监控进程退出（崩溃检测）
        process.EnableRaisingEvents = true;
        process.Exited += (s, e) =>
        {
            var exitCode = process.ExitCode;
            if (exitCode != 0 && !_disposing)
            {
                // ★ 无条件保存 AssignedModule（不检查 State）
                // 因为 HandleServerReconnectAsync 可能已将 State 从 Active 改为 Faulted，
                // 但 AssignedModule 不会被 HandleServerReconnectAsync 修改，始终可靠
                if (_pool.TryGetValue(workerId, out var p) && p.AssignedModule != null)
                {
                    p.PendingModuleRestore = p.AssignedModule;
                    Log($"[{workerId}] 💥 进程异常退出 (ExitCode={exitCode})，记住待恢复模块: {p.AssignedModule}");
                }
                else
                {
                    Log($"[{workerId}] 💥 进程异常退出 (ExitCode={exitCode})，2秒后自动重启...");
                }
                _ = Task.Run(() => AutoRestartWorkerAsync(workerId));
            }
            else if (exitCode == 0)
            {
                Log($"[{workerId}] 进程正常退出 (ExitCode=0)");
                if (_pool.TryGetValue(workerId, out var p))
                {
                    p.State = WorkerState.Shutdown;
                    NotifyStateChanged(workerId);
                }
            }
        };

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

        // 注册消息和事件处理器（提取为方法，重连时复用）
        RegisterChannelHandlers(workerId, pooled, channel);
    }

    /// <summary>
    /// Server 侧重连：当管道断开后，重新创建 Server 管道等待 Worker 重连。
    /// Worker 端已启用 AutoReconnect，会自动尝试连接同名管道。
    /// </summary>
    private async Task HandleServerReconnectAsync(string workerId, PooledProcess pooled, string pipeName)
    {
        // 防护：如果进程已被重启替换（Channel 被清空或换成了新的），不做任何状态修改
        if (_disposing) return;
        if (pooled.Channel == null) return;
        if (!_pool.TryGetValue(workerId, out var current) || current != pooled) return;

        var previousState = pooled.State;
        pooled.State = WorkerState.Faulted;
        NotifyStateChanged(workerId);

        try
        {
            // 等待 Worker 发起重连（Worker 端指数退避，首次 500ms）
            // Server 侧创建新管道等待连接，超时 15 秒
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var newChannel = await SimpleIpcChannel.CreateServerAsync(pipeName, cts.Token);

            // 二次防护：等待期间 AutoRestartWorkerAsync 可能已将进程重启并替换了 Channel
            if (pooled.Channel == null || !_pool.TryGetValue(workerId, out var cur) || cur != pooled)
            {
                Log($"[{workerId}] Server 侧重连被取消（进程已被重启替换）");
                await newChannel.DisposeAsync();
                return;
            }

            // 重连成功：替换旧通道
            var oldChannel = pooled.Channel;
            pooled.Channel = newChannel;

            // 重新注册消息处理器
            RegisterChannelHandlers(workerId, pooled, newChannel);

            Log($"[{workerId}] ✅ Server 侧重连成功，恢复状态 → {previousState}");
            pooled.State = previousState;
            NotifyStateChanged(workerId);

            // 释放旧通道
            if (oldChannel != null)
                await oldChannel.DisposeAsync();
        }
        catch (OperationCanceledException)
        {
            Log($"[{workerId}] ❌ Server 侧重连超时（15s），Worker 可能已退出");
            // 保持 Faulted 状态
        }
        catch (Exception ex)
        {
            Log($"[{workerId}] ❌ Server 侧重连失败: {ex.Message}");
        }
    }

    /// <summary>为通道注册消息和事件处理器</summary>
    private void RegisterChannelHandlers(string workerId, PooledProcess pooled, SimpleIpcChannel channel)
    {
        var sw = Stopwatch.StartNew();

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

                    // 崩溃恢复：如果崩溃前是 Active 状态，自动重新激活（恢复窗口显示）
                    if (pooled.PendingModuleRestore != null)
                    {
                        var module = pooled.PendingModuleRestore;
                        pooled.PendingModuleRestore = null;
                        Log($"[{workerId}] 🔄 崩溃恢复：自动重新激活模块 [{module}]（恢复窗口显示）");
                        _ = channel.SendAsync(new IpcMessage
                        {
                            Type = IpcMessageType.ActivateModule,
                            CallId = Guid.NewGuid().ToString("N"),
                            Payload = module
                        });
                    }
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

                case IpcMessageType.ModuleDeactivated:
                    pooled.State = WorkerState.Ready;
                    pooled.AssignedModule = null;
                    pooled.ActivationElapsedMs = 0;
                    Log($"[{workerId}] 🟢 Ready — 模块已回收");
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

        channel.Disconnected += () =>
        {
            Log($"[{workerId}] ⚠️ 管道断开，尝试 Server 侧重连...");
            _ = Task.Run(() => HandleServerReconnectAsync(workerId, pooled, pooled.PipeName));
        };

        channel.ErrorOccurred += ex =>
        {
            Log($"[{workerId}] ❌ 管道错误: {ex.Message}");
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

    /// <summary>
    /// 回收一个 Active 进程：隐藏其窗口，状态退回 Ready，可被再次激活
    /// </summary>
    /// <summary>
    /// 模拟管道断开（演示用）。
    /// 强制关闭某个 Worker 的 Server 端管道，触发双端自动重连。
    /// </summary>
    public async Task<(bool success, string workerId)> SimulateBrokenPipeAsync()
    {
        var pooled = _pool.Values.FirstOrDefault(p =>
            p.State == WorkerState.Ready || p.State == WorkerState.Active);

        if (pooled?.Channel == null)
        {
            Log("无可用的 Worker 进行断管测试");
            return (false, "");
        }

        var workerId = pooled.WorkerId;
        var previousState = pooled.State;
        Log($"[{workerId}] ⚡ 模拟管道断开（当前状态: {previousState}）...");

        // 强制关闭 Server 端管道
        // → Server 端 ReadLoopAsync 退出 → 触发 Disconnected → 触发 HandleServerReconnectAsync
        // → Client 端检测到管道关闭 → 触发 Disconnected → 触发 ReconnectLoopAsync (AutoReconnect)
        await pooled.Channel.BreakConnectionAsync();

        // 给双端重连一些时间（Worker 首次重连延迟 500ms，Server 侧等待 15s）
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(200);
            // 检查是否已恢复连接
            if (pooled.Channel?.IsConnected == true && pooled.State == previousState)
            {
                Log($"[{workerId}] ✅ 双端重连成功，状态恢复: {previousState}");
                return (true, workerId);
            }
        }

        Log($"[{workerId}] ⚠️ 重连可能仍在进行中，当前状态: {pooled.State}");
        return (pooled.State != WorkerState.Faulted, workerId);
    }

    public async Task<bool> DeactivateAsync()
    {
        var pooled = _pool.Values.FirstOrDefault(p => p.State == WorkerState.Active);
        if (pooled?.Channel == null)
        {
            Log("池中无 Active 进程可回收");
            return false;
        }

        Log($"[{pooled.WorkerId}] 回收模块 [{pooled.AssignedModule}]，退回 Ready...");

        // 发送回收指令给 Worker（Worker 会隐藏窗口并确认）
        await pooled.Channel.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.DeactivateModule,
            CallId = Guid.NewGuid().ToString("N"),
        });

        // 等待 Worker 确认隐藏完成（Conceal 是同步执行的，短暂等待即可）
        await Task.Delay(300);
        var confirmed = true;

        if (confirmed)
        {
            var oldModule = pooled.AssignedModule;
            pooled.State = WorkerState.Ready;
            pooled.AssignedModule = null;
            pooled.ActivationElapsedMs = 0;
            Log($"[{pooled.WorkerId}] ✅ 已回收 [{oldModule}]，退回 Ready，可再次激活");
            NotifyStateChanged(pooled.WorkerId);
        }

        return confirmed;
    }

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

    /// <summary>
    /// 自动重启崩溃的 Worker。
    /// 清理旧资源后启动新进程，等待预热完成，恢复 Ready 状态。
    /// </summary>
    private async Task AutoRestartWorkerAsync(string workerId)
    {
        // 等待一小段时间，确保旧进程的管道完全关闭
        await Task.Delay(2000);

        if (_disposing || !_pool.TryGetValue(workerId, out var oldPooled))
            return;

        Log($"[{workerId}] 🔄 开始自动重启...");

        // PendingModuleRestore 已在 Process.Exited 中保存（在 HandleServerReconnectAsync 修改状态之前）
        // 这里不再判断 wasActive，因为状态可能已被 HandleServerReconnectAsync 改为 Faulted

        // 清理旧通道（必须先置空 Channel 再 Dispose，
        // 否则 Dispose 触发的 Disconnected → HandleServerReconnectAsync 检查到 Channel 非空会误执行）
        var oldChannel = oldPooled.Channel;
        oldPooled.Channel = null;
        if (oldChannel != null)
        {
            try { await oldChannel.DisposeAsync(); } catch { }
        }

        // 标记为 WarmingUp
        oldPooled.State = WorkerState.WarmingUp;
        oldPooled.AssignedModule = null;
        oldPooled.ActivationElapsedMs = 0;
        // 注意：不清除 PendingModuleRestore，保留 Process.Exited 中保存的值
        NotifyStateChanged(workerId);

        if (oldPooled.PendingModuleRestore != null)
            Log($"[{workerId}] 崩溃前为 Active 状态（模块: {oldPooled.PendingModuleRestore}），重启后将自动恢复显示");

        // 用新管道名重新启动（复用同一个 workerId）
        var newPipeName = $"SubProcessDemo_{workerId}_{Guid.NewGuid():N}";
        var sw = Stopwatch.StartNew();

        var channelTask = SimpleIpcChannel.CreateServerAsync(newPipeName);

        var startInfo = new ProcessStartInfo
        {
            FileName = _workerExePath,
            Arguments = $"--warmup --pipe={newPipeName} --parent-pid={_parentPid}",
            UseShellExecute = false,
        };

        var newProcess = Process.Start(startInfo);
        if (newProcess == null)
        {
            Log($"[{workerId}] ❌ 重启失败：无法启动新进程");
            oldPooled.State = WorkerState.Faulted;
            NotifyStateChanged(workerId);
            return;
        }

        JobObjectHelper.AddProcess(newProcess);

        // 注册进程退出监控
        newProcess.EnableRaisingEvents = true;
        newProcess.Exited += (s, e) =>
        {
            var exitCode = newProcess.ExitCode;
            if (exitCode != 0 && !_disposing)
            {
                // ★ 无条件保存 AssignedModule（与 WarmUpOneAsync 同样处理）
                if (_pool.TryGetValue(workerId, out var p) && p.AssignedModule != null)
                    p.PendingModuleRestore = p.AssignedModule;
                Log($"[{workerId}] 💥 进程再次异常退出 (ExitCode={exitCode})，2秒后自动重启...");
                _ = Task.Run(() => AutoRestartWorkerAsync(workerId));
            }
        };

        oldPooled.Process = newProcess;
        oldPooled.PipeName = newPipeName;
        oldPooled.CreatedTime = DateTime.Now;

        Log($"[{workerId}] 新进程 PID={newProcess.Id}，等待预热...");

        // 等待 Worker 连接并完成预热
        var channel = await channelTask;
        oldPooled.Channel = channel;
        RegisterChannelHandlers(workerId, oldPooled, channel);

        // RegisterChannelHandlers 中的 Ready 处理器会在 Worker 报告 Ready 时自动更新状态
        Log($"[{workerId}] ✅ 重启完成，新进程已连接");
    }

    /// <summary>
    /// 模拟某个 Worker 崩溃（演示用）。
    /// 通过管道向 Worker 发送 Shutdown 指令，然后强制 Kill 进程。
    /// </summary>
    public async Task<(bool success, string workerId)> SimulateCrashAsync()
    {
        var pooled = _pool.Values.FirstOrDefault(p =>
            p.State == WorkerState.Ready || p.State == WorkerState.Active);

        if (pooled == null)
        {
            Log("无可用的 Worker 进行崩溃模拟");
            return (false, "");
        }

        var workerId = pooled.WorkerId;
        Log($"[{workerId}] 💥 模拟进程崩溃（强制 Kill PID={pooled.Process.Id}）...");

        // 强制终止进程（模拟崩溃，exit code = 1）
        try
        {
            pooled.Process.Kill();
        }
        catch (Exception ex)
        {
            Log($"[{workerId}] Kill 失败: {ex.Message}");
            return (false, workerId);
        }

        // Process.Exited 事件会检测到 exitCode ≠ 0，自动触发 AutoRestartWorkerAsync
        return (true, workerId);
    }

    public async ValueTask DisposeAsync()
    {
        _disposing = true;
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
        /// <summary>崩溃前正在运行的模块（重启预热完成后自动恢复）</summary>
        public string? PendingModuleRestore { get; set; }
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
