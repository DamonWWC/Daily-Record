using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using SubProcessDemo.Common.Config;
using SubProcessDemo.Common.Ipc;
using SubProcessDemo.Common.Models;
using SubProcessDemo.Common.Services;
using SubProcessDemo.Worker;

namespace SubProcessDemo.Worker;

public partial class App : Application
{
    private SimpleIpcChannel? _channel;
    private WorkerWindow? _window;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var args = e.Args;

        if (args.Length == 0)
        {
            // 无参数：直接显示窗口进入待机模式（方便独立调试）
            ShowWindow(null, null, null, startHidden: false);
            return;
        }

        var mode = args[0];

        if (mode == "--warmup")
            await RunWarmupMode(args);
        else if (mode == "--cold")
            await RunColdStartMode(args);
    }

    // ═══════════════════════════════════════════════════════════
    // 预热模式
    // ═══════════════════════════════════════════════════════════
    private async Task RunWarmupMode(string[] args)
    {
        var totalSw = Stopwatch.StartNew();

        var pipeName = GetArg(args, "--pipe=") ?? $"demo_pipe_{Guid.NewGuid():N}";
        var parentPid = int.TryParse(GetArg(args, "--parent-pid="), out var pid) ? pid : 0;

        // 预热模式：窗口隐藏启动，激活后才显示
        _window = ShowWindow(pipeName, parentPid, null, startHidden: true);
        _window.AddLog("预热模式启动");

        // 步骤 1：预加载公共程序集
        _window.SetStep("步骤 1/4：预加载公共程序集...", WorkerState.WarmingUp);
        var stepSw = Stopwatch.StartNew();
        PreloadAssemblies();
        stepSw.Stop();
        _window.AddLog($"  ✅ CLR 就绪，公共 DLL 已加载 ({stepSw.ElapsedMilliseconds}ms)");

        // 步骤 2：从共享内存读取配置（策略②）
        _window.SetStep("步骤 2/4：从共享内存读取配置...", WorkerState.WarmingUp);
        double configReadMs = 0;
        if (parentPid > 0)
        {
            var (config, elapsed) = SharedConfigManager.ReadConfig(parentPid);
            configReadMs = elapsed;
            if (config != null)
                _window.AddLog($"  ✅ 获取到 {config.StationNames.Count} 个站点, Token={config.AuthToken[..8]}... ({configReadMs:F1}ms)");
            else
                _window.AddLog("  ⚠️ 共享内存不可用");
        }
        else
        {
            _window.AddLog("  ⚠️ 未指定 parent-pid");
        }

        // 步骤 3：JIT 预编译（策略④）
        _window.SetStep("步骤 3/4：JIT 预编译关键路径...", WorkerState.WarmingUp);
        stepSw.Restart();
        JitPrecompile();
        stepSw.Stop();
        _window.AddLog($"  ✅ JIT 预编译完成 ({stepSw.ElapsedMilliseconds}ms)");

        // 步骤 4：连接命名管道
        _window.SetStep("步骤 4/4：连接命名管道...", WorkerState.WarmingUp);
        stepSw.Restart();
        _channel = await SimpleIpcChannel.CreateClientAsync(pipeName);
        _channel.AutoReconnect = true;  // 启用自动重连
        stepSw.Stop();
        _window.AddLog($"  ✅ 管道已连接 ({stepSw.ElapsedMilliseconds}ms)");

        // 注册管道事件
        _channel.ErrorOccurred += ex =>
        {
            DispatcherInvoke(() => _window.AddLog($"❌ 管道错误: {ex.Message}"));
        };
        _channel.Disconnected += () =>
        {
            DispatcherInvoke(() => _window.AddLog("⚠️ 管道断开，正在自动重连..."));
        };
        _channel.Reconnected += () =>
        {
            DispatcherInvoke(() => _window.AddLog("✅ 管道已重连，通信恢复"));
        };

        totalSw.Stop();
        var warmupMs = totalSw.Elapsed.TotalMilliseconds;

        _window.SetState(WorkerState.Ready, warmupMs);
        _window.AddLog($"═══ 预热完成，报告 Ready（总耗时 {warmupMs:F0}ms）═══");

        await _channel.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.Ready,
            Payload = JsonSerializer.Serialize(new { WarmupMs = warmupMs, ConfigReadMs = configReadMs })
        });

        // ── 监听主进程指令（所有处理器均有 try-catch 防止进程崩溃） ──
        _channel.MessageReceived += msg =>
        {
            // 每种消息类型都在独立 Task 中处理，不阻塞读取循环
            _ = Task.Run(async () =>
            {
                try
                {
                    switch (msg.Type)
                    {
                        case IpcMessageType.ActivateModule:
                            await HandleActivation(msg);
                            break;
                        case IpcMessageType.DeactivateModule:
                            await HandleDeactivation(msg);
                            break;
                        case IpcMessageType.Shutdown:
                            Dispatcher.Invoke(() => Shutdown());
                            break;
                        case IpcMessageType.HealthCheck:
                            await _channel.SendAsync(new IpcMessage { Type = IpcMessageType.HealthReport, Payload = "OK" });
                            break;
                        case IpcMessageType.TestServiceProxy:
                            await HandleServiceProxyTest();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => _window.AddLog($"❌ 处理 {msg.Type} 异常: {ex.GetType().Name}: {ex.Message}"));
                }
            });
        };
    }

    // ═══════════════════════════════════════════════════════════
    // 处理模块激活
    // ═══════════════════════════════════════════════════════════
    private async Task HandleActivation(IpcMessage msg)
    {
        var moduleName = msg.Payload ?? "Unknown";

        Dispatcher.Invoke(() =>
        {
            _window!.SetState(WorkerState.Active, 0, moduleName);
            _window.AddLog($"收到激活指令：加载模块 [{moduleName}]");
        });

        var sw = Stopwatch.StartNew();
        var module = DemoModules.All.FirstOrDefault(m => m.ModuleName == moduleName);
        var loadMs = module?.SimulatedLoadMs ?? 200;

        await ModuleSimulator.SimulateWarmActivation(
            moduleName, loadMs,
            log => Dispatcher.Invoke(() => _window!.AddLog($"  {log}")));

        sw.Stop();

        Dispatcher.Invoke(() => _window!.AddLog($"模块 [{moduleName}] 激活完成 ({sw.ElapsedMilliseconds}ms)"));

        await _channel!.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.ModuleActivated,
            CallId = msg.CallId,
            Payload = JsonSerializer.Serialize(new { ModuleName = moduleName, ActivationMs = sw.Elapsed.TotalMilliseconds })
        });
    }

    // ═══════════════════════════════════════════════════════════
    // 处理模块回收（Active → Ready，隐藏窗口）
    // ═══════════════════════════════════════════════════════════
    private async Task HandleDeactivation(IpcMessage msg)
    {
        DispatcherInvoke(() =>
        {
            _window!.AddLog("收到回收指令：隐藏窗口，退回 Ready 状态");
            _window.Conceal();
        });

        // 向主进程确认已退回 Ready
        await _channel!.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.ModuleDeactivated,
            CallId = msg.CallId,
        });
    }

    // ═══════════════════════════════════════════════════════════
    // 策略③：服务代理测试
    // ═══════════════════════════════════════════════════════════
    private async Task HandleServiceProxyTest()
    {
        DispatcherInvoke(() =>
        {
            _window!.AddLog("════════ 策略③：服务代理测试 ════════");
            _window.AddLog("创建 ServiceProxy<IDataService>（DispatchProxy 代理）");
            _window.AddLog("所有方法调用将通过命名管道转发到主进程执行");
        });

        // 创建服务代理
        var dataService = ServiceProxy.Create<IDataService>(_channel!);
        var totalSw = Stopwatch.StartNew();

        try
        {
            // ── 调用 1：GetStations() ──
            DispatcherInvoke(() => _window!.AddLog("调用 dataService.GetStations()..."));
            var sw = Stopwatch.StartNew();
            var stations = await Task.Run(() => dataService.GetStations());
            sw.Stop();
            DispatcherInvoke(() => _window!.AddLog(
                $"  → 返回 {stations.Count} 个站点: [{string.Join(", ", stations.Take(3))}...] ({sw.ElapsedMilliseconds}ms)"));

            // ── 调用 2：GetUserInfo(string) ──
            DispatcherInvoke(() => _window!.AddLog("调用 dataService.GetUserInfo(\"U001\")..."));
            sw.Restart();
            var userInfo = await Task.Run(() => dataService.GetUserInfo("U001"));
            sw.Stop();
            DispatcherInvoke(() => _window!.AddLog(
                $"  → 返回: {userInfo} ({sw.ElapsedMilliseconds}ms)"));

            // ── 调用 3：GetAlarmCount() ──
            DispatcherInvoke(() => _window!.AddLog("调用 dataService.GetAlarmCount()..."));
            sw.Restart();
            var alarmCount = await Task.Run(() => dataService.GetAlarmCount());
            sw.Stop();
            DispatcherInvoke(() => _window!.AddLog(
                $"  → 返回: {alarmCount} 条告警 ({sw.ElapsedMilliseconds}ms)"));

            // ── 调用 4：GetSystemStatus() ──
            DispatcherInvoke(() => _window!.AddLog("调用 dataService.GetSystemStatus()..."));
            sw.Restart();
            var status = await Task.Run(() => dataService.GetSystemStatus());
            sw.Stop();
            var statusStr = string.Join(", ", status.Select(kv => $"{kv.Key}={kv.Value}"));
            DispatcherInvoke(() => _window!.AddLog(
                $"  → 返回: {{{statusStr}}} ({sw.ElapsedMilliseconds}ms)"));

            totalSw.Stop();
            DispatcherInvoke(() => _window!.AddLog(
                $"════════ 代理测试完成（4 次跨进程调用, 总计 {totalSw.ElapsedMilliseconds}ms）════════"));
        }
        catch (Exception ex)
        {
            totalSw.Stop();
            DispatcherInvoke(() => _window!.AddLog(
                $"❌ 代理调用失败 ({totalSw.ElapsedMilliseconds}ms): {ex.GetType().Name}: {ex.Message}"));
            if (ex.InnerException != null)
                DispatcherInvoke(() => _window!.AddLog(
                    $"   内部异常: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}"));
        }

        // 向主进程报告结果
        await _channel!.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.ServiceResponse,
            CallId = "proxy_test_done",
            Payload = JsonSerializer.Serialize(new { TotalMs = totalSw.ElapsedMilliseconds })
        });
    }

    /// <summary>安全的 Dispatcher.Invoke，异常不会导致进程崩溃</summary>
    private void DispatcherInvoke(Action action)
    {
        try { Dispatcher.Invoke(action); }
        catch (Exception ex) { Debug.WriteLine($"DispatcherInvoke 异常: {ex.Message}"); }
    }

    // ═══════════════════════════════════════════════════════════
    // 冷启动模式（对比用）
    // ═══════════════════════════════════════════════════════════
    private async Task RunColdStartMode(string[] args)
    {
        var totalSw = Stopwatch.StartNew();
        var moduleName = GetArg(args, "--module=") ?? "StationModule";
        var pipeName = GetArg(args, "--pipe=");

        _window = ShowWindow(null, null, moduleName, startHidden: false);
        _window.AddLog($"冷启动模式：加载模块 [{moduleName}]");

        await ModuleSimulator.SimulateColdStart(
            moduleName,
            log => DispatcherInvoke(() => _window.AddLog($"  {log}")));

        totalSw.Stop();
        _window.AddLog($"冷启动完成（总耗时 {totalSw.Elapsed.TotalMilliseconds:F0}ms）");

        if (pipeName != null)
        {
            var channel = await SimpleIpcChannel.CreateClientAsync(pipeName);
            await channel.SendAsync(new IpcMessage
            {
                Type = IpcMessageType.ColdStartResult,
                Payload = JsonSerializer.Serialize(new { ModuleName = moduleName, TotalMs = totalSw.Elapsed.TotalMilliseconds })
            });
            await Task.Delay(500);
        }

        Shutdown();
    }

    // ═══════════════════════════════════════════════════════════
    // 辅助
    // ═══════════════════════════════════════════════════════════
    private WorkerWindow ShowWindow(string? pipeName, int? parentPid, string? coldModule, bool startHidden)
    {
        var win = new WorkerWindow(pipeName, parentPid, coldModule, startHidden);
        if (!startHidden)
        {
            win.Show(); // 只有非隐藏模式才立即显示
        }
        // 隐藏模式下不调用 Show()，窗口不会出现在屏幕上
        // 等到 Reveal() 时才 Show()
        return win;
    }

    private static string? GetArg(string[] args, string prefix)
        => args.FirstOrDefault(a => a.StartsWith(prefix))?[prefix.Length..];

    private static void PreloadAssemblies()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try { _ = asm.GetExportedTypes(); } catch { }
        }
        Thread.Sleep(300);
    }

    private static void JitPrecompile()
    {
        var types = new[]
        {
            typeof(JsonSerializer),
            typeof(System.Collections.Concurrent.ConcurrentDictionary<,>),
            typeof(System.IO.Pipes.NamedPipeClientStream),
        };
        foreach (var type in types)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                try
                {
                    if (!method.IsAbstract && !method.ContainsGenericParameters)
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                }
                catch { }
            }
        }
        Thread.Sleep(150);
    }
}
