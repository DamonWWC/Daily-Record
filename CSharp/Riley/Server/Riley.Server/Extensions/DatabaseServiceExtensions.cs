using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Riley.Server.Auth.Extensions;
using Riley.Server.Configuration;
using Riley.Server.Data;
using Riley.Server.Services;
using Riley.Server.Tools;

namespace Riley.Server.Extensions
{
    /// <summary>
    /// 数据库服务扩展
    /// </summary>
    public static class DatabaseServiceExtensions
    {
        /// <summary>
        /// 添加多数据库支持的EntityFramework服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddMultiDatabaseSupport(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
            
            if (databaseSettings == null)
            {
                throw new InvalidOperationException("数据库配置未找到");
            }

            // 注册配置加密服务
            services.AddSingleton<IConfigurationEncryptionService, ConfigurationEncryptionService>();

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var encryptionService = serviceProvider.GetRequiredService<IConfigurationEncryptionService>();
                ConfigureDatabaseProviderTool.ConfigureDatabaseProvider(options, databaseSettings, encryptionService);
            });
            services.AddAuthDbContext(( serviceProvider,options) =>
            {
                var encryptionService = serviceProvider.GetRequiredService<IConfigurationEncryptionService>();
                ConfigureDatabaseProviderTool.ConfigureDatabaseProvider(options, databaseSettings, encryptionService);
            });

            return services;
        }

     
    }
}
