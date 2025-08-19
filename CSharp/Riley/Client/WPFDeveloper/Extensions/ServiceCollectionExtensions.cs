using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFDeveloper.Services;

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
            // 注册自动注册服务
            services.AddSingleton<IViewAutoRegistration>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ViewAutoRegistration>>();
                var autoRegistration = new ViewAutoRegistration(logger);
                
                // 执行自动注册
                autoRegistration.RegisterViews(services);
                
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
            services.AddSingleton<IViewRegistry, ViewRegistry>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INavigationRegistry, NavigationRegistry>();
            services.AddSingleton<IViewFactory, ViewFactory>();

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
