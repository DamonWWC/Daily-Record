using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    /// <summary>
    /// 专门用于实时视频流帧截取的类，支持在流播放后指定时间或帧数后截取图片
    /// </summary>
    public class LiveStreamFrameCapture : IDisposable
    {
        private readonly string _videoSource;
        private VideoCapture? _capture;
        private Thread? _captureThread;
        private bool _isCapturing;
        private bool _isDisposed;
        private DateTime _streamStartTime;
        private long _frameCount;
        private double _fps;
        
        // 截图任务队列
        private readonly ConcurrentQueue<CaptureTask> _captureTasks = new();
        private readonly AutoResetEvent _taskEvent = new(false);

        /// <summary>
        /// 流是否正在运行
        /// </summary>
        public bool IsRunning => _isCapturing && _capture?.IsOpened() == true;

        /// <summary>
        /// 当前帧数
        /// </summary>
        public long CurrentFrameCount => _frameCount;

        /// <summary>
        /// 流开始时间
        /// </summary>
        public DateTime StreamStartTime => _streamStartTime;

        /// <summary>
        /// 视频帧率
        /// </summary>
        public double Fps => _fps;

        /// <summary>
        /// 流运行时长（秒）
        /// </summary>
        public double ElapsedSeconds => IsRunning ? (DateTime.Now - _streamStartTime).TotalSeconds : 0;

        public LiveStreamFrameCapture(string videoSource)
        {
            _videoSource = videoSource ?? throw new ArgumentNullException(nameof(videoSource));
        }

        /// <summary>
        /// 开始监听视频流
        /// </summary>
        /// <param name="timeoutSeconds">连接超时时间（秒）</param>
        /// <returns>是否成功开始监听</returns>
        public async Task<bool> StartAsync(int timeoutSeconds = 10)
        {
            if (_isCapturing)
                return true;

            try
            {
                _capture = new VideoCapture(_videoSource);
                
                // 测试连接
                var connectionTask = Task.Run(() =>
                {
                    if (!_capture.IsOpened())
                        return false;

                    // 尝试读取第一帧来确保连接成功
                    using var testFrame = new Mat();
                    return _capture.Read(testFrame) && !testFrame.Empty();
                });

                bool connected = await connectionTask.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds));
                if (!connected)
                {
                    Console.WriteLine("无法连接到视频流");
                    return false;
                }

                // 获取帧率
                _fps = _capture.Get(VideoCaptureProperties.Fps);
                if (_fps <= 0) _fps = 25; // 默认25fps

                _streamStartTime = DateTime.Now;
                _frameCount = 0;
                _isCapturing = true;

                // 启动捕获线程
                _captureThread = new Thread(CaptureLoop)
                {
                    IsBackground = true,
                    Name = "LiveStreamCapture"
                };
                _captureThread.Start();

                Console.WriteLine($"视频流监听已启动，帧率: {_fps:F2} fps");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动视频流监听失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 停止监听视频流
        /// </summary>
        public void Stop()
        {
            if (!_isCapturing)
                return;

            _isCapturing = false;
            _taskEvent.Set(); // 唤醒线程以便退出

            _captureThread?.Join(5000); // 等待最多5秒
            
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;

            Console.WriteLine("视频流监听已停止");
        }

        /// <summary>
        /// 在指定秒数后截取帧
        /// </summary>
        /// <param name="delaySeconds">延迟秒数（从流开始算起）</param>
        /// <param name="outputPath">输出路径</param>
        /// <param name="width">输出宽度（-1表示保持原始宽度）</param>
        /// <param name="height">输出高度（-1表示保持原始高度）</param>
        /// <returns>任务ID，可用于取消任务</returns>
        public string CaptureAfterSeconds(double delaySeconds, string outputPath, int width = -1, int height = -1)
        {
            if (!IsRunning)
                throw new InvalidOperationException("视频流未在运行，请先调用StartAsync()");

            var task = new CaptureTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = CaptureTaskType.AfterSeconds,
                TriggerValue = delaySeconds,
                OutputPath = outputPath,
                Width = width,
                Height = height,
                CreatedTime = DateTime.Now
            };

            _captureTasks.Enqueue(task);
            _taskEvent.Set();

            Console.WriteLine($"已添加截图任务: {delaySeconds}秒后截取帧到 {outputPath}");
            return task.Id;
        }

        /// <summary>
        /// 在指定帧数后截取帧
        /// </summary>
        /// <param name="frameNumber">帧数（从流开始算起）</param>
        /// <param name="outputPath">输出路径</param>
        /// <param name="width">输出宽度（-1表示保持原始宽度）</param>
        /// <param name="height">输出高度（-1表示保持原始高度）</param>
        /// <returns>任务ID，可用于取消任务</returns>
        public string CaptureAfterFrames(long frameNumber, string outputPath, int width = -1, int height = -1)
        {
            if (!IsRunning)
                throw new InvalidOperationException("视频流未在运行，请先调用StartAsync()");

            var task = new CaptureTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = CaptureTaskType.AfterFrames,
                TriggerValue = frameNumber,
                OutputPath = outputPath,
                Width = width,
                Height = height,
                CreatedTime = DateTime.Now
            };

            _captureTasks.Enqueue(task);
            _taskEvent.Set();

            Console.WriteLine($"已添加截图任务: 第{frameNumber}帧时截取到 {outputPath}");
            return task.Id;
        }

        /// <summary>
        /// 立即截取当前帧
        /// </summary>
        /// <param name="outputPath">输出路径</param>
        /// <param name="width">输出宽度（-1表示保持原始宽度）</param>
        /// <param name="height">输出高度（-1表示保持原始高度）</param>
        /// <returns>是否成功截取</returns>
        public async Task<bool> CaptureNowAsync(string outputPath, int width = -1, int height = -1)
        {
            if (!IsRunning)
                return false;

            var task = new CaptureTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = CaptureTaskType.Immediate,
                TriggerValue = 0,
                OutputPath = outputPath,
                Width = width,
                Height = height,
                CreatedTime = DateTime.Now
            };

            _captureTasks.Enqueue(task);
            _taskEvent.Set();

            // 等待任务完成
            while (!task.IsCompleted && IsRunning)
            {
                await Task.Delay(50);
            }

            return task.IsSuccess;
        }

        /// <summary>
        /// 按间隔持续截取帧
        /// </summary>
        /// <param name="intervalSeconds">截取间隔（秒）</param>
        /// <param name="outputFolder">输出文件夹</param>
        /// <param name="maxFrames">最大截取帧数（0表示无限制）</param>
        /// <param name="width">输出宽度</param>
        /// <param name="height">输出高度</param>
        /// <param name="cancellationToken">取消标记</param>
        /// <returns>截取任务</returns>
        public Task StartContinuousCaptureAsync(double intervalSeconds, string outputFolder, 
            int maxFrames = 0, int width = -1, int height = -1, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                Directory.CreateDirectory(outputFolder);
                int captureCount = 0;
                DateTime lastCaptureTime = DateTime.Now;

                while (IsRunning && !cancellationToken.IsCancellationRequested)
                {
                    if (maxFrames > 0 && captureCount >= maxFrames)
                        break;

                    var elapsed = (DateTime.Now - lastCaptureTime).TotalSeconds;
                    if (elapsed >= intervalSeconds)
                    {
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                        var outputPath = Path.Combine(outputFolder, $"continuous_{timestamp}.jpg");
                        
                        if (await CaptureNowAsync(outputPath, width, height))
                        {
                            captureCount++;
                            lastCaptureTime = DateTime.Now;
                        }
                    }

                    await Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 获取流信息
        /// </summary>
        public StreamInfo GetStreamInfo()
        {
            if (!IsRunning)
                return new StreamInfo();

            return new StreamInfo
            {
                IsRunning = IsRunning,
                Fps = _fps,
                CurrentFrame = _frameCount,
                ElapsedSeconds = ElapsedSeconds,
                StartTime = _streamStartTime,
                VideoSource = _videoSource
            };
        }

        /// <summary>
        /// 捕获线程主循环
        /// </summary>
        private void CaptureLoop()
        {
            var currentFrame = new Mat();
            var pendingTasks = new List<CaptureTask>();

            try
            {
                while (_isCapturing && _capture?.IsOpened() == true)
                {
                    // 读取帧
                    if (!_capture.Read(currentFrame) || currentFrame.Empty())
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    _frameCount++;
                    var currentTime = DateTime.Now;
                    var elapsedSeconds = (currentTime - _streamStartTime).TotalSeconds;

                    // 获取新任务
                    while (_captureTasks.TryDequeue(out var newTask))
                    {
                        pendingTasks.Add(newTask);
                    }

                    // 检查待执行的任务
                    for (int i = pendingTasks.Count - 1; i >= 0; i--)
                    {
                        var task = pendingTasks[i];
                        bool shouldCapture = false;

                        switch (task.Type)
                        {
                            case CaptureTaskType.Immediate:
                                shouldCapture = true;
                                break;
                            case CaptureTaskType.AfterSeconds:
                                shouldCapture = elapsedSeconds >= task.TriggerValue;
                                break;
                            case CaptureTaskType.AfterFrames:
                                shouldCapture = _frameCount >= task.TriggerValue;
                                break;
                        }

                        if (shouldCapture)
                        {
                            task.IsSuccess = ExecuteCaptureTask(task, currentFrame);
                            task.IsCompleted = true;
                            pendingTasks.RemoveAt(i);
                            
                            Console.WriteLine($"截图任务完成: {task.OutputPath}, 成功: {task.IsSuccess}");
                        }
                    }

                    // 控制帧率，避免CPU占用过高
                    Thread.Sleep(Math.Max(1, (int)(1000 / _fps / 2)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"捕获线程异常: {ex.Message}");
            }
            finally
            {
                currentFrame?.Dispose();
                
                // 标记所有未完成的任务为失败
                foreach (var task in pendingTasks)
                {
                    task.IsSuccess = false;
                    task.IsCompleted = true;
                }
            }
        }

        /// <summary>
        /// 执行截图任务
        /// </summary>
        private bool ExecuteCaptureTask(CaptureTask task, Mat frame)
        {
            try
            {
                CreateOutputDirectory(task.OutputPath);

                var outputFrame = frame.Clone();

                // 调整大小
                if (task.Width > 0 && task.Height > 0)
                {
                    Cv2.Resize(outputFrame, outputFrame, new Size(task.Width, task.Height));
                }

                // 保存图片
                string extension = Path.GetExtension(task.OutputPath).ToLower();
                bool success = false;

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    success = Cv2.ImWrite(task.OutputPath, outputFrame, 
                        new ImageEncodingParam(ImwriteFlags.JpegQuality, 95));
                }
                else if (extension == ".png")
                {
                    success = Cv2.ImWrite(task.OutputPath, outputFrame, 
                        new ImageEncodingParam(ImwriteFlags.PngCompression, 9));
                }
                else
                {
                    success = Cv2.ImWrite(task.OutputPath, outputFrame);
                }

                outputFrame.Dispose();
                return success && File.Exists(task.OutputPath) && new FileInfo(task.OutputPath).Length > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行截图任务失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建输出目录
        /// </summary>
        private void CreateOutputDirectory(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Stop();
                    _taskEvent?.Dispose();
                }
                _isDisposed = true;
            }
        }

        ~LiveStreamFrameCapture()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// 截图任务类型
    /// </summary>
    public enum CaptureTaskType
    {
        /// <summary>立即截图</summary>
        Immediate,
        /// <summary>指定秒数后截图</summary>
        AfterSeconds,
        /// <summary>指定帧数后截图</summary>
        AfterFrames
    }

    /// <summary>
    /// 截图任务
    /// </summary>
    public class CaptureTask
    {
        public string Id { get; set; } = string.Empty;
        public CaptureTaskType Type { get; set; }
        public double TriggerValue { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public int Width { get; set; } = -1;
        public int Height { get; set; } = -1;
        public DateTime CreatedTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSuccess { get; set; }
    }

    /// <summary>
    /// 流信息
    /// </summary>
    public class StreamInfo
    {
        public bool IsRunning { get; set; }
        public double Fps { get; set; }
        public long CurrentFrame { get; set; }
        public double ElapsedSeconds { get; set; }
        public DateTime StartTime { get; set; }
        public string VideoSource { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Stream: {VideoSource}, Running: {IsRunning}, FPS: {Fps:F2}, " +
                   $"Frame: {CurrentFrame}, Elapsed: {ElapsedSeconds:F2}s";
        }
    }
}