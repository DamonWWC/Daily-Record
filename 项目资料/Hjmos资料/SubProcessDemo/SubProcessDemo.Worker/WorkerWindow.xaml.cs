using System.Windows;
using System.Windows.Media;
using SubProcessDemo.Common.Models;

namespace SubProcessDemo.Worker;

public partial class WorkerWindow : Window
{
    private readonly List<string> _pendingLogs = new();
    private bool _isVisible;

    public WorkerWindow(string? pipeName, int? parentPid, string? coldModule, bool startHidden = true)
    {
        InitializeComponent();

        // 启动时隐藏窗口，仅 Active 状态才显示
        if (startHidden)
        {
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;
            _isVisible = false;
        }
        else
        {
            _isVisible = true;
        }

        PidText.Text = $"PID: {Environment.ProcessId}";
        PipeText.Text = pipeName != null ? $"管道: ...{pipeName[^12..]}" : "独立模式";

        if (coldModule != null)
        {
            Title = $"Worker — 冷启动 [{coldModule}]";
            TitleText.Text = $"冷启动: {coldModule}";
            StepText.Text = "模拟完整冷启动流程...";
            SetStateBadge(WorkerState.WarmingUp);
        }
        else
        {
            Title = $"Worker-{Environment.ProcessId} — 预热中";
            TitleText.Text = "Worker 子进程 — 预热中";
        }
    }

    /// <summary>显示窗口并刷新缓冲的日志（首次调用时才让窗口出现在屏幕上）</summary>
    public void Reveal()
    {
        if (_isVisible) return;

        Dispatcher.Invoke(() =>
        {
            // 首次显示窗口
            Show();
            WindowStyle = WindowStyle.SingleBorderWindow;
            ShowInTaskbar = true;
            Visibility = Visibility.Visible;
            Activate(); // 带到前台
            _isVisible = true;

            // 刷新隐藏期间积累的日志
            foreach (var entry in _pendingLogs)
            {
                LogList.Items.Add(entry);
            }
            _pendingLogs.Clear();

            if (LogList.Items.Count > 0)
                LogList.ScrollIntoView(LogList.Items[^1]);
        });
    }

    /// <summary>添加一条日志（窗口隐藏时缓冲，显示后立即写入）</summary>
    public void AddLog(string message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        var entry = $"[{time}] {message}";

        if (_isVisible)
        {
            LogList.Items.Add(entry);
            LogList.ScrollIntoView(LogList.Items[^1]);
        }
        else
        {
            _pendingLogs.Add(entry);
        }
    }

    /// <summary>隐藏窗口，退回待命状态（Active → Ready）</summary>
    public void Conceal()
    {
        Dispatcher.Invoke(() =>
        {
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
            _isVisible = false;
            _pendingLogs.Clear();

            // 重置界面状态
            TitleText.Text = "Worker 子进程 — 就绪待命";
            StepText.Text = "等待主进程发送激活指令...";
            SetStateBadge(WorkerState.Ready);
        });
    }

    /// <summary>更新当前步骤文本</summary>
    public void SetStep(string text, WorkerState state)
    {
        StepText.Text = text;
        SetStateBadge(state);
    }

    /// <summary>更新状态</summary>
    public void SetState(WorkerState state, double elapsedMs, string? moduleName = null)
    {
        SetStateBadge(state);

        if (state == WorkerState.Ready)
        {
            Title = $"Worker-{Environment.ProcessId} — Ready";
            TitleText.Text = "Worker 子进程 — 就绪待命";
            StepText.Text = "等待主进程发送激活指令...";
            ElapsedText.Text = $"预热耗时: {elapsedMs:F0}ms";
        }
        else if (state == WorkerState.Active)
        {
            Title = $"Worker-{Environment.ProcessId} — Active [{moduleName}]";
            TitleText.Text = $"Worker 子进程 — 运行中";
            StepText.Text = $"已激活模块: {moduleName}";
            if (elapsedMs > 0)
                ElapsedText.Text = $"激活耗时: {elapsedMs:F0}ms";

            // 激活时显示窗口
            Reveal();
        }
    }

    private void SetStateBadge(WorkerState state)
    {
        var (bg, fg, text) = state switch
        {
            WorkerState.WarmingUp => (FindResource("Yellow") as SolidColorBrush, new SolidColorBrush(Color.FromRgb(30, 30, 46)), "🟡 WarmingUp"),
            WorkerState.Ready     => (FindResource("Green")  as SolidColorBrush, new SolidColorBrush(Color.FromRgb(30, 30, 46)), "🟢 Ready"),
            WorkerState.Active    => (FindResource("Red")    as SolidColorBrush, new SolidColorBrush(Color.FromRgb(255, 255, 255)), "🔴 Active"),
            WorkerState.Faulted   => (FindResource("Red")    as SolidColorBrush, new SolidColorBrush(Color.FromRgb(255, 255, 255)), "❌ Faulted"),
            _                     => (FindResource("FgSecondary") as SolidColorBrush, new SolidColorBrush(Color.FromRgb(30, 30, 46)), "⚪ Unknown"),
        };

        StateBadge.Background = bg!;
        StateText.Foreground = fg!;
        StateText.Text = text;
    }

    /// <summary>模拟崩溃按钮点击</summary>
    private void BtnCrash_Click(object sender, RoutedEventArgs e)
    {
        AddLog("💥 模拟未处理异常 → 进程即将崩溃");
        // 模拟未处理异常导致的进程崩溃
        // 使用 Environment.Exit(1) 模拟非正常退出（exit code ≠ 0）
        Task.Run(() =>
        {
            Thread.Sleep(200); // 让日志有时间刷出
            Environment.Exit(1);
        });
    }
}
