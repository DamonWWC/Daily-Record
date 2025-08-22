using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WPFDeveloper.Services
{
    /// <summary>
    /// 外部进程宿主服务接口
    /// 负责管理外部EXE程序的启动、嵌入和交互
    /// </summary>
    public interface IExternalProcessHostService
    {
        /// <summary>
        /// 启动并嵌入外部EXE程序到指定的宿主窗口句柄
        /// </summary>
        /// <param name="exePath">外部EXE程序的完整路径</param>
        /// <param name="hostHandle">宿主窗口句柄</param>
        /// <param name="arguments">启动参数（可选）</param>
        /// <returns>成功返回进程信息，失败返回null</returns>
        Task<Process?> StartAndEmbedProcessAsync(string exePath, IntPtr hostHandle, string? arguments = null);

        /// <summary>
        /// 调整嵌入进程的窗口大小和位置
        /// </summary>
        /// <param name="process">目标进程</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>操作是否成功</returns>
        bool ResizeEmbeddedProcess(Process process, int x, int y, int width, int height);

        /// <summary>
        /// 停止并清理嵌入的进程
        /// </summary>
        /// <param name="process">要停止的进程</param>
        /// <returns>操作是否成功</returns>
        bool StopEmbeddedProcess(Process process);

        /// <summary>
        /// 检查指定的EXE文件是否存在且可执行
        /// </summary>
        /// <param name="exePath">EXE文件路径</param>
        /// <returns>文件是否有效</returns>
        bool IsValidExecutable(string exePath);

        /// <summary>
        /// 获取当前所有嵌入的进程列表
        /// </summary>
        /// <returns>进程列表</returns>
        System.Collections.Generic.List<Process> GetEmbeddedProcesses();

        /// <summary>
        /// 进程启动成功事件
        /// </summary>
        event EventHandler<ProcessStartedEventArgs>? ProcessStarted;

        /// <summary>
        /// 进程退出事件
        /// </summary>
        event EventHandler<ProcessExitedEventArgs>? ProcessExited;

        /// <summary>
        /// 进程嵌入失败事件
        /// </summary>
        event EventHandler<ProcessEmbedFailedEventArgs>? ProcessEmbedFailed;
    }

    /// <summary>
    /// 进程启动成功事件参数
    /// </summary>
    public class ProcessStartedEventArgs : EventArgs
    {
        public Process Process { get; }
        public string ExecutablePath { get; }
        public DateTime StartTime { get; }

        public ProcessStartedEventArgs(Process process, string executablePath)
        {
            Process = process;
            ExecutablePath = executablePath;
            StartTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 进程退出事件参数
    /// </summary>
    public class ProcessExitedEventArgs : EventArgs
    {
        public int ProcessId { get; }
        public string ExecutablePath { get; }
        public int ExitCode { get; }
        public DateTime ExitTime { get; }

        public ProcessExitedEventArgs(int processId, string executablePath, int exitCode)
        {
            ProcessId = processId;
            ExecutablePath = executablePath;
            ExitCode = exitCode;
            ExitTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 进程嵌入失败事件参数
    /// </summary>
    public class ProcessEmbedFailedEventArgs : EventArgs
    {
        public string ExecutablePath { get; }
        public string ErrorMessage { get; }
        public Exception? Exception { get; }

        public ProcessEmbedFailedEventArgs(string executablePath, string errorMessage, Exception? exception = null)
        {
            ExecutablePath = executablePath;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }
}
