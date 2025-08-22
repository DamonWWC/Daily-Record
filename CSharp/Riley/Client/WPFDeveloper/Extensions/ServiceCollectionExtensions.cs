using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFDeveloper.Services;
using WPFDeveloper.Views;

namespace WPFDeveloper.Extensions
{
    /// <summary>
    /// ServiceCollection 扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加视图自动注册服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddViewAutoRegistration(this IServiceCollection services)
        {
            // 创建临时的自动注册实例来执行注册（不依赖 DI 容器）
            var tempAutoRegistration = new ViewAutoRegistration(null!); // 临时实例，不需要日志
            
            // 在 ServiceProvider 构建前执行自动注册
            tempAutoRegistration.RegisterViews(services);
            
            // 注册自动注册服务实例（用于后续查询视图类型）
            services.AddSingleton<IViewAutoRegistration>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ViewAutoRegistration>>();
                var autoRegistration = new ViewAutoRegistration(logger);
                
                // 重新执行注册以填充内部字典（这次不会重复注册到DI，因为已经注册过了）
                // 但会填充 _registeredViews 字典用于后续查询
                tempAutoRegistration.CopyRegisteredViewsTo(autoRegistration);
                
                return autoRegistration;
            });

            return services;
        }

        /// <summary>
        /// 添加 WPF 开发者服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddWpfDeveloperServices(this IServiceCollection services)
        {
            // 添加日志
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // 添加核心服务
            //services.AddSingleton<IViewRegistry, ViewRegistry>();
            //services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INavigationRegistry, NavigationRegistry>();
            //services.AddSingleton<IViewFactory, ViewFactory>();

            // 添加外部进程宿主服务
            services.AddSingleton<IExternalProcessHostService, ExternalProcessHostService>();

            // 添加视图自动注册
            services.AddViewAutoRegistration();

            // 添加 ViewModels
            services.AddTransient<ViewModels.MainWindowViewModel>();
            
            // 添加主窗口
            services.AddTransient<MainWindow>();
            

            return services;
        }
    }
}
