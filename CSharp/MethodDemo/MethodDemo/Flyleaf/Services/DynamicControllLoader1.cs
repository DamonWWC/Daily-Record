using Flyleaf.Common;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace Flyleaf.Services
{
    /// <summary>
    /// 动态控件加载器 - 负责动态加载FlyleafLib1.dll中的LiveControl
    /// </summary>
    public class DynamicControlLoader1 : IDisposable
    {
        private static readonly Lazy<DynamicControlLoader1> _instance = new(() => new DynamicControlLoader1());
        public static DynamicControlLoader1 Instance => _instance.Value;
        private Assembly? _loadedAssembly;
        private IPlugin? _plugin;
        private readonly ILogger<DynamicControlLoader1>? _logger;
        private bool _disposed = false;

        private DynamicControlLoader1()
        {
            
        }

        

        /// <summary>
        /// 初始化动态加载器
        /// </summary>
        /// <returns>是否初始化成功</returns>
        public bool Initialize()
        {
            if (_loadedAssembly != null && _plugin != null)
            {
                return true; // 已经初始化过了
            }
            try
            {
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Flyleaf", "FlyleafLib1.dll");

                if (!File.Exists(dllPath))
                {
                    return false;
                }
                //var bb = Assembly.LoadFile(dllPath);
                _loadedAssembly = Assembly.LoadFrom(dllPath);
                var aa =  AppDomain.CurrentDomain.GetAssemblies();
                var pluginType = _loadedAssembly.GetTypes().FirstOrDefault(p => p.GetInterface("IPlugin") != null);
                if (pluginType == null)
                {
                    return false;
                }
                _plugin = Activator.CreateInstance(pluginType!) as IPlugin;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 创建LiveControl实例
        /// </summary>
        /// <returns>LiveControl实例，失败时返回null</returns>
        public ILiveControl? CreateLiveControl()
        {
            if (!Initialize())
            {
                return null;
            }
            try
            {
                var _liveControl = _plugin?.CreateControl();

                if (_liveControl is ILiveControl control)
                {
                    return control;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }      
        /// <summary>
        /// 设置控件的CameraUrl属性
        /// </summary>
        /// <param name="control">控件实例</param>
        /// <param name="url">视频URL</param>
        /// <returns>是否设置成功</returns>
        public bool SetCameraUrl(ILiveControl? control, string? url)
        {
            if (control == null)
            {
                return false;
            }
            try
            {
                control.PlayLive(url);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 调用控件的TakeSnapShot方法
        /// </summary>
        /// <param name="control">控件实例</param>
        /// <param name="fileName">文件名</param>
        /// <returns>是否调用成功</returns>
        public bool TakeSnapshot(ILiveControl? control, string? fileName = null)
        {
            if (control == null)
            {
                return false;
            }

            try
            {
                control.TakeSnapShot(fileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _loadedAssembly = null;
                _plugin = null;
                _disposed = true;
            }
        }
    }
}