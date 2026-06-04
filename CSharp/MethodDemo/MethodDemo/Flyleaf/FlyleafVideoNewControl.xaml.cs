using Flyleaf.Common;
using Flyleaf.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Flyleaf
{
    /// <summary>
    /// 优化的视频控件，使用单例模式动态加载器
    /// 增强了错误处理、日志记录和资源管理功能
    /// </summary>
    public partial class FlyleafVideoNewControl : UserControl, IDisposable
    {
        #region 依赖属性

        /// <summary>
        /// 是否自动截图
        /// </summary>
        public bool IsAutoSnapShot
        {
            get => (bool)GetValue(IsAutoSnapShotProperty);
            set => SetValue(IsAutoSnapShotProperty, value);
        }

        public static readonly DependencyProperty IsAutoSnapShotProperty =
            DependencyProperty.Register(nameof(IsAutoSnapShot), typeof(bool), typeof(FlyleafVideoNewControl),
                new PropertyMetadata(false));

        public bool EnableAutoRetry
        {
            get { return (bool)GetValue(EnableAutoRetryProperty); }
            set { SetValue(EnableAutoRetryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableAutoRetry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableAutoRetryProperty =
            DependencyProperty.Register("EnableAutoRetry", typeof(bool), typeof(FlyleafVideoNewControl), new PropertyMetadata(default));

        public int MaxRetryAttempts
        {
            get { return (int)GetValue(MaxRetryAttemptsProperty); }
            set { SetValue(MaxRetryAttemptsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxRetryAttempts.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxRetryAttemptsProperty =
            DependencyProperty.Register("MaxRetryAttempts", typeof(int), typeof(FlyleafVideoNewControl), new PropertyMetadata(3));

        /// <summary>
        /// 摄像头信息或视频源地址
        /// </summary>
        public string CameraInfo
        {
            get => (string)GetValue(CameraInfoProperty);
            set => SetValue(CameraInfoProperty, value);
        }

        public static readonly DependencyProperty CameraInfoProperty =
            DependencyProperty.Register(nameof(CameraInfo), typeof(string), typeof(FlyleafVideoNewControl),
                new PropertyMetadata(string.Empty, OnCameraInfoChanged));

        /// <summary>
        /// 摄像头信息变更处理
        /// </summary>
        private static void OnCameraInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlyleafVideoNewControl control && e.NewValue is string newValue)
            {
                _ = control.HandleCameraInfoChangedAsync(newValue);
            }
        }

        #endregion 依赖属性

        #region 属性和字段

        /// <summary>
        /// 播放延迟时间（毫秒）
        /// </summary>
        public int DelayPlayTimes { get; set; } = 200;

        /// <summary>
        /// 播放状态变化回调
        /// </summary>
        public Action<bool>? PlayAction { get; set; }

        /// <summary>
        /// 摄像头URL列表
        /// </summary>
        public List<string> Cameras { get; private set; } = new();

        /// <summary>
        /// 当前摄像头信息
        /// </summary>
        public string CurrentCameraInfo { get; private set; }

        /// <summary>
        /// 当前播放状态
        /// </summary>
        public Status CurrentStatus { get; private set; } = Status.Stopped;

        // 私有字段
        private readonly ILiveControl? _liveControl;

        private readonly DynamicControlLoader1 _controlLoader;
        private readonly ILogger<FlyleafVideoNewControl>? _logger;

        private int _retryCount = 0;
        private bool _disposed = false;
        private bool _isInitialized = false;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly object _lockObject = new object();

        #endregion 属性和字段

        #region 构造函数和初始化

        public FlyleafVideoNewControl()
        {
            InitializeComponent();

            // 初始化控件加载器和视频控件
            _controlLoader = DynamicControlLoader1.Instance;
            _liveControl = CreateLiveControl();
            //_liveControl = new LiveControl();

            //if (_liveControl is FrameworkElement element)
            //{
            //    maingrid.Children.Add(element);
            //    LogInfo("视频控件已创建并添加到UI");
            //    //return liveControl;
            //}
            // 设置事件处理器
            this.Loaded += FlyleafVideoNewControl_Loaded;
            this.Unloaded += FlyleafVideoNewControl_Unloaded;

            LogInfo("FlyleafVideoNewControl 已初始化");
        }

        /// <summary>
        /// 创建并配置视频控件实例
        /// </summary>
        private ILiveControl? CreateLiveControl()
        {
            try
            {
                var videoControl = _controlLoader.CreateLiveControl();
                if (videoControl is ILiveControl liveControl)
                {
                    liveControl.StatusAction += OnStatusChanged;
                    liveControl.OpenCompleted += OnOpenCompleted;

                    if (videoControl is FrameworkElement element)
                    {
                        maingrid.Children.Add(element);
                        LogInfo("视频控件已创建并添加到UI");
                        return liveControl;
                    }
                }

                LogError("创建视频控件失败 - 类型无效");
                return null;
            }
            catch (Exception ex)
            {
                LogError($"创建视频控件时发生异常: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 控件加载事件处理
        /// </summary>
        private void FlyleafVideoNewControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _isInitialized = true;

                LogInfo("控件已加载");
            }
            catch (Exception ex)
            {
                LogError($"控件加载过程中发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 控件卸载事件处理
        /// </summary>
        private void FlyleafVideoNewControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                StopPlay();
                LogInfo("控件已卸载");
            }
            catch (Exception ex)
            {
                LogError($"控件卸载过程中发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 处理摄像头信息依赖属性变更
        /// </summary>
        private async Task HandleCameraInfoChangedAsync(string newCameraInfo)
        {
            try
            {             
                if (!string.IsNullOrWhiteSpace(newCameraInfo))
                {
                    await StartPlayAsync(newCameraInfo);
                }
                else
                {
                    StopPlay();
                }
            }
            catch (Exception ex)
            {
                LogError($"处理摄像头信息变更时发生错误: {ex.Message}", ex);
            }
        }

        #endregion 构造函数和初始化

        #region 事件处理器

        /// <summary>
        /// 处理播放状态变更
        /// </summary>
        private async void OnStatusChanged(Status status)
        {
            try
            {
                lock (_lockObject)
                {
                    CurrentStatus = status;
                }

                LogInfo($"状态变更为: {status}");

                switch (status)
                {
                    case Status.Playing:
                        await HandlePlayingStatusAsync();
                        break;

                    case Status.Failed:
                        await HandleFailedStatusAsync();
                        break;

                    case Status.Stopped:
                        HandleStoppedStatus();
                        break;

                    case Status.Opening:
                        LogInfo("正在打开视频流...");
                        break;
                }

                // 通知订阅者
                bool isPlaying = status == Status.Playing;
                PlayAction?.Invoke(isPlaying);
            }
            catch (Exception ex)
            {
                LogError($"处理状态变更时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 处理播放状态
        /// </summary>
        private async Task HandlePlayingStatusAsync()
        {
            lock (_lockObject)
            {
                _retryCount = 0; // 成功播放时重置重试计数
            }

            if (IsAutoSnapShot)
            {
                try
                {
                    // 播放开始后延迟一段时间再截图，确保画面稳定
                    await Task.Delay(3000, _cancellationTokenSource?.Token ?? CancellationToken.None);

                    if (!_disposed && GetStatus() == Status.Playing)
                    {
                        TakeSnapshot();
                    }
                }
                catch (OperationCanceledException)
                {
                    // 控件被释放时的预期异常
                    LogInfo("自动截图操作被取消");
                }
                catch (Exception ex)
                {
                    LogError($"自动截图时发生错误: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 处理播放失败状态
        /// </summary>
        private async Task HandleFailedStatusAsync()
        {
            if (!EnableAutoRetry)
            {
                LogError("播放失败，自动重试已禁用");
                return;
            }

            int currentRetryCount;
            lock (_lockObject)
            {
                if (_retryCount >= MaxRetryAttempts)
                {
                    _retryCount = 0;
                    LogError($"播放失败，已达到最大重试次数 {MaxRetryAttempts}");
                    return;
                }

                _retryCount++;
                currentRetryCount = _retryCount;
            }

            LogInfo($"播放失败，正在重试 ({currentRetryCount}/{MaxRetryAttempts})...");

            try
            {
                await Task.Delay(500, _cancellationTokenSource?.Token ?? CancellationToken.None);

                if (!_disposed && !string.IsNullOrWhiteSpace(CurrentCameraInfo))
                {
                    await StartPlayAsync(CurrentCameraInfo);
                }
            }
            catch (OperationCanceledException)
            {
                LogInfo("重试操作被取消");
            }
            catch (Exception ex)
            {
                LogError($"重试过程中发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 处理停止状态
        /// </summary>
        private void HandleStoppedStatus()
        {
            lock (_lockObject)
            {
                _retryCount = 0;
            }
            CurrentCameraInfo = null;
            LogInfo("播放已停止");
        }

        /// <summary>
        /// 处理打开完成事件
        /// </summary>
        private void OnOpenCompleted(bool success)
        {
            if (success)
            {
                LogInfo("视频流打开成功");
            }
            else
            {
                LogError("视频流打开失败");
            }
        }

        #endregion 事件处理器

        #region 播放控制方法

        /// <summary>
        /// 从摄像头信息获取摄像头URL列表
        /// </summary>
        private async Task<List<string>> GetCamerasAsync(string cameraInfo)
        {
            try
            {
                // 模拟异步操作
                await Task.Delay(50, _cancellationTokenSource?.Token ?? CancellationToken.None);

                //if (string.IsNullOrWhiteSpace(cameraInfo))
                //{
                //    return new List<string>(_config.DefaultVideoUrls);
                //}

                // TODO: 在这里可以实现更复杂的摄像头URL解析逻辑
                // 例如从数据库或配置文件中根据摄像头ID获取对应的URL

                // 否则返回默认URL
                return new List<string>()
                {
                    "站台扶梯.mp4",
                     "rtmp://ns8.indexforce.com/home/mystream",
                    
                   

                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError($"获取摄像头URL时发生错误: {ex.Message}", ex);
                return new List<string>()
                {
                    "rtmp://ns8.indexforce.com/home/mystream1",
                    "站台扶梯1.mp4"
                };
            }
        }

        /// <summary>
        /// 启动视频播放
        /// </summary>
        /// <param name="cameraInfo">摄像头信息或URL</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> StartPlayAsync(string? cameraInfo = null)
        {
            if (_liveControl == null)
            {
                LogError("无法启动播放 - 视频控件为空");
                return false;
            }

            if (_disposed)
            {
                LogError("无法启动播放 - 控件已释放");
                return false;
            }

            try
            {
                CurrentCameraInfo = cameraInfo ?? CurrentCameraInfo;
                if (string.IsNullOrWhiteSpace(CurrentCameraInfo))
                {
                    LogError("无法启动播放 - 未提供摄像头信息");
                    return false;
                }

                LogInfo($"开始播放，摄像头信息: {CurrentCameraInfo}");

                var stopwatch = Stopwatch.StartNew();

                // 获取摄像头URL列表
                Cameras = await GetCamerasAsync(CurrentCameraInfo);
                if (!Cameras.Any())
                {
                    LogError("没有可用的摄像头URL");
                    return false;
                }

                stopwatch.Stop();

                // 应用播放延迟
                int remainingDelay = Math.Max(0, DelayPlayTimes - (int)stopwatch.Elapsed.TotalMilliseconds);
                if (remainingDelay > 0)
                {
                    await Task.Delay(remainingDelay, _cancellationTokenSource?.Token ?? CancellationToken.None);
                }

                // 设置摄像头URL并开始播放
                string primaryUrl = Cameras.First();
                bool success = _controlLoader.SetCameraUrl(_liveControl, primaryUrl);

                if (success)
                {
                    LogInfo($"播放启动成功，URL: {primaryUrl}");
                    return true;
                }
                else
                {
                    LogError("设置摄像头URL失败");
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                LogInfo("播放启动操作被取消");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"启动播放时发生错误: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止视频播放
        /// </summary>
        /// <returns>是否停止成功</returns>
        public bool StopPlay()
        {
            if (_liveControl == null)
            {
                return true; // 已经停止
            }

            try
            {
                LogInfo("正在停止播放");

                bool success = _controlLoader.SetCameraUrl(_liveControl, null);
                if (success)
                {
                    CurrentCameraInfo = null;
                    LogInfo("播放停止成功");
                }
                else
                {
                    LogError("停止播放失败");
                }

                return success;
            }
            catch (Exception ex)
            {
                LogError($"停止播放时发生错误: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取当前播放状态
        /// </summary>
        /// <returns>当前播放状态</returns>
        public Status GetStatus()
        {
            return _liveControl?.GetStatus() ?? Status.Stopped;
        }

        #endregion 播放控制方法

        #region 截图功能

        /// <summary>
        /// 截取当前视频帧
        /// </summary>
        /// <param name="fileName">保存文件名，null为使用默认名称</param>
        /// <returns>是否截图成功</returns>
        public bool TakeSnapshot(string? fileName = null)
        {
            if (_liveControl == null)
            {
                LogError("无法截图 - 视频控件为空");
                return false;
            }

            if (_liveControl.GetStatus() != Status.Playing)
            {
                LogError("无法截图 - 视频未在播放");
                return false;
            }

            try
            {
                // 使用配置的截图文件名（如果未提供）
                string snapshotFileName = fileName;

                bool success = _controlLoader.TakeSnapshot(_liveControl, snapshotFileName);

                if (success)
                {
                    LogInfo($"截图成功: {snapshotFileName}");
                }
                else
                {
                    LogError("截图失败");
                }

                return success;
            }
            catch (Exception ex)
            {
                LogError($"截图时发生错误: {ex.Message}", ex);
                return false;
            }
        }
        public string GetSnapshotFileName()
        {
            try
            {
                // Ensure snapshot directory exists
                if (!Directory.Exists("Snapshots"))
                {
                    Directory.CreateDirectory("Snapshots");
                }

                string fileName = string.Format("snapshot_{0:yyyyMMdd_HHmmss}.png", DateTime.Now);
                return Path.Combine("Snapshots", fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating snapshot filename: {ex.Message}");
                return $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            }
        }

        #endregion 截图功能

        #region 日志记录

        /// <summary>
        /// 记录信息日志
        /// </summary>
        private void LogInfo(string message)
        {
           
            {
                _logger?.LogInformation(message);
                Debug.WriteLine($"[FlyleafVideoNewControl] 信息: {message}");
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private void LogError(string message, Exception? ex = null)
        {
            _logger?.LogError(ex, message);
            Debug.WriteLine($"[FlyleafVideoNewControl] 错误: {message}");
            if (ex != null)
            {
                Debug.WriteLine($"[FlyleafVideoNewControl] 异常详情: {ex}");
            }
        }

        #endregion 日志记录

        #region IDisposable 实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                LogInfo("正在释放 FlyleafVideoNewControl 资源");

                // 取消所有正在进行的操作
                _cancellationTokenSource?.Cancel();

                // 停止播放
                StopPlay();

                // 取消事件订阅
                if (_liveControl != null)
                {
                    _liveControl.StatusAction -= OnStatusChanged;
                    _liveControl.OpenCompleted -= OnOpenCompleted;
                }

                // 清理UI
                maingrid?.Children.Clear();

                // 释放取消令牌源
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                _disposed = true;
                LogInfo("FlyleafVideoNewControl 资源释放完成");
            }
            catch (Exception ex)
            {
                LogError($"释放资源时发生错误: {ex.Message}", ex);
            }
        }

        #endregion IDisposable 实现
    }
}