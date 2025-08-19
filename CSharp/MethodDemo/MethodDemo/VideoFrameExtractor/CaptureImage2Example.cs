using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    /// <summary>
    /// Example class demonstrating the usage of CaptureImage2 with Flyleaf library
    /// </summary>
    public static class CaptureImage2Example
    {
        /// <summary>
        /// Runs comprehensive examples of CaptureImage2 functionality
        /// </summary>
        public static async Task RunExample()
        {
            Console.WriteLine("CaptureImage2 (Flyleaf) Examples");
            Console.WriteLine("================================");

            try
            {
                using var captureImage = new CaptureImage2();

                // Example 1: Test connection and capture single frame from RTSP stream
                await TestRtspStreamCapture(captureImage);

                // Example 2: Capture multiple frames from RTSP stream
                await TestRtspMultipleFrames(captureImage);

                // Example 3: Capture single frame from local video file
                await TestLocalVideoCapture(captureImage);

                // Example 4: Extract multiple frames from local video file
                await TestLocalVideoMultipleFrames(captureImage);

                Console.WriteLine("\nAll examples completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in examples: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Tests RTSP stream connection and single frame capture
        /// </summary>
        private static async Task TestRtspStreamCapture(CaptureImage2 captureImage)
        {
            Console.WriteLine("\n=== Example 1: RTSP Stream Single Frame Capture ===");
            Console.Write("Enter RTSP URL (or press Enter to skip): ");
            string rtspUrl = Console.ReadLine();

            if (string.IsNullOrEmpty(rtspUrl))
            {
                Console.WriteLine("Skipping RTSP stream example.");
                return;
            }

            try
            {
                // Test connection first
                Console.WriteLine("Testing connection to RTSP stream...");
                bool connectionTest = captureImage.TestConnection(rtspUrl, 15);
                
                if (connectionTest)
                {
                    Console.WriteLine("✓ Connection successful!");
                    
                    // Capture a single frame
                    string outputPath = Path.Combine("output", "flyleaf_rtsp_frame.jpg");
                    Console.WriteLine($"Capturing frame to {outputPath}...");
                    
                    bool success = await captureImage.CaptureFrameAsync(rtspUrl, outputPath, 0, 30);
                    
                    if (success)
                    {
                        Console.WriteLine("✓ Frame captured successfully!");
                        Console.WriteLine($"  Output: {Path.GetFullPath(outputPath)}");
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to capture frame.");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to connect to the RTSP stream.");
                    Console.WriteLine("  Please check the URL and network connectivity.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error in RTSP stream capture: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests capturing multiple frames from RTSP stream
        /// </summary>
        private static async Task TestRtspMultipleFrames(CaptureImage2 captureImage)
        {
            Console.WriteLine("\n=== Example 2: RTSP Stream Multiple Frames Capture ===");
            Console.Write("Enter RTSP URL (or press Enter to skip): ");
            string rtspUrl = Console.ReadLine();

            if (string.IsNullOrEmpty(rtspUrl))
            {
                Console.WriteLine("Skipping RTSP multiple frames example.");
                return;
            }

            try
            {
                Console.Write("Enter capture interval in seconds (e.g., 2.0): ");
                if (!double.TryParse(Console.ReadLine(), out double interval) || interval <= 0)
                {
                    interval = 2.0;
                    Console.WriteLine($"Using default interval: {interval} seconds");
                }

                Console.Write("Enter total duration in seconds (e.g., 10): ");
                if (!double.TryParse(Console.ReadLine(), out double duration) || duration <= 0)
                {
                    duration = 10.0;
                    Console.WriteLine($"Using default duration: {duration} seconds");
                }

                string outputFolder = Path.Combine("output", "flyleaf_rtsp_frames");
                Console.WriteLine($"Capturing frames to {outputFolder}");
                Console.WriteLine($"Interval: {interval}s, Duration: {duration}s");
                Console.WriteLine("Press ESC to cancel...");

                using var cts = new CancellationTokenSource();

                // Monitor for ESC key press
                var keyTask = Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                Console.WriteLine("Cancelling capture...");
                                cts.Cancel();
                                break;
                            }
                        }
                        Thread.Sleep(100);
                    }
                });

                int count = await captureImage.CaptureMultipleFramesAsync(
                    rtspUrl, outputFolder, interval, duration,
                    timeoutSeconds: 30, cancellationToken: cts.Token);

                Console.WriteLine($"✓ Captured {count} frames from RTSP stream.");
                if (count > 0)
                {
                    Console.WriteLine($"  Output folder: {Path.GetFullPath(outputFolder)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error in RTSP multiple frames capture: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests capturing a single frame from local video file
        /// </summary>
        private static async Task TestLocalVideoCapture(CaptureImage2 captureImage)
        {
            Console.WriteLine("\n=== Example 3: Local Video Single Frame Capture ===");
            Console.Write("Enter local video file path (or press Enter to skip): ");
            string videoPath = Console.ReadLine();

            if (string.IsNullOrEmpty(videoPath))
            {
                Console.WriteLine("Skipping local video example.");
                return;
            }

            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"✗ File not found: {videoPath}");
                return;
            }

            try
            {
                Console.Write("Enter time position in seconds (e.g., 5.5): ");
                if (!double.TryParse(Console.ReadLine(), out double position) || position < 0)
                {
                    position = 0;
                    Console.WriteLine($"Using default position: {position} seconds");
                }

                Console.Write("Enter output width (0 for original): ");
                if (!int.TryParse(Console.ReadLine(), out int width) || width < 0)
                {
                    width = 0;
                }

                Console.Write("Enter output height (0 for original): ");
                if (!int.TryParse(Console.ReadLine(), out int height) || height < 0)
                {
                    height = 0;
                }

                string outputPath = Path.Combine("output", $"flyleaf_local_frame_{position:F1}s.jpg");
                Console.WriteLine($"Capturing frame at {position}s to {outputPath}...");

                bool success = await captureImage.CaptureFrameAsync(
                    videoPath, outputPath, position, 30, width, height);

                if (success)
                {
                    Console.WriteLine("✓ Frame captured successfully!");
                    Console.WriteLine($"  Output: {Path.GetFullPath(outputPath)}");
                }
                else
                {
                    Console.WriteLine("✗ Failed to capture frame.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error in local video capture: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests extracting multiple frames from local video file
        /// </summary>
        private static async Task TestLocalVideoMultipleFrames(CaptureImage2 captureImage)
        {
            Console.WriteLine("\n=== Example 4: Local Video Multiple Frames Extraction ===");
            Console.Write("Enter local video file path (or press Enter to skip): ");
            string videoPath = Console.ReadLine();

            if (string.IsNullOrEmpty(videoPath))
            {
                Console.WriteLine("Skipping local video multiple frames example.");
                return;
            }

            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"✗ File not found: {videoPath}");
                return;
            }

            try
            {
                Console.Write("Enter capture interval in seconds (e.g., 1.0): ");
                if (!double.TryParse(Console.ReadLine(), out double interval) || interval <= 0)
                {
                    interval = 1.0;
                    Console.WriteLine($"Using default interval: {interval} seconds");
                }

                Console.Write("Enter start position in seconds (e.g., 0): ");
                if (!double.TryParse(Console.ReadLine(), out double startPos) || startPos < 0)
                {
                    startPos = 0;
                    Console.WriteLine($"Using default start position: {startPos} seconds");
                }

                Console.Write("Enter duration in seconds (0 for entire video): ");
                if (!double.TryParse(Console.ReadLine(), out double duration) || duration < 0)
                {
                    duration = 0;
                    Console.WriteLine("Using entire video duration");
                }

                string outputFolder = Path.Combine("output", "flyleaf_local_frames");
                Console.WriteLine($"Extracting frames to {outputFolder}");
                Console.WriteLine($"Interval: {interval}s, Start: {startPos}s, Duration: {(duration > 0 ? duration.ToString() : "entire video")}");

                using var cts = new CancellationTokenSource();

                int count = await captureImage.CaptureMultipleFramesAsync(
                    videoPath, outputFolder, interval, duration, startPos,
                    timeoutSeconds: 30, cancellationToken: cts.Token);

                Console.WriteLine($"✓ Extracted {count} frames from local video.");
                if (count > 0)
                {
                    Console.WriteLine($"  Output folder: {Path.GetFullPath(outputFolder)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error in local video multiple frames extraction: {ex.Message}");
            }
        }
    }
}
