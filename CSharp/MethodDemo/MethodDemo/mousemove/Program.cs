
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace mousemove
{
 
    class MouseMover
    {
        // P/Invoke声明
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, IntPtr dwData, IntPtr dwExtraInfo);

        // 鼠标事件常量
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x0008;

        // 定时器
        private static System.Threading.Timer _timer;

        static void Main(string[] args)
        {
            Console.WriteLine("鼠标移动器已启动，按Ctrl+C停止...");

            // 默认参数：每5秒向右移动100像素
            uint moveInterval = 60000; // 时间间隔(毫秒)
            uint deltaX = 100;       // X轴变化量
            bool isAbsolute = false; // 是否使用绝对坐标

            // 解析命令行参数（可选）
            if (args.Length > 0)
            {
                try
                {
                    moveInterval = uint.Parse(args[0]);
                    if (args.Length > 1) deltaX = uint.Parse(args[1]);
                    if (args.Length > 2) isAbsolute = bool.Parse(args[2]);
                }
                catch
                {
                    // 使用默认参数
                }
            }

            // 创建定时器
            _timer = new System.Threading.Timer(MoveMouse, null, moveInterval, moveInterval);

            // 等待控制台输入停止
            Console.WriteLine("按 Ctrl+C 停止...");
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // 防止生成Ctrl+C日志
                StopTimer();
            };

            Task.Run(() => { while (true) Thread.Sleep(1000); }).Wait(); // 保持主线程存活
        }

        private static void MoveMouse(object state)
        {
            try
            {
                // 计算新坐标（相对或绝对模式）
                uint screenWidth = GetScreenWidth();
                uint screenHeight = GetScreenHeight();

                uint newX = 10;
                uint newY = 10;

                // 移动鼠标
                mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, newX, newY, IntPtr.Zero, IntPtr.Zero);

                // 输出日志
                Console.WriteLine($"[{DateTime.Now}] 鼠标移动到 ({newX},{newY})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：{ex.Message}");
            }
        }

        private static void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                Console.WriteLine("鼠标移动器已停止");
            }
        }

        // 获取屏幕尺寸
        private static uint GetScreenWidth()
        {
            return (uint)SystemParametersInfo(0x0010, 0, out _, 0);
        }

        private static uint GetScreenHeight()
        {
            return (uint)SystemParametersInfo(0x0011, 0, out _, 0);
        }

        [DllImport("user32.dll")]
        private static extern uint SystemParametersInfo(uint uiAction, uint uiParam, out IntPtr pvParam, uint fWinIni);
    }
}
