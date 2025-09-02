using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using WPFDeveloper.Attributes;

namespace WPFDeveloper.Services
{
    /// <summary>
    /// 视图自动注册服务接口
    /// </summary>
    public interface IViewAutoRegistration
    {
        /// <summary>
        /// 自动注册所有标记的视图
        /// </summary>
        /// <param name="services">服务集合</param>
        void RegisterViews(IServiceCollection services);

        /// <summary>
        /// 获取所有注册的视图信息
        /// </summary>
        /// <returns>视图信息列表</returns>
        IReadOnlyList<ViewInfo> GetRegisteredViews();

        /// <summary>
        /// 根据键获取视图类型
        /// </summary>
        /// <param name="key">视图键</param>
        /// <returns>视图类型</returns>
        Type? GetViewType(string key);
    }

    /// <summary>
    /// 视图信息
    /// </summary>
    public class ViewInfo
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Group { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }
        public Type ViewType { get; set; } = null!;       
    }

    /// <summary>
    /// 视图自动注册服务实现
    /// </summary>
    public class ViewAutoRegistration : IViewAutoRegistration
    {
        private readonly Dictionary<string, ViewInfo> _registeredViews = new(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<ViewAutoRegistration> _logger;

        public ViewAutoRegistration(ILogger<ViewAutoRegistration>? logger)
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ViewAutoRegistration>.Instance;
        }

        public void RegisterViews(IServiceCollection services)
        {
            try
            {
                // 获取当前程序集
                var assembly = Assembly.GetExecutingAssembly();
                
                // 查找所有标记了 ViewAttribute 的类
                var viewTypes = assembly.GetTypes()
                    .Where(type => type.IsClass && 
                                  !type.IsAbstract && 
                                  type.GetCustomAttribute<ViewAttribute>() != null)
                    .ToList();

                _logger.LogInformation("发现 {Count} 个标记的视图类", viewTypes.Count);

                foreach (var viewType in viewTypes)
                {
                    var viewAttribute = viewType.GetCustomAttribute<ViewAttribute>()!;
                    
                    try
                    {
                        RegisterSingleView(services, viewType, viewAttribute);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "注册视图失败: {ViewType}", viewType.Name);
                    }
                }

                _logger.LogInformation("视图自动注册完成，共注册 {Count} 个视图", _registeredViews.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "视图自动注册过程中发生错误");
                throw;
            }
        }

        private void RegisterSingleView(IServiceCollection services, Type viewType, ViewAttribute viewAttribute)
        {
            // 检查是否已经注册
            if (_registeredViews.ContainsKey(viewAttribute.Key))
            {
                _logger.LogWarning("视图键 '{Key}' 已存在，跳过注册 {ViewType}", viewAttribute.Key, viewType.Name);
                return;
            }

            // 创建视图信息
            var viewInfo = new ViewInfo
            {
                Key = viewAttribute.Key,
                DisplayName = viewAttribute.DisplayName ?? viewType.Name.Replace("View", ""),
                Group = viewAttribute.Group,
                Order = viewAttribute.Order,
                IsEnabled = viewAttribute.IsEnabled,
                ViewType = viewType,              
            };

            // 根据生命周期注册到 DI 容器
            //switch (viewAttribute.Lifetime)
            //{
            //    case Attributes.ServiceLifetime.Singleton:
            //        services.AddSingleton(viewType);
            //        break;
            //    case Attributes.ServiceLifetime.Scoped:
            //        services.AddScoped(viewType);
            //        break;
            //    case Attributes.ServiceLifetime.Transient:
            //    default:
            //        services.AddTransient(viewType);
            //        //services.AddKeyedTransient(viewType, viewAttribute.Key);
            //        //services.AddKeyedTransient(viewType, viewAttribute.Key);
            //        break;
            //}

            // 添加到注册表
            _registeredViews[viewAttribute.Key] = viewInfo;

            _logger.LogDebug("注册视图: {Key} -> {ViewType} ({Lifetime})", 
                viewAttribute.Key, viewType.Name, viewAttribute.Lifetime);
        }

        public IReadOnlyList<ViewInfo> GetRegisteredViews()
        {
            return _registeredViews.Values
                .Where(v => v.IsEnabled)
                .OrderBy(v => v.Order)
                .ThenBy(v => v.DisplayName)
                .ToList();
        }

        public Type? GetViewType(string key)
        {
            return _registeredViews.TryGetValue(key, out var viewInfo) ? viewInfo.ViewType : null;
        }

        /// <summary>
        /// 将已注册的视图信息复制到另一个实例
        /// </summary>
        /// <param name="target">目标实例</param>
        public void CopyRegisteredViewsTo(ViewAutoRegistration target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            foreach (var kvp in _registeredViews)
            {
                target._registeredViews[kvp.Key] = kvp.Value;
            }

            _logger.LogDebug("已复制 {Count} 个视图注册信息", _registeredViews.Count);
        }
    }
}
