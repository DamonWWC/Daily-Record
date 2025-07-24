using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Video Frame Extractor Demo");
            Console.WriteLine("==========================");
            Console.WriteLine("1. Use FFmpeg-based extractor");
            Console.WriteLine("2. Use OpenCV-based extractor (No FFmpeg)");
            Console.WriteLine("3. Use Flyleaf-based extractor");
            Console.Write("Select an option (1, 2, or 3): ");
            
            string option = Console.ReadLine();
            
            if (option == "2")
            {
                await CaptureImage1Example.RunExample();
                return;
            }
            else if (option == "3")
            {
                await CaptureImage2Example.RunExample();
                return;
            }
            
            try
            {
                // Initialize the CaptureImage class
                // Assuming ffmpeg.exe is in the lib/ffmpeg directory
                string ffmpegPath = Path.Combine("lib", "ffmpeg", "ffmpeg.exe");
                using var captureImage = new CaptureImage(ffmpegPath);

                // Example 1: Capture a single frame from an RTSP stream
                Console.WriteLine("\nExample 1: Capture a single frame from an RTSP stream");
                Console.Write("Enter RTSP URL (or press Enter to skip): ");
                string rtspUrl = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(rtspUrl))
                {
                    // Test connection first
                    Console.WriteLine("Testing connection to stream...");
                    if (captureImage.TestStreamConnection(rtspUrl))
                    {
                        Console.WriteLine("Connection successful!");
                        
                        string outputPath = Path.Combine("output", "rtsp_frame.jpg");
                        Console.WriteLine($"Capturing frame to {outputPath}...");
                        
                        bool success = await captureImage.CaptureFrameAsync(rtspUrl, outputPath);
                        Console.WriteLine(success ? "Frame captured successfully!" : "Failed to capture frame.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to the stream.");
                    }
                }

                // Example 2: Capture multiple frames from an RTSP stream
                Console.WriteLine("\nExample 2: Capture multiple frames from an RTSP stream");
                if (!string.IsNullOrEmpty(rtspUrl))
                {
                    Console.Write("Enter capture interval in seconds (e.g., 1.5): ");
                    if (double.TryParse(Console.ReadLine(), out double interval) && interval > 0)
                    {
                        Console.Write("Enter total duration in seconds (e.g., 10): ");
                        if (double.TryParse(Console.ReadLine(), out double duration) && duration > 0)
                        {
                            string outputFolder = Path.Combine("output", "rtsp_frames");
                            Console.WriteLine($"Capturing frames to {outputFolder} every {interval} seconds for {duration} seconds...");
                            
                            using var cts = new CancellationTokenSource();
                            
                            // Allow cancellation with Escape key
                            var cancellationTask = Task.Run(() => {
                                while (!cts.Token.IsCancellationRequested)
                                {
                                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                                    {
                                        Console.WriteLine("Cancelling capture...");
                                        cts.Cancel();
                                        break;
                                    }
                                    Thread.Sleep(100);
                                }
                            });
                            
                            int count = await captureImage.CaptureMultipleFramesAsync(
                                rtspUrl, outputFolder, interval, duration, 
                                timeoutSeconds: 10, cancellationToken: cts.Token);
                            
                            Console.WriteLine($"Captured {count} frames.");
                        }
                    }
                }

                // Example 3: Capture a frame from a local video file
                Console.WriteLine("\nExample 3: Capture a frame from a local video file");
                Console.Write("Enter local video file path (or press Enter to skip): ");
                string videoPath = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
                {
                    Console.Write("Enter time position in seconds: ");
                    if (double.TryParse(Console.ReadLine(), out double position) && position >= 0)
                    {
                        string outputPath = Path.Combine("output", $"local_frame_{position}s.jpg");
                        Console.WriteLine($"Capturing frame at {position}s to {outputPath}...");
                        
                        bool success = await captureImage.CaptureFrameAsync(videoPath, outputPath, position);
                        Console.WriteLine(success ? "Frame captured successfully!" : "Failed to capture frame.");
                    }
                }

                // Example 4: Extract frames at regular intervals from a local video
                Console.WriteLine("\nExample 4: Extract frames at regular intervals from a local video");
                if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
                {
                    Console.Write("Enter capture interval in seconds (e.g., 1.5): ");
                    if (double.TryParse(Console.ReadLine(), out double interval) && interval > 0)
                    {
                        string outputFolder = Path.Combine("output", "local_frames");
                        Console.WriteLine($"Extracting frames to {outputFolder} every {interval} seconds...");
                        
                        int count = await captureImage.CaptureMultipleFramesAsync(
                            videoPath, outputFolder, interval);
                        
                        Console.WriteLine($"Extracted {count} frames.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}