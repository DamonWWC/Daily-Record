using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace WPFDeveloper.Utils
{
    /// <summary>
    /// 系统程序路径工具类
    /// 提供常见系统程序的路径查找功能
    /// </summary>
    public static class SystemProgramPaths
    {
        /// <summary>
        /// 获取常见的系统程序路径
        /// </summary>
        /// <returns>程序名称和路径的字典</returns>
        public static Dictionary<string, string> GetCommonSystemPrograms()
        {
            var programs = new Dictionary<string, string>();

            // Windows系统目录
            var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            // 常见系统程序
            var commonPrograms = new[]
            {
                ("记事本", "notepad.exe"),
                ("计算器", "calc.exe"),
                ("画图", "mspaint.exe"),
                ("写字板", "write.exe"),
                ("字符映射表", "charmap.exe"),
                ("任务管理器", "taskmgr.exe"),
                ("注册表编辑器", "regedit.exe"),
                ("系统配置", "msconfig.exe"),
                ("磁盘清理", "cleanmgr.exe"),
                ("磁盘碎片整理", "dfrgui.exe")
            };

            foreach (var (name, fileName) in commonPrograms)
            {
                var systemPath = Path.Combine(systemDir, fileName);
                var windowsPath = Path.Combine(windowsDir, fileName);

                if (File.Exists(systemPath))
                {
                    programs[name] = systemPath;
                }
                else if (File.Exists(windowsPath))
                {
                    programs[name] = windowsPath;
                }
            }

            // 尝试查找其他常见程序
            TryAddProgramFromRegistry(programs, "Visual Studio Code", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "Code.exe");
            TryAddProgramFromRegistry(programs, "Google Chrome", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "chrome.exe");
            TryAddProgramFromRegistry(programs, "Firefox", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "firefox.exe");

            return programs;
        }

        /// <summary>
        /// 验证程序路径是否有效
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <returns>是否有效</returns>
        public static bool IsValidProgramPath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                if (!File.Exists(path))
                    return false;

                var extension = Path.GetExtension(path).ToLowerInvariant();
                return extension == ".exe" || extension == ".com" || extension == ".bat" || extension == ".cmd";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取程序的友好名称
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <returns>友好名称</returns>
        public static string GetProgramFriendlyName(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return "未知程序";

                // 尝试获取文件版本信息
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
                if (!string.IsNullOrWhiteSpace(versionInfo.ProductName))
                    return versionInfo.ProductName;

                if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
                    return versionInfo.FileDescription;

                // 返回文件名（不含扩展名）
                return Path.GetFileNameWithoutExtension(path);
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(path) ?? "未知程序";
            }
        }

        /// <summary>
        /// 尝试从注册表添加程序
        /// </summary>
        private static void TryAddProgramFromRegistry(Dictionary<string, string> programs, string programName, string registryPath, string executableName)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(registryPath);
                if (key == null) return;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null) continue;

                    var displayName = subKey.GetValue("DisplayName")?.ToString();
                    if (string.IsNullOrEmpty(displayName) || !displayName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var installLocation = subKey.GetValue("InstallLocation")?.ToString();
                    if (string.IsNullOrEmpty(installLocation)) continue;

                    var executablePath = Path.Combine(installLocation, executableName);
                    if (File.Exists(executablePath))
                    {
                        programs[programName] = executablePath;
                        break;
                    }
                }
            }
            catch
            {
                // 忽略注册表访问异常
            }
        }

        /// <summary>
        /// 获取推荐的测试程序列表
        /// </summary>
        /// <returns>推荐程序列表</returns>
        public static List<(string Name, string Path, string Description)> GetRecommendedTestPrograms()
        {
            var programs = new List<(string Name, string Path, string Description)>();
            var systemPrograms = GetCommonSystemPrograms();

            foreach (var program in systemPrograms)
            {
                var description = program.Key switch
                {
                    "记事本" => "简单的文本编辑器，适合测试基本交互",
                    "计算器" => "系统计算器，测试按钮点击和数字输入",
                    "画图" => "图形编辑程序，测试鼠标绘制功能",
                    "写字板" => "富文本编辑器，测试复杂文本操作",
                    _ => "系统程序"
                };

                programs.Add((program.Key, program.Value, description));
            }

            return programs;
        }
    }
}
