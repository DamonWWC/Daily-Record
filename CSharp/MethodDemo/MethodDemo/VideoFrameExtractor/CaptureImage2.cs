using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FlyleafLib;
using FlyleafLib.MediaPlayer;

namespace VideoFrameExtractor
{
    /// <summary>
    /// Utility class for capturing frames from video sources using Flyleaf library
    /// Supports RTSP streams and local video files with advanced frame extraction capabilities
    /// </summary>
    public class CaptureImage2 : IDisposable
    {
        private Player _player;
        private bool _isDisposed;
        private readonly object _lockObject = new object();
        private Config _config;

        /// <summary>
        /// Initializes a new instance of the CaptureImage2 class
        /// </summary>
        public CaptureImage2()
        {
            InitializeFlyleaf();
        }

        /// <summary>
        /// Initializes Flyleaf engine and creates a player instance
        /// </summary>
        private void InitializeFlyleaf()
        {
            try
            {
                // Initialize Flyleaf engine
                Engine.Start(new EngineConfig()
                {
                    FFmpegPath = null, // Use system FFmpeg or bundled
                    FFmpegDevices = false,
                    FFmpegLogLevel = FFmpegLogLevel.Warning,
                    LogLevel = LogLevel.Quiet,
                    LogOutput = null
                });

                // Create player configuration
                _config = new Config()
                {
                    Player = new PlayerConfig()
                    {
                        AutoPlay = false,
                        SeekAccurate = true,
                        MaxVideoFrames = 3
                    },
                    Video = new VideoConfig()
                    {
                        Enabled = true,
                        MaxWidth = 0, // No limit
                        MaxHeight = 0 // No limit
                    },
                    Audio = new AudioConfig()
                    {
                        Enabled = false // We don't need audio for frame capture
                    },
                    Subtitles = new SubtitlesConfig()
                    {
                        Enabled = false // We don't need subtitles
                    }
                };

                // Create player instance
                _player = new Player(_config);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize Flyleaf: {ex.Message}", ex);
            }
        }

        #region Single Frame Capture

        /// <summary>
        /// Captures a single frame from a video source at a specific time position
        /// </summary>
        /// <param name="videoSource">Video source URL or file path</param>
        /// <param name="outputPath">Output image path</param>
        /// <param name="timePosition">Time position to capture (in seconds)</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds</param>
        /// <param name="width">Output image width (0 = original)</param>
        /// <param name="height">Output image height (0 = original)</param>
        /// <returns>Whether the operation was successful</returns>
        public bool CaptureFrame(string videoSource, string outputPath, double timePosition = 0,
            int timeoutSeconds = 30, int width = 0, int height = 0)
        {
            if (string.IsNullOrEmpty(videoSource) || string.IsNullOrEmpty(outputPath))
                return false;

            CreateOutputDirectory(outputPath);

            lock (_lockObject)
            {
                try
                {
                    // Open the video source
                    var openResult = OpenVideoSource(videoSource, timeoutSeconds);
                    if (!openResult)
                    {
                        Console.WriteLine($"Failed to open video source: {videoSource}");
                        return false;
                    }

                    // For local files, seek to the specified position
                    if (!IsStreamSource(videoSource) && timePosition > 0)
                    {
                        long seekTicks = (long)(timePosition * 10000000); // Convert to ticks (100ns units)
                        _player.Seek(seekTicks);
                        
                        // Wait for seek to complete
                        Thread.Sleep(500);
                    }

                    // Wait for a frame to be available
                    if (!WaitForFrame(timeoutSeconds))
                    {
                        Console.WriteLine("No frame available for capture");
                        return false;
                    }

                    // Capture the current frame
                    var bitmap = CaptureCurrentFrame();
                    if (bitmap == null)
                    {
                        Console.WriteLine("Failed to capture frame");
                        return false;
                    }

                    // Resize if needed
                    if (width > 0 && height > 0)
                    {
                        bitmap = ResizeBitmap(bitmap, width, height);
                    }

                    // Save the frame
                    SaveBitmap(bitmap, outputPath);
                    bitmap.Dispose();

                    return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error capturing frame: {ex.Message}");
                    return false;
                }
                finally
                {
                    _player?.Stop();
                }
            }
        }

        /// <summary>
        /// Asynchronously captures a single frame from a video source
        /// </summary>
        public async Task<bool> CaptureFrameAsync(string videoSource, string outputPath, double timePosition = 0,
            int timeoutSeconds = 30, int width = 0, int height = 0)
        {
            return await Task.Run(() => CaptureFrame(videoSource, outputPath, timePosition, timeoutSeconds, width, height));
        }

        #endregion

        #region Multiple Frames Capture

        /// <summary>
        /// Captures multiple frames from a video source at fixed intervals
        /// </summary>
        /// <param name="videoSource">Video source URL or file path</param>
        /// <param name="outputFolder">Output folder</param>
        /// <param name="captureInterval">Capture interval in seconds</param>
        /// <param name="totalDuration">Total duration in seconds (0 = entire video for local files)</param>
        /// <param name="startPosition">Start position in seconds</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds</param>
        /// <param name="width">Output image width (0 = original)</param>
        /// <param name="height">Output image height (0 = original)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully captured frames</returns>
        public async Task<int> CaptureMultipleFramesAsync(string videoSource, string outputFolder,
            double captureInterval, double totalDuration = 0, double startPosition = 0,
            int timeoutSeconds = 30, int width = 0, int height = 0,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(videoSource) || string.IsNullOrEmpty(outputFolder) || captureInterval <= 0)
                return 0;

            CreateOutputDirectory(outputFolder);
            bool isStream = IsStreamSource(videoSource);
            int count = 0;

            try
            {
                if (isStream)
                {
                    // For streams, capture frames in real-time
                    return await CaptureStreamFramesAsync(videoSource, outputFolder, captureInterval,
                        totalDuration, timeoutSeconds, width, height, cancellationToken);
                }
                else
                {
                    // For local files, extract frames at specific timestamps
                    return await CaptureFileFramesAsync(videoSource, outputFolder, captureInterval,
                        totalDuration, startPosition, timeoutSeconds, width, height, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing multiple frames: {ex.Message}");
                return count;
            }
        }

        /// <summary>
        /// Captures frames from a live stream at regular intervals
        /// </summary>
        private async Task<int> CaptureStreamFramesAsync(string streamUrl, string outputFolder,
            double captureInterval, double totalDuration, int timeoutSeconds,
            int width, int height, CancellationToken cancellationToken)
        {
            int count = 0;
            DateTime startTime = DateTime.Now;

            // Default duration for streams if not specified
            if (totalDuration <= 0)
                totalDuration = 60;

            int totalFrames = (int)Math.Ceiling(totalDuration / captureInterval);

            lock (_lockObject)
            {
                try
                {
                    // Open the stream
                    if (!OpenVideoSource(streamUrl, timeoutSeconds))
                    {
                        Console.WriteLine($"Failed to open stream: {streamUrl}");
                        return 0;
                    }

                    for (int i = 0; i < totalFrames && !cancellationToken.IsCancellationRequested; i++)
                    {
                        try
                        {
                            // Wait for the next capture time
                            var elapsedTime = DateTime.Now - startTime;
                            var nextCaptureTime = TimeSpan.FromSeconds(captureInterval * i);
                            var waitTime = nextCaptureTime - elapsedTime;

                            if (waitTime > TimeSpan.Zero)
                            {
                                await Task.Delay(waitTime, cancellationToken);
                            }

                            // Capture frame
                            if (WaitForFrame(5)) // Shorter timeout for stream frames
                            {
                                var bitmap = CaptureCurrentFrame();
                                if (bitmap != null)
                                {
                                    if (width > 0 && height > 0)
                                    {
                                        bitmap = ResizeBitmap(bitmap, width, height);
                                    }

                                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                                    var outputPath = Path.Combine(outputFolder, $"stream_frame_{timestamp}.jpg");
                                    SaveBitmap(bitmap, outputPath);
                                    bitmap.Dispose();
                                    count++;
                                }
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error capturing stream frame {i}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    _player?.Stop();
                }
            }

            return count;
        }

        /// <summary>
        /// Captures frames from a local video file at specific timestamps
        /// </summary>
        private async Task<int> CaptureFileFramesAsync(string videoPath, string outputFolder,
            double captureInterval, double totalDuration, double startPosition,
            int timeoutSeconds, int width, int height, CancellationToken cancellationToken)
        {
            int count = 0;

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        // Open the video file
                        if (!OpenVideoSource(videoPath, timeoutSeconds))
                        {
                            Console.WriteLine($"Failed to open video file: {videoPath}");
                            return;
                        }

                        // Get video duration
                        double videoDuration = _player.Duration / 10000000.0; // Convert from ticks to seconds

                        // Calculate effective duration
                        if (totalDuration <= 0)
                            totalDuration = videoDuration - startPosition;
                        else
                            totalDuration = Math.Min(totalDuration, videoDuration - startPosition);

                        if (totalDuration <= 0)
                        {
                            Console.WriteLine("Invalid duration or start position");
                            return;
                        }

                        int totalFrames = (int)Math.Ceiling(totalDuration / captureInterval);

                        for (int i = 0; i < totalFrames && !cancellationToken.IsCancellationRequested; i++)
                        {
                            try
                            {
                                double position = startPosition + (i * captureInterval);
                                long seekTicks = (long)(position * 10000000);

                                // Seek to position
                                _player.Seek(seekTicks);
                                Thread.Sleep(200); // Wait for seek to complete

                                // Capture frame
                                if (WaitForFrame(5))
                                {
                                    var bitmap = CaptureCurrentFrame();
                                    if (bitmap != null)
                                    {
                                        if (width > 0 && height > 0)
                                        {
                                            bitmap = ResizeBitmap(bitmap, width, height);
                                        }

                                        var outputPath = Path.Combine(outputFolder, $"frame_{i:D5}_{position:F2}s.jpg");
                                        SaveBitmap(bitmap, outputPath);
                                        bitmap.Dispose();
                                        count++;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error capturing frame {i}: {ex.Message}");
                            }
                        }
                    }
                    finally
                    {
                        _player?.Stop();
                    }
                }
            }, cancellationToken);

            return count;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Opens a video source (file or stream)
        /// </summary>
        private bool OpenVideoSource(string videoSource, int timeoutSeconds)
        {
            try
            {
                _player.Open(videoSource);

                // Wait for the player to be ready
                var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
                while (DateTime.Now < timeout && !_player.IsPlaying && _player.Status != Status.Ended)
                {
                    if (_player.Status == Status.Failed)
                    {
                        Console.WriteLine($"Player failed to open: {_player.LastError}");
                        return false;
                    }

                    Thread.Sleep(100);
                }

                return _player.Status == Status.Playing || _player.Status == Status.Paused || _player.Status == Status.Ended;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception opening video source: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Waits for a video frame to be available
        /// </summary>
        private bool WaitForFrame(int timeoutSeconds)
        {
            var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
            while (DateTime.Now < timeout)
            {
                if (_player.renderer?.GetFrame() != null)
                    return true;

                Thread.Sleep(50);
            }
            return false;
        }

        /// <summary>
        /// Captures the current frame as a bitmap
        /// </summary>
        private Bitmap CaptureCurrentFrame()
        {
            try
            {
                var frame = _player.renderer?.GetFrame();
                if (frame?.textures?.Length > 0)
                {
                    // Convert frame to bitmap
                    // This is a simplified approach - in practice, you might need to handle different pixel formats
                    var texture = frame.textures[0];
                    if (texture.data != null)
                    {
                        // Create bitmap from texture data
                        var bitmap = new Bitmap(texture.width, texture.height, PixelFormat.Format24bppRgb);
                        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                        unsafe
                        {
                            byte* ptr = (byte*)bitmapData.Scan0;
                            fixed (byte* srcPtr = texture.data)
                            {
                                // Copy texture data to bitmap (this is simplified - actual implementation depends on pixel format)
                                for (int i = 0; i < Math.Min(texture.data.Length, bitmapData.Stride * bitmap.Height); i++)
                                {
                                    ptr[i] = srcPtr[i];
                                }
                            }
                        }

                        bitmap.UnlockBits(bitmapData);
                        return bitmap;
                    }
                }

                // Fallback: try to get frame through different method
                return GetFrameAlternative();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing current frame: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Alternative method to get frame (simplified implementation)
        /// </summary>
        private Bitmap GetFrameAlternative()
        {
            try
            {
                // This is a placeholder implementation
                // In a real scenario, you would need to properly extract the frame data
                // from Flyleaf's renderer based on the specific version and configuration
                
                // For now, return a simple placeholder or null
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resizes a bitmap to the specified dimensions
        /// </summary>
        private Bitmap ResizeBitmap(Bitmap original, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(original, 0, 0, width, height);
            }
            original.Dispose();
            return resized;
        }

        /// <summary>
        /// Saves a bitmap to the specified path
        /// </summary>
        private void SaveBitmap(Bitmap bitmap, string outputPath)
        {
            var extension = Path.GetExtension(outputPath).ToLower();
            ImageFormat format = extension switch
            {
                ".png" => ImageFormat.Png,
                ".bmp" => ImageFormat.Bmp,
                ".gif" => ImageFormat.Gif,
                _ => ImageFormat.Jpeg
            };

            bitmap.Save(outputPath, format);
        }

        /// <summary>
        /// Determines if the video source is a stream or a local file
        /// </summary>
        private bool IsStreamSource(string videoSource)
        {
            return videoSource.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase) ||
                   videoSource.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   videoSource.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                   videoSource.StartsWith("udp://", StringComparison.OrdinalIgnoreCase) ||
                   videoSource.StartsWith("rtp://", StringComparison.OrdinalIgnoreCase) ||
                   videoSource.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Tests connection to a video stream or file
        /// </summary>
        /// <param name="videoSource">Video source URL or file path</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Whether the connection/file access was successful</returns>
        public bool TestConnection(string videoSource, int timeoutSeconds = 10)
        {
            if (string.IsNullOrEmpty(videoSource))
                return false;

            if (!IsStreamSource(videoSource))
            {
                // For local files, check if file exists and is accessible
                return File.Exists(videoSource);
            }

            // For streams, try to open and get at least one frame
            lock (_lockObject)
            {
                try
                {
                    var tempPlayer = new Player(_config);
                    tempPlayer.Open(videoSource);

                    var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
                    while (DateTime.Now < timeout)
                    {
                        if (tempPlayer.Status == Status.Playing || tempPlayer.Status == Status.Paused)
                        {
                            tempPlayer.Stop();
                            tempPlayer.Dispose();
                            return true;
                        }
                        if (tempPlayer.Status == Status.Failed)
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }

                    tempPlayer.Stop();
                    tempPlayer.Dispose();
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection test failed: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates the output directory if it doesn't exist
        /// </summary>
        private void CreateOutputDirectory(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        #endregion

        #region IDisposable Implementation

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
                    _player?.Stop();
                    _player?.Dispose();
                }
                _isDisposed = true;
            }
        }

        ~CaptureImage2()
        {
            Dispose(false);
        }

        #endregion
    }
}