using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WPFDeveloper.Attributes;
using WPFDeveloper.Interop;

namespace WPFDeveloper.Services
{
    /// <summary>
    /// 外部进程宿主服务实现
    /// 负责管理外部EXE程序的启动、嵌入和交互
    /// </summary>
    [ServiceRegistration(typeof(IExternalProcessHostService), WPFDeveloper.Attributes.ServiceLifetime.Singleton)]
    public class ExternalProcessHostService : IExternalProcessHostService, IDisposable
    {
        private readonly ILogger<ExternalProcessHostService> _logger;
        private readonly List<Process> _embeddedProcesses;
        private readonly object _processLock = new();

        public ExternalProcessHostService(ILogger<ExternalProcessHostService> logger)
        {
            _logger = logger;
            _embeddedProcesses = new List<Process>();
        }

        #region 事件定义

        public event EventHandler<ProcessStartedEventArgs>? ProcessStarted;
        public event EventHandler<ProcessExitedEventArgs>? ProcessExited;
        public event EventHandler<ProcessEmbedFailedEventArgs>? ProcessEmbedFailed;

        #endregion

        #region 公共方法

        public async Task<Process?> StartAndEmbedProcessAsync(string exePath, IntPtr hostHandle, string? arguments = null)
        {
            try
            {
                _logger.LogInformation("开始启动外部进程: {ExePath}", exePath);

                // 验证可执行文件
                if (!IsValidExecutable(exePath))
                {
                    var errorMessage = $"无效的可执行文件: {exePath}";
                    _logger.LogError(errorMessage);
                    ProcessEmbedFailed?.Invoke(this, new ProcessEmbedFailedEventArgs(exePath, errorMessage));
                    return null;
                }

                // 验证宿主窗口句柄
                if (hostHandle == IntPtr.Zero)
                {
                    var errorMessage = "宿主窗口句柄无效";
                    _logger.LogError(errorMessage);
                    ProcessEmbedFailed?.Invoke(this, new ProcessEmbedFailedEventArgs(exePath, errorMessage));
                    return null;
                }

                // 创建进程启动信息
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                // 启动进程
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    var errorMessage = "无法启动进程";
                    _logger.LogError(errorMessage);
                    ProcessEmbedFailed?.Invoke(this, new ProcessEmbedFailedEventArgs(exePath, errorMessage));
                    return null;
                }

                _logger.LogInformation("进程启动成功，PID: {ProcessId}", process.Id);

                // 等待进程主窗口创建
                var embedded = await EmbedProcessAsync(process, hostHandle);
                if (!embedded)
                {
                    var errorMessage = "进程嵌入失败";
                    _logger.LogError(errorMessage);
                    ProcessEmbedFailed?.Invoke(this, new ProcessEmbedFailedEventArgs(exePath, errorMessage));
                    
                    // 清理失败的进程
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                        process.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "清理失败进程时出现异常");
                    }
                    
                    return null;
                }

                // 注册进程退出事件
                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) =>
                {
                    OnProcessExited(process, exePath);
                };

                // 添加到管理列表
                lock (_processLock)
                {
                    _embeddedProcesses.Add(process);
                }

                // 触发进程启动成功事件
                ProcessStarted?.Invoke(this, new ProcessStartedEventArgs(process, exePath));

                _logger.LogInformation("进程嵌入成功完成: {ExePath}", exePath);
                return process;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动和嵌入进程时发生异常: {ExePath}", exePath);
                ProcessEmbedFailed?.Invoke(this, new ProcessEmbedFailedEventArgs(exePath, ex.Message, ex));
                return null;
            }
        }

        public bool ResizeEmbeddedProcess(Process process, int x, int y, int width, int height)
        {
            try
            {
                if (process.HasExited)
                {
                    _logger.LogWarning("尝试调整已退出进程的窗口大小，PID: {ProcessId}", process.Id);
                    return false;
                }

                var mainWindowHandle = process.MainWindowHandle;
                if (mainWindowHandle == IntPtr.Zero)
                {
                    _logger.LogWarning("进程主窗口句柄无效，PID: {ProcessId}", process.Id);
                    return false;
                }

                // 调整窗口大小和位置
                var result = Win32Api.MoveWindow(mainWindowHandle, x, y, width, height, true);
                
                if (result)
                {
                    _logger.LogDebug("成功调整进程窗口大小，PID: {ProcessId}, 位置: ({X}, {Y}), 大小: {Width}x{Height}", 
                        process.Id, x, y, width, height);
                }
                else
                {
                    _logger.LogWarning("调整进程窗口大小失败，PID: {ProcessId}", process.Id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调整嵌入进程窗口大小时发生异常，PID: {ProcessId}", process.Id);
                return false;
            }
        }

        public bool StopEmbeddedProcess(Process process)
        {
            try
            {
                if (process.HasExited)
                {
                    _logger.LogInformation("进程已经退出，PID: {ProcessId}", process.Id);
                    RemoveProcessFromList(process);
                    return true;
                }

                _logger.LogInformation("正在停止嵌入的进程，PID: {ProcessId}", process.Id);

                // 尝试优雅地关闭进程
                if (!process.CloseMainWindow())
                {
                    _logger.LogWarning("无法优雅关闭进程，尝试强制终止，PID: {ProcessId}", process.Id);
                    
                    // 强制终止进程
                    process.Kill();
                }

                // 等待进程退出
                var exited = process.WaitForExit(5000); // 等待5秒
                if (!exited)
                {
                    _logger.LogWarning("进程未在超时时间内退出，强制终止，PID: {ProcessId}", process.Id);
                    process.Kill();
                    process.WaitForExit(2000); // 再等待2秒
                }

                RemoveProcessFromList(process);
                _logger.LogInformation("成功停止嵌入的进程，PID: {ProcessId}", process.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止嵌入进程时发生异常，PID: {ProcessId}", process.Id);
                RemoveProcessFromList(process);
                return false;
            }
        }

        public bool IsValidExecutable(string exePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(exePath))
                    return false;

                if (!File.Exists(exePath))
                    return false;

                var extension = Path.GetExtension(exePath).ToLowerInvariant();
                return extension == ".exe" || extension == ".com";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证可执行文件时发生异常: {ExePath}", exePath);
                return false;
            }
        }

        public List<Process> GetEmbeddedProcesses()
        {
            lock (_processLock)
            {
                // 清理已退出的进程
                _embeddedProcesses.RemoveAll(p => p.HasExited);
                return new List<Process>(_embeddedProcesses);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 异步嵌入进程到宿主窗口
        /// </summary>
        private async Task<bool> EmbedProcessAsync(Process process, IntPtr hostHandle)
        {
            const int maxRetries = 50; // 最大重试次数
            const int retryDelay = 100; // 重试间隔（毫秒）

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // 等待进程创建主窗口
                    process.Refresh();
                    
                    if (process.HasExited)
                    {
                        _logger.LogWarning("进程在嵌入前已退出，PID: {ProcessId}", process.Id);
                        return false;
                    }

                    var mainWindowHandle = process.MainWindowHandle;
                    if (mainWindowHandle != IntPtr.Zero)
                    {
                        return EmbedWindow(mainWindowHandle, hostHandle);
                    }

                    // 如果主窗口句柄为空，尝试枚举进程的所有窗口
                    var processWindows = GetProcessWindows(process.Id);
                    var mainWindow = processWindows.FirstOrDefault(w => IsMainWindow(w));
                    
                    if (mainWindow != IntPtr.Zero)
                    {
                        return EmbedWindow(mainWindow, hostHandle);
                    }

                    // 等待后重试
                    await Task.Delay(retryDelay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "嵌入进程窗口时发生异常，重试 {Retry}/{MaxRetries}", i + 1, maxRetries);
                }
            }

            _logger.LogError("在 {MaxRetries} 次重试后仍无法嵌入进程窗口，PID: {ProcessId}", maxRetries, process.Id);
            return false;
        }

        /// <summary>
        /// 嵌入指定窗口到宿主窗口
        /// </summary>
        private bool EmbedWindow(IntPtr windowHandle, IntPtr hostHandle)
        {
            try
            {
                _logger.LogDebug("开始嵌入窗口，窗口句柄: {WindowHandle}, 宿主句柄: {HostHandle}", 
                    windowHandle, hostHandle);

                // 设置窗口为子窗口
                var result = Win32Api.SetParent(windowHandle, hostHandle);
                if (result == IntPtr.Zero)
                {
                    _logger.LogError("设置父窗口失败");
                    return false;
                }

                // 修改窗口样式使其适合嵌入
                if (!Win32Api.MakeWindowEmbeddable(windowHandle))
                {
                    _logger.LogWarning("修改窗口样式失败，但继续尝试嵌入");
                }

                // 显示窗口
                Win32Api.ShowWindow(windowHandle, Win32Api.SW_SHOW);

                _logger.LogInformation("成功嵌入窗口");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "嵌入窗口时发生异常");
                return false;
            }
        }

        /// <summary>
        /// 获取指定进程的所有窗口句柄
        /// </summary>
        private List<IntPtr> GetProcessWindows(int processId)
        {
            var windows = new List<IntPtr>();

            Win32Api.EnumWindows((hWnd, lParam) =>
            {
                Win32Api.GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                if (windowProcessId == processId)
                {
                    windows.Add(hWnd);
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary>
        /// 判断是否为主窗口
        /// </summary>
        private bool IsMainWindow(IntPtr hWnd)
        {
            // 窗口必须可见且不是工具窗口
            if (!Win32Api.IsWindowVisible(hWnd))
                return false;

            // 获取窗口样式
            var style = Win32Api.GetWindowLong(hWnd, Win32Api.GWL_EXSTYLE);
            if ((style & Win32Api.WS_EX_TOOLWINDOW) != 0)
                return false;

            // 检查是否有标题
            var title = Win32Api.GetWindowTitle(hWnd);
            return !string.IsNullOrEmpty(title);
        }

        /// <summary>
        /// 处理进程退出事件
        /// </summary>
        private void OnProcessExited(Process process, string exePath)
        {
            try
            {
                var processId = process.Id;
                var exitCode = process.ExitCode;
                
                _logger.LogInformation("嵌入的进程已退出，PID: {ProcessId}, 退出代码: {ExitCode}", processId, exitCode);

                RemoveProcessFromList(process);
                ProcessExited?.Invoke(this, new ProcessExitedEventArgs(processId, exePath, exitCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理进程退出事件时发生异常");
            }
        }

        /// <summary>
        /// 从管理列表中移除进程
        /// </summary>
        private void RemoveProcessFromList(Process process)
        {
            lock (_processLock)
            {
                _embeddedProcesses.Remove(process);
            }

            try
            {
                process.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "释放进程资源时发生异常");
            }
        }

        #endregion

        #region 资源清理

        public void Dispose()
        {
            lock (_processLock)
            {
                foreach (var process in _embeddedProcesses.ToList())
                {
                    try
                    {
                        StopEmbeddedProcess(process);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "清理进程时发生异常，PID: {ProcessId}", process.Id);
                    }
                }
                _embeddedProcesses.Clear();
            }
        }

        #endregion
    }
}
