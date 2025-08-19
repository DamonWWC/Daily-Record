using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    public class VideoScreenshotter : IDisposable
    {
        private readonly string _ffmpegPath;
        private bool _isDisposed;

        /// <summary>
        /// 初始化视频截图器
        /// </summary>
        /// <param name="ffmpegPath">FFmpeg.exe 的路径</param>
        public VideoScreenshotter(string ffmpegPath = "ffmpeg.exe")
        {
            _ffmpegPath = ffmpegPath;

            if (!File.Exists(_ffmpegPath))
            {
                // 尝试在 PATH 环境变量中查找
                if (!TryFindInPath(_ffmpegPath, out _ffmpegPath))
                {
                    throw new FileNotFoundException($"找不到 FFmpeg 可执行文件: {ffmpegPath}");
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
        /// 从实时监控视频截取单张图片
        /// </summary>
        /// <param name="streamUrl">视频流 URL (例如: rtsp://example.com/stream)</param>
        /// <param name="outputPath">输出图片路径</param>
        /// <param name="timeoutSeconds">连接超时时间（秒）</param>
        /// <param name="width">输出图片宽度（默认-1，保持原始比例）</param>
        /// <param name="height">输出图片高度（默认-1，保持原始比例）</param>
        /// <returns>是否成功</returns>
        public bool CaptureSingleImage(string streamUrl, string outputPath, int timeoutSeconds = 10, int width = -1, int height = -1)
        {
            CreateOutputDirectory(outputPath);

            var args = BuildCaptureArguments(streamUrl, outputPath, timeoutSeconds, width, height);
            return ExecuteFFmpeg(args);
        }
        /// <summary>
        /// 按时间点截取视频图片
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputPath">输出图片路径</param>
        /// <param name="timeStamp">时间点（秒）</param>
        /// <param name="width">输出图片宽度（默认-1，保持原始比例）</param>
        /// <param name="height">输出图片高度（默认-1，保持原始比例）</param>
        /// <returns>是否成功</returns>
        public bool CaptureByTime(string videoPath, string outputPath, double timeStamp, int width = -1, int height = -1)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException("视频文件不存在", videoPath);

            CreateOutputDirectory(outputPath);

            var args = BuildCaptureArguments(videoPath, outputPath, timeStamp, width, height);
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 异步按时间点截取视频图片
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputPath">输出图片路径</param>
        /// <param name="timeStamp">时间点（秒）</param>
        /// <param name="width">输出图片宽度（默认-1，保持原始比例）</param>
        /// <param name="height">输出图片高度（默认-1，保持原始比例）</param>
        /// <returns>是否成功的任务</returns>
        public async Task<bool> CaptureByTimeAsync(string videoPath, string outputPath, double timeStamp, int width = -1, int height = -1)
        {
            return await Task.Run(() => CaptureByTime(videoPath, outputPath, timeStamp, width, height));
        }

        /// <summary>
        /// 按固定间隔截取多个视频图片
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputFolder">输出文件夹</param>
        /// <param name="interval">间隔（秒）</param>
        /// <param name="width">输出图片宽度（默认-1，保持原始比例）</param>
        /// <param name="height">输出图片高度（默认-1，保持原始比例）</param>
        /// <returns>成功截取的图片数量</returns>
        public int CaptureMultipleByInterval(string videoPath, string outputFolder, double interval, int width = -1, int height = -1)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException("视频文件不存在", videoPath);

            CreateOutputDirectory(outputFolder);

            var duration = GetVideoDuration(videoPath);
            if (duration <= 0) return 0;

            int count = 0;
            for (double time = 0; time < duration; time += interval)
            {
                var outputPath = Path.Combine(outputFolder, $"frame_{time:0.00}.jpg");
                if (CaptureByTime(videoPath, outputPath, time, width, height))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 异步按固定间隔截取多个视频图片
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputFolder">输出文件夹</param>
        /// <param name="interval">间隔（秒）</param>
        /// <param name="width">输出图片宽度（默认-1，保持原始比例）</param>
        /// <param name="height">输出图片高度（默认-1，保持原始比例）</param>
        /// <returns>成功截取的图片数量的任务</returns>
        public async Task<int> CaptureMultipleByIntervalAsync(string videoPath, string outputFolder, double interval, int width = -1, int height = -1)
        {
            return await Task.Run(() => CaptureMultipleByInterval(videoPath, outputFolder, interval, width, height));
        }

        /// <summary>
        /// 获取视频时长（秒）
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <returns>视频时长（秒），失败时返回-1</returns>
        public double GetVideoDuration(string videoPath)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException("视频文件不存在", videoPath);

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    if (process == null) return -1;

                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"获取视频时长时出错: {error}");
                        return -1;
                    }

                    if (double.TryParse(output, out double duration))
                    {
                        return duration;
                    }

                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取视频时长时发生异常: {ex.Message}");
                return -1;
            }
        }

        private string BuildCaptureArguments(string videoPath, string outputPath, double timeStamp, int width, int height)
        {
            var sizeArg = width > 0 && height > 0 ? $"-s {width}x{height}" : "";
            var format = Path.GetExtension(outputPath).TrimStart('.').ToLower();

            // 设置输出格式
            string formatArg = "";
            if (format == "jpg" || format == "jpeg")
            {
                formatArg = "-q:v 2"; // JPEG 质量，1-31，数值越小质量越高
            }
            else if (format == "png")
            {
                formatArg = "-compression_level 9"; // PNG 压缩级别，0-9
            }

            return $"-ss {timeStamp} -i \"{videoPath}\" -vframes 1 {sizeArg} {formatArg} -y \"{outputPath}\"";
        }

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

                    process.WaitForExit();
                    var error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"FFmpeg 执行失败: {error}");
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行 FFmpeg 时发生异常: {ex.Message}");
                return false;
            }
        }

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

        ~VideoScreenshotter()
        {
            Dispose(false);
        }
    }
}
