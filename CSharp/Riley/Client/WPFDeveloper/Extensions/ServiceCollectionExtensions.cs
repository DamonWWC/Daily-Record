using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFDeveloper.Services;

namespace WPFDeveloper.Extensions
{
    /// <summary>
    /// 手动注册的 ServiceCollection 扩展方法
    /// 注意：这个类不是 partial，避免与 SourceGenerator 生成的 ServiceCollectionExtensions 冲突
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        ///// <summary>
        ///// 添加视图自动注册服务
        ///// </summary>
        ///// <param name="services">服务集合</param>
        ///// <returns>服务集合</returns>
        public static IServiceCollection AddViewAutoRegistration(this IServiceCollection services)
        {
            // 注册自动注册服务实例（用于后续查询视图类型）
            services.AddSingleton<IViewAutoRegistration>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ViewAutoRegistration>>();
                var autoRegistration = new ViewAutoRegistration(logger);
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

            // 添加由 SourceGenerator 自动发现的服务
            services.AddGeneratedServices();
            services.AddViewAutoRegistration();
            return services;
        }
    }
}
