using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SubProcessDemo.Common.Config;
using SubProcessDemo.Common.Ipc;
using SubProcessDemo.Common.Models;
using SubProcessDemo.Common.Services;
using SubProcessDemo.Host.Helpers;
using SubProcessDemo.Host.Pool;
using SubProcessDemo.Host.Services;

namespace SubProcessDemo.Host;

public partial class MainWindow : Window
{
    private ProcessPoolManager? _poolManager;
    private ServiceDispatcher? _serviceDispatcher;
    private SharedConfigManager? _configManager;
    private double _coldStartMs;
    private double _warmStartMs;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;

        // 填充模块下拉列表
        CmbModules.ItemsSource = DemoModules.All;
        CmbModules.DisplayMemberPath = "DisplayName";
        CmbModules.SelectedIndex = 0;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Log("主进程启动完成 (PID=" + Environment.ProcessId + ")");

        // 策略②：写入共享内存
        _configManager = new SharedConfigManager(Environment.ProcessId);
        var config = new SharedConfigData();
        var writeMs = _configManager.WriteConfig(config);
        Log($"[策略②] 共享内存写入完成 ({writeMs:F1}ms) — {config.StationNames.Count} 个站点, Token={config.AuthToken}");
    }

    // ═══════════════════════════════════════════════════════════
    // 策略①：初始化预热池
    // ═══════════════════════════════════════════════════════════
    private async void BtnWarmUp_Click(object sender, RoutedEventArgs e)
    {
        BtnWarmUp.IsEnabled = false;
        Log("═══════════════════════════════════════");
        Log("策略①：开始初始化进程预热池");

        // 查找 Worker.exe
        var workerPath = FindWorkerExe();
        if (workerPath == null)
        {
            Log("❌ 未找到 SubProcessDemo.Worker.exe，请先编译项目");
            BtnWarmUp.IsEnabled = true;
            return;
        }
        Log($"Worker 路径: {workerPath}");

        // 策略④：JIT 预编译（在主进程中也演示一下）
        var (methods, jitMs) = JitPrecompiler.PrecompileCriticalPaths();
        Log($"[策略④] JIT 预编译: {methods} 个方法, 耗时 {jitMs:F1}ms");

        _poolManager = new ProcessPoolManager(workerPath);
        _poolManager.OnLog += msg => Dispatcher.Invoke(() => Log(msg));
        _poolManager.OnWorkerStateChanged += (id, info) => Dispatcher.Invoke(() => UpdatePoolUI());

        _serviceDispatcher = new ServiceDispatcher(_poolManager);
        _serviceDispatcher.OnLog += msg => Dispatcher.Invoke(() => Log(msg));

        await _poolManager.WarmUpAsync(3);

        UpdatePoolUI();

        // 启用后续按钮
        BtnActivate.IsEnabled = _poolManager.HasReadyProcess;
        BtnColdStart.IsEnabled = true;
        BtnServiceProxy.IsEnabled = _poolManager.HasActiveProcess;
        BtnTieredWarmUp.IsEnabled = true;

        Log("═══════════════════════════════════════\n");
    }

    // ═══════════════════════════════════════════════════════════
    // 策略①②：激活模块（热启动）
    // ═══════════════════════════════════════════════════════════
    private async void BtnActivate_Click(object sender, RoutedEventArgs e)
    {
        if (_poolManager == null || CmbModules.SelectedItem is not ModuleConfig module) return;

        BtnActivate.IsEnabled = false;
        Log($"策略①：激活模块 [{module.DisplayName}]（热启动路径）");

        var sw = Stopwatch.StartNew();
        var (success, ms) = await _poolManager.ActivateAsync(module.ModuleName);
        sw.Stop();

        if (success)
        {
            _warmStartMs = ms;
            Log($"✅ 热启动完成: {ms:F0}ms（跳过进程创建+CLR+DLL+配置加载）");
            UpdatePerformanceBars();
        }
        else
        {
            Log("❌ 激活失败：无可用 Ready 进程");
        }

        UpdatePoolUI();
        BtnActivate.IsEnabled = _poolManager.HasReadyProcess;
        BtnServiceProxy.IsEnabled = _poolManager.HasActiveProcess;
    }

    // ═══════════════════════════════════════════════════════════
    // 冷启动对比
    // ═══════════════════════════════════════════════════════════
    private async void BtnColdStart_Click(object sender, RoutedEventArgs e)
    {
        if (_poolManager == null || CmbModules.SelectedItem is not ModuleConfig module) return;

        BtnColdStart.IsEnabled = false;
        Log($"冷启动对比：直接创建新进程加载 [{module.DisplayName}]");

        _coldStartMs = await _poolManager.ColdStartAsync(module.ModuleName);

        Log($"冷启动完成: {_coldStartMs:F0}ms（含进程创建+CLR+DLL+配置+服务+UI）");
        UpdatePerformanceBars();

        BtnColdStart.IsEnabled = true;
    }

    // ═══════════════════════════════════════════════════════════
    // 策略③：服务代理测试
    // 向 Active 的 Worker 发送 TestServiceProxy 指令，
    // Worker 通过 ServiceProxy<IDataService> 代理回调主进程
    // ═══════════════════════════════════════════════════════════
    private async void BtnServiceProxy_Click(object sender, RoutedEventArgs e)
    {
        if (_poolManager == null) return;

        BtnServiceProxy.IsEnabled = false;
        Log("策略③：服务代理测试");
        Log("  流程: Host 发送指令 → Worker 创建 ServiceProxy<IDataService>");
        Log("        → Worker 调用接口方法 → DispatchProxy 拦截 → 管道转发到 Host");
        Log("        → Host ServiceDispatcher 反射调用 DataService → 结果沿管道返回 Worker");

        // 监听 Worker 的代理测试结果
        var resultTcs = new TaskCompletionSource<string>();
        _poolManager.OnLog += msg =>
        {
            // ServiceDispatcher 会打日志，结果通过 ServiceResponse 消息返回
        };

        // 向 Active Worker 发送测试指令
        var (sent, workerId) = await _poolManager.SendToAnyActiveWorkerAsync(new IpcMessage
        {
            Type = IpcMessageType.TestServiceProxy,
            Payload = "run_test"
        });

        if (!sent)
        {
            Log("❌ 无 Active 状态的 Worker，请先激活一个模块");
            BtnServiceProxy.IsEnabled = _poolManager.HasReadyProcess || _poolManager.HasActiveProcess;
            return;
        }

        Log($"已发送测试指令到 [{workerId}]，等待 Worker 通过代理回调...");

        // 等待 ServiceDispatcher 的日志输出（通过事件观察）
        // Worker 会依次调用 GetStations/GetUserInfo/GetAlarmCount/GetSystemStatus
        // 每次调用都经过管道，ServiceDispatcher 会打日志
        await Task.Delay(3000); // 给 Worker 时间完成 4 次跨进程调用

        Log("（请查看 Worker 窗口和上方 ServiceDispatcher 日志，观察完整调用链）");

        BtnServiceProxy.IsEnabled = _poolManager.HasReadyProcess || _poolManager.HasActiveProcess;
    }

    // ═══════════════════════════════════════════════════════════
    // 策略⑤：分级预热
    // ═══════════════════════════════════════════════════════════
    private async void BtnTieredWarmUp_Click(object sender, RoutedEventArgs e)
    {
        if (_poolManager == null) return;

        BtnTieredWarmUp.IsEnabled = false;
        Log("═══════════════════════════════════════");
        Log("策略⑤：分级预热调度");

        var scheduler = new TieredWarmUpScheduler(_poolManager, DemoModules.All);
        scheduler.OnLog += msg => Dispatcher.Invoke(() => Log(msg));

        await scheduler.ScheduleAsync();

        UpdatePoolUI();
        Log("═══════════════════════════════════════\n");
        BtnTieredWarmUp.IsEnabled = true;
    }

    // ═══════════════════════════════════════════════════════════
    // 重置
    // ═══════════════════════════════════════════════════════════
    private async void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        if (_poolManager != null)
        {
            Log("重置：关闭所有子进程...");
            await _poolManager.ResetAsync();
            await _poolManager.DisposeAsync();
            _poolManager = null;
        }
        _serviceDispatcher = null;
        _coldStartMs = 0;
        _warmStartMs = 0;

        PoolPanel.Children.Clear();
        LogList.Items.Clear();
        ColdBar.Width = 0;
        WarmBar.Width = 0;
        ColdMsText.Text = "—";
        WarmMsText.Text = "—";

        BtnWarmUp.IsEnabled = true;
        BtnActivate.IsEnabled = false;
        BtnColdStart.IsEnabled = false;
        BtnServiceProxy.IsEnabled = false;
        BtnTieredWarmUp.IsEnabled = false;

        Log("重置完成");
    }

    // ═══════════════════════════════════════════════════════════
    // UI 更新
    // ═══════════════════════════════════════════════════════════
    private void UpdatePoolUI()
    {
        if (_poolManager == null) return;

        PoolPanel.Children.Clear();

        foreach (var worker in _poolManager.Workers)
        {
            var card = CreateWorkerCard(worker);
            PoolPanel.Children.Add(card);
        }
    }

    private Border CreateWorkerCard(WorkerProcessInfo info)
    {
        var (stateColor, stateIcon) = info.State switch
        {
            WorkerState.WarmingUp => (FindResource("Yellow") as SolidColorBrush, "🟡"),
            WorkerState.Ready => (FindResource("Green") as SolidColorBrush, "🟢"),
            WorkerState.Active => (FindResource("Red") as SolidColorBrush, "🔴"),
            WorkerState.Faulted => (FindResource("Red") as SolidColorBrush, "❌"),
            _ => (FindResource("FgSecondary") as SolidColorBrush, "⚪"),
        };

        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };

        // 第一行：名称 + 状态
        var header = new DockPanel();
        header.Children.Add(new TextBlock
        {
            Text = $"{stateIcon} {info.WorkerId}",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = FindResource("FgPrimary") as SolidColorBrush,
        });
        header.Children.Add(new TextBlock
        {
            Text = info.State.ToString(),
            FontSize = 12,
            Foreground = stateColor,
            HorizontalAlignment = HorizontalAlignment.Right,
        });
        stack.Children.Add(header);

        // 第二行：PID + 管道
        stack.Children.Add(new TextBlock
        {
            Text = $"PID: {info.Pid}  |  管道: ...{info.PipeName[^12..]}",
            FontSize = 11,
            Foreground = FindResource("FgSecondary") as SolidColorBrush,
            Margin = new Thickness(0, 2, 0, 0),
        });

        // 第三行：耗时/模块
        var details = new List<string>();
        if (info.WarmupElapsedMs > 0) details.Add($"预热: {info.WarmupElapsedMs:F0}ms");
        if (info.ActivationElapsedMs > 0) details.Add($"激活: {info.ActivationElapsedMs:F0}ms");
        if (info.AssignedModule != null) details.Add($"模块: {info.AssignedModule}");

        if (details.Count > 0)
        {
            stack.Children.Add(new TextBlock
            {
                Text = string.Join("  |  ", details),
                FontSize = 11,
                Foreground = FindResource("FgAccent") as SolidColorBrush,
                Margin = new Thickness(0, 2, 0, 0),
            });
        }

        return new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 8, 10, 8),
            Child = stack,
        };
    }

    private void UpdatePerformanceBars()
    {
        var maxMs = Math.Max(_coldStartMs, _warmStartMs);
        if (maxMs <= 0) return;

        // 最大条宽度 = 控件宽度 - 140（标签和数字）
        var maxBarWidth = Math.Max(300, ColdBar.Parent is FrameworkElement fe ? fe.ActualWidth - 160 : 500);

        if (_coldStartMs > 0)
        {
            ColdBar.Width = (_coldStartMs / maxMs) * maxBarWidth;
            ColdMsText.Text = $"{_coldStartMs:F0}ms";
        }

        if (_warmStartMs > 0)
        {
            WarmBar.Width = (_warmStartMs / maxMs) * maxBarWidth;
            WarmMsText.Text = $"{_warmStartMs:F0}ms";
        }

        // 计算提速百分比
        if (_coldStartMs > 0 && _warmStartMs > 0)
        {
            var speedup = (1 - _warmStartMs / _coldStartMs) * 100;
            Log($"📊 性能对比: 冷启动 {_coldStartMs:F0}ms vs 热启动 {_warmStartMs:F0}ms → 提速 {speedup:F1}%");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 日志
    // ═══════════════════════════════════════════════════════════
    private void Log(string message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        var entry = $"[{time}] {message}";
        LogList.Items.Add(entry);
        LogList.ScrollIntoView(entry);
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        LogList.Items.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    // 辅助
    // ═══════════════════════════════════════════════════════════
    private string? FindWorkerExe()
    {
        // 在构建输出目录中查找 Worker.exe
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 尝试同级目录（Host 和 Worker 在同一解决方案中编译）
        var candidates = new[]
        {
            System.IO.Path.Combine(baseDir, "..", "..", "..", "..", "SubProcessDemo.Worker", "bin", "Debug", "net8.0-windows", "SubProcessDemo.Worker.exe"),
            System.IO.Path.Combine(baseDir, "..", "..", "..", "..", "SubProcessDemo.Worker", "bin", "Release", "net8.0-windows", "SubProcessDemo.Worker.exe"),
            System.IO.Path.Combine(baseDir, "SubProcessDemo.Worker.exe"),
        };

        foreach (var path in candidates)
        {
            var fullPath = System.IO.Path.GetFullPath(path);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_poolManager != null)
        {
            await _poolManager.ShutdownAllAsync();
            await _poolManager.DisposeAsync();
        }
        _configManager?.Dispose();
    }
}
