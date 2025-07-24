using Flyleaf.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Flyleaf
{
    /// <summary>
    /// FlyleafVideoNewControl.xaml 的交互逻辑
    /// 优化后的视频控件，使用单例模式的动态加载器
    /// </summary>
    public partial class FlyleafVideoNewControl : UserControl, IDisposable
    {
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
        /// 摄像头列表
        /// </summary>
        public List<string> Cameras { get; set; } = new List<string>();

        /// <summary>
        /// 摄像头信息
        /// </summary>
        public string? CameraInfo { get; set; }

        /// <summary>
        /// 当前播放状态
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// 当前播放的URL
        /// </summary>
        public string? CurrentUrl { get; private set; }

        private readonly FrameworkElement? _videoControl;
        private readonly DynamicControlLoader _controlLoader;
        private bool _disposed = false;

        #endregion 属性和字段

        #region 构造函数和初始化

        public FlyleafVideoNewControl()
        {
            InitializeComponent();

            _controlLoader = DynamicControlLoader.Instance;
            _videoControl = InitializeVideoControl();

            if (_videoControl != null)
            {
                maingrid.Children.Add(_videoControl);
                LogInfo("FlyleafVideoNewControl initialized successfully");
            }
            else
            {
                LogError("Failed to initialize video control");
                ShowErrorMessage("视频控件初始化失败", "无法加载视频播放组件，请检查FlyleafLib1.dll是否存在。");
            }
        }

        /// <summary>
        /// 初始化视频控件
        /// </summary>
        /// <returns>视频控件实例</returns>
        private FrameworkElement? InitializeVideoControl()
        {
            try
            {
                var control = _controlLoader.CreateLiveControl();
                if (control == null)
                {
                    LogError("Failed to create LiveControl instance");
                    return null;
                }

                LogInfo("Video control created successfully");
                return control;
            }
            catch (Exception ex)
            {
                LogError($"Exception during video control initialization: {ex.Message}", ex);
                return null;
            }
        }

        #endregion 构造函数和初始化

        #region 播放控制方法

        /// <summary>
        /// 开始播放视频
        /// </summary>
        /// <param name="url">视频URL</param>
        public async Task<bool> StartPlayAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                LogError("Cannot start playback - URL is null or empty");
                return false;
            }

            if (_videoControl == null)
            {
                LogError("Cannot start playback - video control is not initialized");
                return false;
            }

            try
            {
                LogInfo($"Starting playback for URL: {url}");

                var stopwatch = Stopwatch.StartNew();

                // TODO: 实现摄像头获取逻辑
                // await LoadCameraInfoAsync();

                stopwatch.Stop();

                // 计算剩余延迟时间
                int remainingDelay = Math.Max(0, DelayPlayTimes - (int)stopwatch.Elapsed.TotalMilliseconds);
                if (remainingDelay > 0)
                {
                    await Task.Delay(remainingDelay);
                }

                // 设置视频URL开始播放
                bool success = _controlLoader.SetCameraUrl(_videoControl, url);

                if (success)
                {
                    CurrentUrl = url;
                    IsPlaying = true;
                    PlayAction?.Invoke(true);
                    LogInfo($"Playback started successfully for: {url}");
                    return true;
                }
                else
                {
                    LogError("Failed to set camera URL");
                    PlayAction?.Invoke(false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception during playback start: {ex.Message}", ex);
                PlayAction?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// 开始播放视频（同步版本，保持向后兼容）
        /// </summary>
        /// <param name="url">视频URL</param>
        public async void StartPlay(string url)
        {
            await StartPlayAsync(url);
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public bool StopPlay()
        {
            if (_videoControl == null)
            {
                LogError("Cannot stop playback - video control is not initialized");
                return false;
            }

            try
            {
                LogInfo("Stopping playback");

                bool success = _controlLoader.SetCameraUrl(_videoControl, null);

                if (success)
                {
                    CurrentUrl = null;
                    IsPlaying = false;
                    PlayAction?.Invoke(false);
                    LogInfo("Playback stopped successfully");
                    return true;
                }
                else
                {
                    LogError("Failed to stop playback");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception during playback stop: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 暂停/恢复播放
        /// </summary>
        public bool TogglePlayback()
        {
            if (IsPlaying)
            {
                return StopPlay();
            }
            else if (!string.IsNullOrEmpty(CurrentUrl))
            {
                StartPlay(CurrentUrl);
                return true;
            }
            return false;
        }

        #endregion 播放控制方法

        #region 截图功能

        /// <summary>
        /// 截取当前视频帧
        /// </summary>
        /// <param name="fileName">保存文件名，为null时使用默认名称</param>
        /// <returns>是否截图成功</returns>
        public bool TakeSnapshot(string? fileName = null)
        {
            if (_videoControl == null)
            {
                LogError("Cannot take snapshot - video control is not initialized");
                return false;
            }

            if (!IsPlaying)
            {
                LogError("Cannot take snapshot - no video is playing");
                return false;
            }

            try
            {
                bool success = _controlLoader.TakeSnapshot(_videoControl, fileName);
                if (success)
                {
                    LogInfo($"Snapshot taken: {fileName ?? "default"}");
                }
                return success;
            }
            catch (Exception ex)
            {
                LogError($"Exception during snapshot: {ex.Message}", ex);
                return false;
            }
        }

        #endregion 截图功能

        #region 辅助方法

        /// <summary>
        /// 记录信息日志
        /// </summary>
        private void LogInfo(string message)
        {
            Debug.WriteLine($"[FlyleafVideoNewControl] INFO: {message}");
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private void LogError(string message, Exception? ex = null)
        {
            Debug.WriteLine($"[FlyleafVideoNewControl] ERROR: {message}");
            if (ex != null)
            {
                Debug.WriteLine($"[FlyleafVideoNewControl] EXCEPTION: {ex}");
            }
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        private void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion 辅助方法

        #region IDisposable实现

        public void Dispose()
        {
            if (!_disposed)
            {
                StopPlay();

                // 清理资源
                if (_videoControl != null && maingrid.Children.Contains(_videoControl))
                {
                    maingrid.Children.Remove(_videoControl);
                }

                _disposed = true;
                LogInfo("FlyleafVideoNewControl disposed");
            }
        }

        #endregion IDisposable实现
    }
}