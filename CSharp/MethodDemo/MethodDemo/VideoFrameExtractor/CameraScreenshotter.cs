﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    public class CameraScreenshotter : IDisposable
    {
        private readonly string _ffmpegPath;
        private bool _isDisposed;

        /// <summary>
        /// Initializes the camera screenshot utility
        /// </summary>
        /// <param name="ffmpegPath">Path to FFmpeg.exe</param>
        public CameraScreenshotter(string ffmpegPath = "ffmpeg.exe")
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

        /// <summary>
        /// Captures a single image from a live RTSP stream
        /// </summary>
        /// <param name="streamUrl">Stream URL (e.g., rtsp://example.com/stream)</param>
        /// <param name="outputPath">Output image path</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds</param>
        /// <param name="width">Output image width (default -1, maintain aspect ratio)</param>
        /// <param name="height">Output image height (default -1, maintain aspect ratio)</param>
        /// <returns>Whether the operation was successful</returns>
        public bool CaptureSingleImage(string streamUrl, string outputPath, int timeoutSeconds = 10, int width = -1, int height = -1)
        {
            CreateOutputDirectory(outputPath);

            var args = BuildCaptureArguments(streamUrl, outputPath, timeoutSeconds, width, height);
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// Asynchronously captures a single image from a live RTSP stream
        /// </summary>
        /// <param name="streamUrl">Stream URL</param>
        /// <param name="outputPath">Output image path</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds</param>
        /// <param name="width">Output image width</param>
        /// <param name="height">Output image height</param>
        /// <returns>Task representing whether the operation was successful</returns>
        public async Task<bool> CaptureSingleImageAsync(string streamUrl, string outputPath, int timeoutSeconds = 10, int width = -1, int height = -1)
        {
            return await Task.Run(() => CaptureSingleImage(streamUrl, outputPath, timeoutSeconds, width, height));
        }

        /// <summary>
        /// Captures multiple images from a live RTSP stream at fixed intervals
        /// </summary>
        /// <param name="streamUrl">Stream URL</param>
        /// <param name="outputFolder">Output folder</param>
        /// <param name="captureInterval">Capture interval in seconds</param>
        /// <param name="totalDuration">Total duration in seconds</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds</param>
        /// <param name="width">Output image width</param>
        /// <param name="height">Output image height</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully captured images</returns>
        public async Task<int> CaptureMultipleImagesAsync(string streamUrl, string outputFolder,
            double captureInterval, double totalDuration, int timeoutSeconds = 10,
            int width = -1, int height = -1, CancellationToken cancellationToken = default)
        {
            CreateOutputDirectory(outputFolder);

            int count = 0;
            int totalIntervals = (int)Math.Ceiling(totalDuration / captureInterval);
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < totalIntervals; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var outputPath = Path.Combine(outputFolder, $"screenshot_{timestamp}.jpg");

                if (await CaptureSingleImageAsync(streamUrl, outputPath, timeoutSeconds, width, height))
                {
                    count++;
                }

                // Calculate remaining time for next capture
                var elapsedTime = DateTime.Now - startTime;
                var nextCaptureTime = TimeSpan.FromSeconds(captureInterval * (i + 1));
                var waitTime = nextCaptureTime - elapsedTime;
                
                // Only wait if we need to
                if (waitTime > TimeSpan.Zero && i < totalIntervals - 1)
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

            return count;
        }

        /// <summary>
        /// Tests connection to an RTSP stream
        /// </summary>
        /// <param name="streamUrl">Stream URL</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Whether the connection was successful</returns>
        public bool TestStreamConnection(string streamUrl, int timeoutSeconds = 5)
        {
            // Use more reliable parameters for testing connection
            var args = $"-rtsp_transport tcp -stimeout {timeoutSeconds * 1000000} -i \"{streamUrl}\" -vframes 0 -an -f null -";

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
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

                    // Wait for twice the timeout to give FFmpeg enough time to establish connection
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

                    var error = errorBuilder.ToString();
                    return process.ExitCode == 0 && 
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
        private string BuildCaptureArguments(string streamUrl, string outputPath, int timeoutSeconds, int width, int height)
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

            // Optimized parameters for RTSP streams
            return $"-rtsp_transport tcp " +          // Use TCP transport for more reliable streaming
                   $"-stimeout {timeoutSeconds * 1000000} " + // Connection timeout in microseconds
                   $"-i \"{streamUrl}\" " +           // Input stream
                   "-y " +                            // Overwrite existing files
                   "-vframes 1 " +                    // Capture only one frame
                   "-an " +                           // Ignore audio
                   $"{sizeArg} " +                    // Resolution settings (if specified)
                   $"{formatArg} " +                  // Format-specific parameters
                   $"\"{outputPath}\"";               // Output file path
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
                    bool exited = process.WaitForExit(30000); // 30 second timeout

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

        ~CameraScreenshotter()
        {
            Dispose(false);
        }
    }
}