using OpenCvSharp;

namespace VideoFrameExtractor
{
    /// <summary>
    /// Utility class for capturing frames from video sources (RTSP streams and local video files)
    /// without using external FFmpeg executable
    /// </summary>
    public class CaptureImage1 : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the CaptureImage1 class
        /// </summary>
        public CaptureImage1()
        {
        }

        #region Single Frame Capture

        /// <summary>
        /// Captures a single frame from a video source (RTSP stream or local file)
        /// </summary>
        /// <param name="videoSource">Video source URL or file path</param>
        /// <param name="outputPath">Output image path</param>
        /// <param name="timePosition">Time position to capture (in seconds, for local files only)</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds (for streams)</param>
        /// <param name="width">Output image width (default -1, maintain aspect ratio)</param>
        /// <param name="height">Output image height (default -1, maintain aspect ratio)</param>
        /// <returns>Whether the operation was successful</returns>
        public bool CaptureFrame(string videoSource, string outputPath, double timePosition = 0, 
            int timeoutSeconds = 10, int width = -1, int height = -1)
        {
            CreateOutputDirectory(outputPath);

            try
            {
                using (var capture = new VideoCapture(videoSource))
                {
                    if (!capture.IsOpened())
                    {
                        Console.WriteLine($"Failed to open video source: {videoSource}");
                        return false;
                    }

                    // For local files, seek to the specified position
                    if (!IsStreamSource(videoSource) && timePosition > 0)
                    {
                        // Get FPS to calculate frame position
                        double fps = capture.Get(VideoCaptureProperties.Fps);
                        if (fps <= 0) fps = 25; // Default to 25 fps if unable to determine

                        // Calculate frame position
                        int framePosition = (int)(timePosition * fps);
                        capture.Set(VideoCaptureProperties.PosFrames, framePosition);
                    }

                    // Set timeout for streams
                    if (IsStreamSource(videoSource))
                    {
                        // OpenCV doesn't have a direct timeout setting, so we'll use a task with timeout
                        var timeoutTask = Task.Delay(timeoutSeconds * 1000);
                        var captureTask = Task.Run(() => 
                        {
                            // Try to read a few frames to ensure connection is established
                            for (int i = 0; i < 10; i++)
                            {
                                using (var frame = new Mat())
                                {
                                    if (capture.Read(frame) && !frame.Empty())
                                    {
                                        return true;
                                    }
                                }
                                Thread.Sleep(100);
                            }
                            return false;
                        });

                        int completedTask = Task.WaitAny(captureTask, timeoutTask);
                        if (completedTask == 1 || !captureTask.Result)
                        {
                            Console.WriteLine("Connection to stream timed out or failed");
                            return false;
                        }
                    }

                    // Read the frame
                    using (var frame = new Mat())
                    {
                        if (!capture.Read(frame) || frame.Empty())
                        {
                            Console.WriteLine("Failed to read frame from video source");
                            return false;
                        }

                        // Resize if needed
                        if (width > 0 && height > 0)
                        {
                            Cv2.Resize(frame, frame, new OpenCvSharp.Size(width, height));
                        }

                        // Save the frame
                        string extension = Path.GetExtension(outputPath).ToLower();
                        if (extension == ".jpg" || extension == ".jpeg")
                        {
                            Cv2.ImWrite(outputPath, frame, new ImageEncodingParam(ImwriteFlags.JpegQuality, 95));
                        }
                        else if (extension == ".png")
                        {
                            Cv2.ImWrite(outputPath, frame, new ImageEncodingParam(ImwriteFlags.PngCompression, 9));
                        }
                        else
                        {
                            Cv2.ImWrite(outputPath, frame);
                        }

                        return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing frame: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously captures a single frame from a video source
        /// </summary>
        /// <param name="videoSource">Video source URL or file path</param>
        /// <param name="outputPath">Output image path</param>
        /// <param name="timePosition">Time position to capture (in seconds, for local files only)</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds (for streams)</param>
        /// <param name="width">Output image width</param>
        /// <param name="height">Output image height</param>
        /// <returns>Task representing whether the operation was successful</returns>
        public async Task<bool> CaptureFrameAsync(string videoSource, string outputPath, double timePosition = 0,
            int timeoutSeconds = 10, int width = -1, int height = -1)
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
        /// <param name="totalDuration">Total duration in seconds (for streams) or 0 to use entire video (for local files)</param>
        /// <param name="startPosition">Start position in seconds (for local files only)</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds (for streams)</param>
        /// <param name="width">Output image width</param>
        /// <param name="height">Output image height</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully captured frames</returns>
        public async Task<int> CaptureMultipleFramesAsync(string videoSource, string outputFolder,
            double captureInterval, double totalDuration = 0, double startPosition = 0,
            int timeoutSeconds = 10, int width = -1, int height = -1, 
            CancellationToken cancellationToken = default)
        {
            CreateOutputDirectory(outputFolder);
            bool isStream = IsStreamSource(videoSource);
            int count = 0;

            try
            {
                if (isStream)
                {
                    // For streams, capture frames in real-time
                    DateTime startTime = DateTime.Now;
                    
                    // For streams, if no duration specified, use a default
                    if (totalDuration <= 0)
                    {
                        totalDuration = 60; // Default to 60 seconds for streams if not specified
                    }

                    int totalFrames = (int)Math.Ceiling(totalDuration / captureInterval);

                    for (int i = 0; i < totalFrames; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                        var outputPath = Path.Combine(outputFolder, $"frame_{timestamp}.jpg");

                        if (await CaptureFrameAsync(videoSource, outputPath, 0, timeoutSeconds, width, height))
                        {
                            count++;
                        }

                        // Calculate remaining time for next capture
                        var elapsedTime = DateTime.Now - startTime;
                        var nextCaptureTime = TimeSpan.FromSeconds(captureInterval * (i + 1));
                        var waitTime = nextCaptureTime - elapsedTime;
                        
                        // Only wait if we need to
                        if (waitTime > TimeSpan.Zero && i < totalFrames - 1)
                        {
                            try
                            {
                                await Task.Delay(waitTime, cancellationToken);
                            }
                            catch (TaskCanceledException)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // For local files, extract frames at specific timestamps
                    using (var capture = new VideoCapture(videoSource))
                    {
                        if (!capture.IsOpened())
                        {
                            Console.WriteLine($"Failed to open video source: {videoSource}");
                            return 0;
                        }

                        // Get video properties
                        double fps = capture.Get(VideoCaptureProperties.Fps);
                        if (fps <= 0) fps = 25; // Default to 25 fps if unable to determine
                        
                        double videoDuration = capture.Get(VideoCaptureProperties.FrameCount) / fps;
                        
                        // If totalDuration is 0, use the entire video
                        if (totalDuration <= 0)
                        {
                            totalDuration = videoDuration;
                        }
                        
                        // Adjust total duration based on start position
                        totalDuration = Math.Min(totalDuration, videoDuration - startPosition);
                        if (totalDuration <= 0)
                        {
                            Console.WriteLine("Start position is beyond video duration");
                            return 0;
                        }

                        int totalFrames = (int)Math.Ceiling(totalDuration / captureInterval);

                        for (int i = 0; i < totalFrames; i++)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            double position = startPosition + (i * captureInterval);
                            var outputPath = Path.Combine(outputFolder, $"frame_{i:D5}_{position:F2}s.jpg");

                            if (await CaptureFrameAsync(videoSource, outputPath, position, timeoutSeconds, width, height))
                            {
                                count++;
                            }
                        }
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing multiple frames: {ex.Message}");
                return count;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if the video source is a stream (RTSP, HTTP, etc.) or a local file
        /// </summary>
        private bool IsStreamSource(string videoSource)
        {
            return videoSource.StartsWith("rtsp://") || 
                   videoSource.StartsWith("http://") || 
                   videoSource.StartsWith("https://") ||
                   videoSource.StartsWith("udp://") ||
                   videoSource.StartsWith("rtp://")||
                   videoSource.StartsWith("rtmp://");
        }

        /// <summary>
        /// Tests connection to a video stream
        /// </summary>
        /// <param name="streamUrl">Stream URL</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Whether the connection was successful</returns>
        public bool TestStreamConnection(string streamUrl, int timeoutSeconds = 5)
        {
            if (!IsStreamSource(streamUrl))
            {
                // For local files, just check if the file exists
                return File.Exists(streamUrl);
            }

            try
            {
                using (var capture = new VideoCapture(streamUrl))
                {
                    var connectionTask = Task.Run(() =>
                    {
                        if (!capture.IsOpened())
                            return false;

                        // Try to read a frame to ensure connection is established
                        using (var frame = new Mat())
                        {
                            return capture.Read(frame) && !frame.Empty();
                        }
                    });

                    return connectionTask.Wait(timeoutSeconds * 1000) && connectionTask.Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while testing stream connection: {ex.Message}");
                return false;
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
                _isDisposed = true;
            }
        }

        ~CaptureImage1()
        {
            Dispose(false);
        }

        #endregion
    }
}