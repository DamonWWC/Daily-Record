using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    /// <summary>
    /// Utility class for capturing frames from video sources (RTSP streams and local video files)
    /// </summary>
    public class CaptureImage : IDisposable
    {
        private readonly string _ffmpegPath;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the CaptureImage class
        /// </summary>
        /// <param name="ffmpegPath">Path to FFmpeg.exe</param>
        public CaptureImage(string ffmpegPath = "ffmpeg.exe")
        {
            _ffmpegPath = ffmpegPath;

            if (!File.Exists(_ffmpegPath))
            {
                // Try to find in PATH environment variable
                if (!TryFindInPath(_ffmpegPath, out _ffmpegPath))
                {
                    throw new FileNotFoundException($"Could not find FFmpeg executable: {ffmpegPath}");
                }
            }
        }

        private bool TryFindInPath(string fileName, out string fullPath)
        {
            fullPath = null;
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv)) return false;

            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var fullFilePath = Path.Combine(path.Trim(), fileName);
                if (File.Exists(fullFilePath))
                {
                    fullPath = fullFilePath;
                    return true;
                }
            }
            return false;
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

            bool isStream = IsStreamSource(videoSource);
            var args = BuildCaptureArguments(videoSource, outputPath, isStream, timePosition, timeoutSeconds, width, height);
            return ExecuteFFmpeg(args);
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

            // For local files, if totalDuration is 0, get the video duration
            if (!isStream && totalDuration <= 0)
            {
                totalDuration = await GetVideoDurationAsync(videoSource);
                if (totalDuration <= 0)
                {
                    Console.WriteLine("Could not determine video duration");
                    return 0;
                }
                
                // Adjust total duration based on start position
                totalDuration -= startPosition;
                if (totalDuration <= 0)
                {
                    Console.WriteLine("Start position is beyond video duration");
                    return 0;
                }
            }

            // For streams, if no duration specified, use a default
            if (isStream && totalDuration <= 0)
            {
                totalDuration = 60; // Default to 60 seconds for streams if not specified
            }

            int count = 0;
            int totalFrames = (int)Math.Ceiling(totalDuration / captureInterval);

            if (isStream)
            {
                // For streams, capture frames in real-time
                DateTime startTime = DateTime.Now;

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

            return count;
        }

        /// <summary>
        /// Gets the duration of a video file in seconds
        /// </summary>
        /// <param name="videoPath">Path to the video file</param>
        /// <returns>Duration in seconds or 0 if unable to determine</returns>
        private async Task<double> GetVideoDurationAsync(string videoPath)
        {
            var args = $"-i \"{videoPath}\" -v quiet -show_entries format=duration -of default=noprint_wrappers=1:nokey=1";

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath.Replace("ffmpeg", "ffprobe"), // Use ffprobe instead of ffmpeg
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    if (process == null) return 0;

                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0 && double.TryParse(output.Trim(), out double duration))
                    {
                        return duration;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting video duration: {ex.Message}");
            }

            return 0;
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
                   videoSource.StartsWith("rtp://");
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

            // For streams, use ffprobe to test connection
            var args = $"-rtsp_transport tcp -stimeout {timeoutSeconds * 1000000} -i \"{streamUrl}\" -v error -show_entries stream=codec_type -of default=noprint_wrappers=1:nokey=1";

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath.Replace("ffmpeg", "ffprobe"), // Use ffprobe instead of ffmpeg
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    if (process == null) return false;

                    var errorBuilder = new StringBuilder();
                    process.ErrorDataReceived += (sender, e) => 
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            errorBuilder.AppendLine(e.Data);
                    };
                    process.BeginErrorReadLine();

                    // Wait for twice the timeout to give ffprobe enough time to establish connection
                    bool exited = process.WaitForExit(timeoutSeconds * 2 * 1000);

                    if (!exited)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch { /* Ignore errors during termination */ }
                        return false;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    var error = errorBuilder.ToString();
                    
                    // Check if we found any video or audio streams
                    return process.ExitCode == 0 && 
                           (output.Contains("video") || output.Contains("audio")) &&
                           !error.Contains("Failed to connect") && 
                           !error.Contains("Connection refused") &&
                           !error.Contains("Invalid data");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while testing stream connection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Builds FFmpeg command arguments for capturing a frame
        /// </summary>
        private string BuildCaptureArguments(string videoSource, string outputPath, bool isStream, 
            double timePosition, int timeoutSeconds, int width, int height)
        {
            var sizeArg = width > 0 && height > 0 ? $"-s {width}x{height}" : "";
            var format = Path.GetExtension(outputPath).TrimStart('.').ToLower();

            // Format-specific settings
            string formatArg = "";
            if (format == "jpg" || format == "jpeg")
            {
                formatArg = "-q:v 2"; // JPEG quality, 1-31, lower is better
            }
            else if (format == "png")
            {
                formatArg = "-compression_level 9"; // PNG compression level, 0-9
            }

            StringBuilder args = new StringBuilder();

            if (isStream)
            {
                // Optimized parameters for RTSP/network streams
                args.Append($"-rtsp_transport tcp ");          // Use TCP transport for more reliable streaming
                args.Append($"-stimeout {timeoutSeconds * 1000000} "); // Connection timeout in microseconds
            }
            else
            {
                // For local files, seek to the specified position
                if (timePosition > 0)
                {
                    args.Append($"-ss {timePosition} ");       // Seek to position (more accurate when before -i)
                }
            }

            args.Append($"-i \"{videoSource}\" ");             // Input source
            
            // If seeking in local files and we didn't do it before the input
            if (!isStream && timePosition > 0 && !args.ToString().Contains("-ss"))
            {
                args.Append($"-ss {timePosition} ");           // Seek to position
            }

            args.Append("-y ");                                // Overwrite existing files
            args.Append("-vframes 1 ");                        // Capture only one frame
            args.Append("-an ");                               // Ignore audio
            args.Append($"{sizeArg} ");                        // Resolution settings (if specified)
            args.Append($"{formatArg} ");                      // Format-specific parameters
            args.Append($"\"{outputPath}\"");                  // Output file path

            return args.ToString();
        }

        /// <summary>
        /// Executes FFmpeg with the given arguments
        /// </summary>
        private bool ExecuteFFmpeg(string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    if (process == null) return false;

                    // Capture error output (FFmpeg writes most information to stderr)
                    var errorBuilder = new StringBuilder();
                    process.ErrorDataReceived += (sender, e) => 
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            errorBuilder.AppendLine(e.Data);
                    };
                    process.BeginErrorReadLine();

                    // Set timeout to avoid blocking indefinitely
                    bool exited = process.WaitForExit(60000); // 60 second timeout

                    if (!exited)
                    {
                        try
                        {
                            process.Kill();
                            Console.WriteLine("FFmpeg execution timed out and was terminated");
                        }
                        catch { /* Ignore errors during termination */ }
                        return false;
                    }

                    string error = errorBuilder.ToString();

                    // Check for critical errors
                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"FFmpeg execution failed (Exit Code: {process.ExitCode}): {error}");
                        return false;
                    }

                    // Verify output file was created
                    if (arguments.Contains("-vframes 1") && arguments.Contains("-y"))
                    {
                        // Extract output path from arguments
                        int lastQuoteIndex = arguments.LastIndexOf("\"");
                        int secondLastQuoteIndex = arguments.LastIndexOf("\"", lastQuoteIndex - 1);
                        
                        if (secondLastQuoteIndex >= 0 && lastQuoteIndex > secondLastQuoteIndex)
                        {
                            string outputPath = arguments.Substring(secondLastQuoteIndex + 1, lastQuoteIndex - secondLastQuoteIndex - 1);
                            if (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0)
                            {
                                Console.WriteLine("FFmpeg did not generate a valid output file");
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during FFmpeg execution: {ex.Message}");
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

        ~CaptureImage()
        {
            Dispose(false);
        }

        #endregion
    }
}