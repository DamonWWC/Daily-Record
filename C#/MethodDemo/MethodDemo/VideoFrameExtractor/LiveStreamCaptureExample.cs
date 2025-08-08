using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    /// <summary>
    /// LiveStreamFrameCapture 使用示例
    /// </summary>
    public static class LiveStreamCaptureExample
    {
        /// <summary>
        /// 运行实时视频流截图示�?
        /// </summary>
        public static async Task RunExample()
        {
            Console.WriteLine("=== 实时视频流截图功能演�?===");
            Console.WriteLine();

            // 获取视频�?
            Console.Write("请输入视频流URL或本地视频文件路�? ");
            string? videoSource = Console.ReadLine();
            
            if (string.IsNullOrEmpty(videoSource))
            {
                Console.WriteLine("未输入视频源，使用默认测试视频�?);
                videoSource = "Sample.mp4"; // 使用项目中的示例视频
                
                if (!File.Exists(videoSource))
                {
                    Console.WriteLine("默认测试视频不存在，请提供有效的视频源�?);
                    return;
                }
            }

            // 创建输出目录
            string outputDir = Path.Combine("output", "livestream_captures");
            Directory.CreateDirectory(outputDir);

            using var liveCapture = new LiveStreamFrameCapture(videoSource);

            try
            {
                // 1. 启动视频流监�?
                Console.WriteLine("正在连接视频�?..");
                bool started = await liveCapture.StartAsync(timeoutSeconds: 10);
                
                if (!started)
                {
                    Console.WriteLine("无法连接到视频流�?);
                    return;
                }

                Console.WriteLine("视频流连接成功！");
                Console.WriteLine($"流信�? {liveCapture.GetStreamInfo()}");
                Console.WriteLine();

                // 显示功能菜单
                await ShowMenuAndHandleInput(liveCapture, outputDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
            finally
            {
                liveCapture.Stop();
            }
        }

        /// <summary>
        /// 显示菜单并处理用户输�?
        /// </summary>
        private static async Task ShowMenuAndHandleInput(LiveStreamFrameCapture liveCapture, string outputDir)
        {
            var cts = new CancellationTokenSource();
            Task? continuousTask = null;

            while (liveCapture.IsRunning)
            {
                // 显示当前流状�?
                var info = liveCapture.GetStreamInfo();
                Console.WriteLine($"\n当前状�? 帧数={info.CurrentFrame}, 运行时间={info.ElapsedSeconds:F1}�? FPS={info.Fps:F1}");
                
                Console.WriteLine("\n请选择操作:");
                Console.WriteLine("1. 立即截取当前�?);
                Console.WriteLine("2. 5秒后截取�?);
                Console.WriteLine("3. �?00帧时截取");
                Console.WriteLine("4. 开始每2秒持续截�?);
                Console.WriteLine("5. 停止持续截图");
                Console.WriteLine("6. 查看流信�?);
                Console.WriteLine("0. 退�?);
                Console.Write("请输入选择 (0-6): ");

                string? input = Console.ReadLine();
                
                switch (input)
                {
                    case "1":
                        await HandleImmediateCapture(liveCapture, outputDir);
                        break;
                        
                    case "2":
                        HandleDelayedCapture(liveCapture, outputDir);
                        break;
                        
                    case "3":
                        HandleFrameCapture(liveCapture, outputDir);
                        break;
                        
                    case "4":
                        if (continuousTask == null || continuousTask.IsCompleted)
                        {
                            cts = new CancellationTokenSource();
                            continuousTask = liveCapture.StartContinuousCaptureAsync(
                                intervalSeconds: 2.0, 
                                outputDir, 
                                maxFrames: 10, // 最�?0�?
                                cancellationToken: cts.Token);
                            Console.WriteLine("已启动持续截�?(�?秒一次，最�?0�?");
                        }
                        else
                        {
                            Console.WriteLine("持续截图已在运行�?);
                        }
                        break;
                        
                    case "5":
                        if (continuousTask != null && !continuousTask.IsCompleted)
                        {
                            cts.Cancel();
                            await continuousTask;
                            Console.WriteLine("持续截图已停�?);
                        }
                        else
                        {
                            Console.WriteLine("没有运行中的持续截图任务");
                        }
                        break;
                        
                    case "6":
                        ShowStreamInfo(liveCapture);
                        break;
                        
                    case "0":
                        if (continuousTask != null && !continuousTask.IsCompleted)
                        {
                            cts.Cancel();
                            await continuousTask;
                        }
                        return;
                        
                    default:
                        Console.WriteLine("无效选择，请重新输入");
                        break;
                }

                // 短暂暂停，让用户看到结果
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// 处理立即截图
        /// </summary>
        private static async Task HandleImmediateCapture(LiveStreamFrameCapture liveCapture, string outputDir)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var outputPath = Path.Combine(outputDir, $"immediate_{timestamp}.jpg");
            
            Console.WriteLine("正在截取当前�?..");
            bool success = await liveCapture.CaptureNowAsync(outputPath, width: 640, height: 480);
            
            if (success)
            {
                Console.WriteLine($"�?截图成功: {outputPath}");
            }
            else
            {
                Console.WriteLine("�?截图失败");
            }
        }

        /// <summary>
        /// 处理延时截图
        /// </summary>
        private static void HandleDelayedCapture(LiveStreamFrameCapture liveCapture, string outputDir)
        {
            var info = liveCapture.GetStreamInfo();
            double targetTime = info.ElapsedSeconds + 5.0; // 5秒后
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var outputPath = Path.Combine(outputDir, $"delayed_{timestamp}.jpg");
            
            string taskId = liveCapture.CaptureAfterSeconds(targetTime, outputPath, width: 800, height: 600);
            Console.WriteLine($"已安�?秒后截图任务 (任务ID: {taskId[..8]}...)");
        }

        /// <summary>
        /// 处理指定帧截�?
        /// </summary>
        private static void HandleFrameCapture(LiveStreamFrameCapture liveCapture, string outputDir)
        {
            var info = liveCapture.GetStreamInfo();
            long targetFrame = Math.Max(100, info.CurrentFrame + 50); // 至少�?00帧或当前�?50
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var outputPath = Path.Combine(outputDir, $"frame_{targetFrame}_{timestamp}.jpg");
            
            string taskId = liveCapture.CaptureAfterFrames(targetFrame, outputPath);
            Console.WriteLine($"已安排第{targetFrame}帧截图任�?(任务ID: {taskId[..8]}...)");
        }

        /// <summary>
        /// 显示流信�?
        /// </summary>
        private static void ShowStreamInfo(LiveStreamFrameCapture liveCapture)
        {
            var info = liveCapture.GetStreamInfo();
            Console.WriteLine("\n=== 流信息详�?===");
            Console.WriteLine($"视频�? {info.VideoSource}");
            Console.WriteLine($"运行状�? {(info.IsRunning ? "运行�? : "已停�?)}");
            Console.WriteLine($"帧率: {info.Fps:F2} fps");
            Console.WriteLine($"当前帧数: {info.CurrentFrame}");
            Console.WriteLine($"运行时长: {info.ElapsedSeconds:F2} �?);
            Console.WriteLine($"开始时�? {info.StartTime:yyyy-MM-dd HH:mm:ss}");
            
            if (info.IsRunning && info.Fps > 0)
            {
                Console.WriteLine($"预计视频时长: {info.CurrentFrame / info.Fps:F2} �?);
            }
            Console.WriteLine("==================");
        }

        /// <summary>
        /// 演示批量任务调度
        /// </summary>
        public static async Task RunBatchExample(string videoSource)
        {
            Console.WriteLine("=== 批量截图任务演示 ===");
            
            using var liveCapture = new LiveStreamFrameCapture(videoSource);
            
            if (!await liveCapture.StartAsync())
            {
                Console.WriteLine("无法启动视频�?);
                return;
            }

            string outputDir = Path.Combine("output", "batch_captures");
            Directory.CreateDirectory(outputDir);

            // 安排多个截图任务
            var tasks = new[]
            {
                liveCapture.CaptureAfterSeconds(2.0, Path.Combine(outputDir, "after_2s.jpg")),
                liveCapture.CaptureAfterSeconds(5.0, Path.Combine(outputDir, "after_5s.jpg")),
                liveCapture.CaptureAfterSeconds(10.0, Path.Combine(outputDir, "after_10s.jpg")),
                liveCapture.CaptureAfterFrames(100, Path.Combine(outputDir, "frame_100.jpg")),
                liveCapture.CaptureAfterFrames(200, Path.Combine(outputDir, "frame_200.jpg")),
            };

            Console.WriteLine($"已安�?{tasks.Length} 个截图任�?);
            Console.WriteLine("等待任务完成... (按任意键提前退�?");

            // 等待15秒或用户按键
            var waitTask = Task.Delay(15000);
            var keyTask = Task.Run(() => Console.ReadKey(true));
            
            await Task.WhenAny(waitTask, keyTask);
            
            Console.WriteLine("\n批量截图演示完成");
        }
    }
}