using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Flyleaf.Common;
using Microsoft.Extensions.Logging;

namespace Flyleaf.Services
{
    /// <summary>
    /// 动态控件加载器 - 负责动态加载FlyleafLib1.dll中的LiveControl
    /// </summary>
    public class DynamicControlLoader : IDisposable
    {
        private static readonly Lazy<DynamicControlLoader> _instance = new(() => new DynamicControlLoader());
        public static DynamicControlLoader Instance => _instance.Value;

        private Assembly? _loadedAssembly;
        private Type? _liveControlType;
        private readonly ILogger<DynamicControlLoader>? _logger;
        private bool _disposed = false;

        private DynamicControlLoader()
        {
            // 可以注入日志记录器
            // _logger = loggerFactory?.CreateLogger<DynamicControlLoader>();
        }

        /// <summary>
        /// 初始化动态加载器
        /// </summary>
        /// <returns>是否初始化成功</returns>
        public bool Initialize()
        {
            if (_loadedAssembly != null && _liveControlType != null)
            {
                return true; // 已经初始化过了
            }

            try
            {
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Flyleaf", "FlyleafLib1.dll");
                
                if (!File.Exists(dllPath))
                {
                    LogError($"FlyleafLib1.dll not found at: {dllPath}");
                    return false;
                }

                _loadedAssembly = Assembly.LoadFrom(dllPath);
                _liveControlType = _loadedAssembly.GetType("FlyleafLib1.Controls.LiveControl");

                if (_liveControlType == null)
                {
                    LogError("LiveControl type not found in FlyleafLib1.dll");
                    return false;
                }

                LogInfo("DynamicControlLoader initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize DynamicControlLoader: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 创建LiveControl实例
        /// </summary>
        /// <returns>LiveControl实例，失败时返回null</returns>
        public FrameworkElement? CreateLiveControl()
        {
            if (!Initialize())
            {
                return null;
            }
            try
            {
                if (Activator.CreateInstance(_liveControlType!) is ILiveControl control)
                {
                    LogInfo("LiveControl instance created successfully");
                    control.StatusAction += StatusAction;
                    return control.GetInstance() as FrameworkElement;
                }
                else
                {
                    LogError("Failed to create LiveControl instance - invalid type");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception creating LiveControl instance: {ex.Message}", ex);
                return null;
            }
        }

        public void StatusAction(Status status)
        {

        }

        /// <summary>
        /// 获取CameraUrl依赖属性
        /// </summary>
        /// <returns>CameraUrl依赖属性，失败时返回null</returns>
        public DependencyProperty? GetCameraUrlProperty()
        {
            if (!Initialize())
            {
                return null;
            }
            try
            {
                var field = _liveControlType!.GetField("CameraUrlProperty", BindingFlags.Public | BindingFlags.Static);
                return field?.GetValue(null) as DependencyProperty;
            }
            catch (Exception ex)
            {
                LogError($"Failed to get CameraUrlProperty: {ex.Message}", ex);
                return null;
            }
        }
        /// <summary>
        /// 获取字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public FieldInfo? GetField(string fieldName)
        {
            if (!Initialize())
            {
                return default;
            }
            try
            {
                var field = _liveControlType!.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                return field;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public PropertyInfo? GetProperty(string property) 
        {
            if (!Initialize())
            {
                return default;
            }
            try
            {
                var propertyInfo = _liveControlType!.GetProperty(property, BindingFlags.Public | BindingFlags.Static|BindingFlags.Instance);
                return propertyInfo;
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        /// <summary>
        /// 设置控件的CameraUrl属性
        /// </summary>
        /// <param name="control">控件实例</param>
        /// <param name="url">视频URL</param>
        /// <returns>是否设置成功</returns>
        public bool SetCameraUrl(FrameworkElement control, string? url)
        {
            if (control == null)
            {
                LogError("Control is null when setting CameraUrl");
                return false;
            }
            try
            {
                var field = GetField("CameraUrlProperty");
                var cameraUrlProperty = field?.GetValue(null) as DependencyProperty;
                if (cameraUrlProperty != null)
                {
                    control.SetValue(cameraUrlProperty, url);
                    LogInfo($"CameraUrl set to: {url ?? "null"}");
                    return true;
                }
                else
                {
                    LogError("CameraUrlProperty not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to set CameraUrl: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 调用控件的TakeSnapShot方法
        /// </summary>
        /// <param name="control">控件实例</param>
        /// <param name="fileName">文件名</param>
        /// <returns>是否调用成功</returns>
        public bool TakeSnapshot(FrameworkElement control, string? fileName = null)
        {
            if (control == null)
            {
                LogError("Control is null when taking snapshot");
                return false;
            }

            try
            {
                var method = control.GetType().GetMethod("TakeSnapShot");
                if (method != null)
                {
                    method.Invoke(control, new object[] { fileName });
                    LogInfo($"Snapshot taken: {fileName ?? "default"}");
                    return true;
                }
                else
                {
                    LogError("TakeSnapShot method not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to take snapshot: {ex.Message}", ex);
                return false;
            }
        }

        private void LogInfo(string message)
        {
            _logger?.LogInformation(message);
            System.Diagnostics.Debug.WriteLine($"[DynamicControlLoader] INFO: {message}");
        }

        private void LogError(string message, Exception? ex = null)
        {
            _logger?.LogError(ex, message);
            System.Diagnostics.Debug.WriteLine($"[DynamicControlLoader] ERROR: {message}");
            if (ex != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DynamicControlLoader] EXCEPTION: {ex}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // 注意：Assembly无法直接卸载，只能在AppDomain卸载时释放
                _loadedAssembly = null;
                _liveControlType = null;
                _disposed = true;
                LogInfo("DynamicControlLoader disposed");
            }
        }
    }
}