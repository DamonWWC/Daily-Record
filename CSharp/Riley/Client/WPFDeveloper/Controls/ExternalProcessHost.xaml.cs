using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WPFDeveloper.Services;
using WPFDeveloper.Utils;

namespace WPFDeveloper.Controls
{
    /// <summary>
    /// 外部进程宿主控件
    /// 提供UI界面用于启动、嵌入和管理外部EXE程序
    /// </summary>
    public partial class ExternalProcessHost : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty ExecutablePathProperty =
            DependencyProperty.Register(nameof(ExecutablePath), typeof(string), typeof(ExternalProcessHost),
                new PropertyMetadata(string.Empty, OnExecutablePathChanged));

        public static readonly DependencyProperty StartupArgumentsProperty =
            DependencyProperty.Register(nameof(StartupArguments), typeof(string), typeof(ExternalProcessHost),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsProcessRunningProperty =
            DependencyProperty.Register(nameof(IsProcessRunning), typeof(bool), typeof(ExternalProcessHost),
                new PropertyMetadata(false, OnProcessRunningChanged));

        public static readonly DependencyProperty CanStartProcessProperty =
            DependencyProperty.Register(nameof(CanStartProcess), typeof(bool), typeof(ExternalProcessHost),
                new PropertyMetadata(true));

        public static readonly DependencyProperty CanStopProcessProperty =
            DependencyProperty.Register(nameof(CanStopProcess), typeof(bool), typeof(ExternalProcessHost),
                new PropertyMetadata(false));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(ExternalProcessHost),
                new PropertyMetadata("就绪"));

        public static readonly DependencyProperty StatusColorProperty =
            DependencyProperty.Register(nameof(StatusColor), typeof(System.Windows.Media.Brush), typeof(ExternalProcessHost),
                new PropertyMetadata(System.Windows.Media.Brushes.Gray));

        public static readonly DependencyProperty ProcessInfoProperty =
            DependencyProperty.Register(nameof(ProcessInfo), typeof(string), typeof(ExternalProcessHost),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProcessUptimeProperty =
            DependencyProperty.Register(nameof(ProcessUptime), typeof(string), typeof(ExternalProcessHost),
                new PropertyMetadata(string.Empty));

        #endregion

        #region 属性

        public string ExecutablePath
        {
            get => (string)GetValue(ExecutablePathProperty);
            set => SetValue(ExecutablePathProperty, value);
        }

        public string StartupArguments
        {
            get => (string)GetValue(StartupArgumentsProperty);
            set => SetValue(StartupArgumentsProperty, value);
        }

        public bool IsProcessRunning
        {
            get => (bool)GetValue(IsProcessRunningProperty);
            private set => SetValue(IsProcessRunningProperty, value);
        }

        public bool CanStartProcess
        {
            get => (bool)GetValue(CanStartProcessProperty);
            private set => SetValue(CanStartProcessProperty, value);
        }

        public bool CanStopProcess
        {
            get => (bool)GetValue(CanStopProcessProperty);
            private set => SetValue(CanStopProcessProperty, value);
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            private set => SetValue(StatusTextProperty, value);
        }

        public System.Windows.Media.Brush StatusColor
        {
            get => (System.Windows.Media.Brush)GetValue(StatusColorProperty);
            private set => SetValue(StatusColorProperty, value);
        }

        public string ProcessInfo
        {
            get => (string)GetValue(ProcessInfoProperty);
            private set => SetValue(ProcessInfoProperty, value);
        }

        public string ProcessUptime
        {
            get => (string)GetValue(ProcessUptimeProperty);
            private set => SetValue(ProcessUptimeProperty, value);
        }

        #endregion

        #region 字段

        private readonly IExternalProcessHostService _processHostService;
        private readonly ILogger<ExternalProcessHost> _logger;
        private Process? _currentProcess;
        private DispatcherTimer? _uptimeTimer;
        private DateTime _processStartTime;

        #endregion

        #region 构造函数

        public ExternalProcessHost()
        {
            InitializeComponent();
            DataContext = this;

            // 从依赖注入容器获取服务
            _processHostService = (App.ServiceProvider.GetService(typeof(IExternalProcessHostService)) as IExternalProcessHostService)!;
            if (_processHostService == null)
                throw new InvalidOperationException("无法获取 IExternalProcessHostService 服务");
            
            _logger = (App.ServiceProvider.GetService(typeof(ILogger<ExternalProcessHost>)) as ILogger<ExternalProcessHost>)!;
            if (_logger == null)
                throw new InvalidOperationException("无法获取 ILogger<ExternalProcessHost> 服务");

            // 订阅服务事件
            _processHostService.ProcessStarted += OnProcessStarted;
            _processHostService.ProcessExited += OnProcessExited;
            _processHostService.ProcessEmbedFailed += OnProcessEmbedFailed;

            // 初始化定时器
            _uptimeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _uptimeTimer.Tick += UpdateProcessUptime;

            Loaded += OnControlLoaded;
            Unloaded += OnControlUnloaded;
        }

        #endregion

        #region 事件处理

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            UpdateCanStartProcess();
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            // 清理资源
            StopCurrentProcess();
            _uptimeTimer?.Stop();

            // 取消订阅事件
            if (_processHostService != null)
            {
                _processHostService.ProcessStarted -= OnProcessStarted;
                _processHostService.ProcessExited -= OnProcessExited;
                _processHostService.ProcessEmbedFailed -= OnProcessEmbedFailed;
            }
        }

        private void QuickSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var recommendedPrograms = SystemProgramPaths.GetRecommendedTestPrograms();
                
                if (!recommendedPrograms.Any())
                {
                    MessageBox.Show("未找到可用的系统程序。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 创建选择对话框
                var dialog = new Window
                {
                    Title = "选择系统程序",
                    Width = 500,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this),
                    ResizeMode = ResizeMode.CanResize
                };

                var listBox = new ListBox
                {
                    Margin = new Thickness(10),
                    DisplayMemberPath = "DisplayText"
                };

                // 添加程序项
                foreach (var program in recommendedPrograms)
                {
                    listBox.Items.Add(new
                    {
                        Name = program.Name,
                        Path = program.Path,
                        Description = program.Description,
                        DisplayText = $"{program.Name} - {program.Description}"
                    });
                }

                var panel = new DockPanel();
                
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(10)
                };
                DockPanel.SetDock(buttonPanel, Dock.Bottom);

                var okButton = new Button
                {
                    Content = "确定",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(5, 0, 0, 0),
                    IsDefault = true
                };

                var cancelButton = new Button
                {
                    Content = "取消",
                    Width = 80,
                    Height = 30,
                    IsCancel = true
                };

                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(okButton);
                panel.Children.Add(buttonPanel);
                panel.Children.Add(listBox);

                dialog.Content = panel;

                okButton.Click += (s, args) =>
                {
                    if (listBox.SelectedItem != null)
                    {
                        dynamic selectedItem = listBox.SelectedItem;
                        ExecutablePath = selectedItem.Path;
                        dialog.DialogResult = true;
                    }
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "快速选择程序时发生异常");
                MessageBox.Show($"快速选择失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择要嵌入的程序",
                Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExecutablePath = openFileDialog.FileName;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ExecutablePath))
                {
                    MessageBox.Show("请先选择要启动的程序路径。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!File.Exists(ExecutablePath))
                {
                    MessageBox.Show("指定的程序文件不存在，请检查路径是否正确。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SetStatus("正在启动程序...", System.Windows.Media.Brushes.Orange);
                CanStartProcess = false;

                // 获取宿主区域的窗口句柄
                var hostHandle = GetHostWindowHandle();
                if (hostHandle == IntPtr.Zero)
                {
                    SetStatus("无法获取宿主窗口句柄", System.Windows.Media.Brushes.Red);
                    CanStartProcess = true;
                    return;
                }

                // 启动并嵌入进程
                _currentProcess = await _processHostService.StartAndEmbedProcessAsync(
                    ExecutablePath, hostHandle, StartupArguments);

                if (_currentProcess == null)
                {
                    SetStatus("启动程序失败", System.Windows.Media.Brushes.Red);
                    CanStartProcess = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动程序时发生异常");
                SetStatus($"启动失败: {ex.Message}", System.Windows.Media.Brushes.Red);
                CanStartProcess = true;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopCurrentProcess();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                try
                {
                    // 重新调整嵌入进程的窗口大小
                    var hostArea = ProcessHostArea;
                    var width = (int)hostArea.ActualWidth;
                    var height = (int)hostArea.ActualHeight;
                    
                    _processHostService.ResizeEmbeddedProcess(_currentProcess, 0, 0, width, height);
                    
                    SetStatus("已刷新程序窗口", System.Windows.Media.Brushes.Green);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "刷新程序窗口时发生异常");
                    SetStatus($"刷新失败: {ex.Message}", System.Windows.Media.Brushes.Red);
                }
            }
        }

        private void OnProcessStarted(object? sender, ProcessStartedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsProcessRunning = true;
                CanStartProcess = false;
                CanStopProcess = true;
                _processStartTime = e.StartTime;
                
                ProcessInfo = $"PID: {e.Process.Id}";
                SetStatus("程序运行中", System.Windows.Media.Brushes.Green);

                // 启动运行时间计时器
                _uptimeTimer?.Start();

                // 调整嵌入进程窗口大小以适应宿主区域
                SizeChanged += OnHostSizeChanged;
                ResizeEmbeddedWindow();
            });
        }

        private void OnProcessExited(object? sender, ProcessExitedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsProcessRunning = false;
                CanStartProcess = true;
                CanStopProcess = false;
                ProcessInfo = string.Empty;
                ProcessUptime = string.Empty;
                
                SetStatus($"程序已退出 (退出代码: {e.ExitCode})", System.Windows.Media.Brushes.Gray);

                // 停止计时器
                _uptimeTimer?.Stop();
                SizeChanged -= OnHostSizeChanged;

                _currentProcess = null;
            });
        }

        private void OnProcessEmbedFailed(object? sender, ProcessEmbedFailedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                SetStatus($"嵌入失败: {e.ErrorMessage}", System.Windows.Media.Brushes.Red);
                CanStartProcess = true;
            });
        }

        private void OnHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeEmbeddedWindow();
        }

        private void UpdateProcessUptime(object? sender, EventArgs e)
        {
            if (IsProcessRunning)
            {
                var uptime = DateTime.Now - _processStartTime;
                ProcessUptime = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            }
        }

        private static void OnExecutablePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ExternalProcessHost host)
            {
                host.UpdateCanStartProcess();
            }
        }

        private static void OnProcessRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 可以在这里添加额外的UI更新逻辑
        }

        #endregion

        #region 私有方法

        private IntPtr GetHostWindowHandle()
        {
            try
            {
                var source = PresentationSource.FromVisual(ProcessHostArea) as HwndSource;
                return source?.Handle ?? IntPtr.Zero;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取宿主窗口句柄时发生异常");
                return IntPtr.Zero;
            }
        }

        private void ResizeEmbeddedWindow()
        {
            if (_currentProcess != null && !_currentProcess.HasExited && ProcessHostArea.ActualWidth > 0 && ProcessHostArea.ActualHeight > 0)
            {
                var width = (int)ProcessHostArea.ActualWidth;
                var height = (int)ProcessHostArea.ActualHeight;
                
                _processHostService.ResizeEmbeddedProcess(_currentProcess, 0, 0, width, height);
            }
        }

        private void StopCurrentProcess()
        {
            if (_currentProcess != null)
            {
                try
                {
                    SetStatus("正在停止程序...", System.Windows.Media.Brushes.Orange);
                    _processHostService.StopEmbeddedProcess(_currentProcess);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "停止程序时发生异常");
                    SetStatus($"停止失败: {ex.Message}", System.Windows.Media.Brushes.Red);
                }
            }
        }

        private void UpdateCanStartProcess()
        {
            CanStartProcess = !IsProcessRunning && !string.IsNullOrWhiteSpace(ExecutablePath);
        }

        private void SetStatus(string text, System.Windows.Media.Brush color)
        {
            StatusText = text;
            StatusColor = color;
            _logger.LogInformation("状态更新: {StatusText}", text);
        }

        #endregion
    }
}
