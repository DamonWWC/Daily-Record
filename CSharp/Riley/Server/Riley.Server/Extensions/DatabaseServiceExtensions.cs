using Microsoft.EntityFrameworkCore;
using Riley.Server.Configuration;
using Riley.Server.Data;

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

            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                ConfigureDatabaseProvider(options, databaseSettings);
            });

            return services;
        }

        /// <summary>
        /// 配置数据库提供程序
        /// </summary>
        /// <param name="options">DbContext选项构建器</param>
        /// <param name="databaseSettings">数据库设置</param>
        private static void ConfigureDatabaseProvider(DbContextOptionsBuilder options, DatabaseSettings databaseSettings)
        {
            switch (databaseSettings.Provider.ToLowerInvariant())
            {
                case "sqlserver":
                    ConfigureSqlServer(options, databaseSettings.ConnectionStrings.SqlServer);
                    break;
                case "postgresql":
                    ConfigurePostgreSQL(options, databaseSettings.ConnectionStrings.PostgreSQL);
                    break;
                case "mysql":
                    ConfigureMySQL(options, databaseSettings.ConnectionStrings.MySQL);
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据库提供程序: {databaseSettings.Provider}");
            }
        }

        /// <summary>
        /// 配置SQL Server
        /// </summary>
        private static void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("SQL Server连接字符串未配置");
            }

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }

        /// <summary>
        /// 配置PostgreSQL
        /// </summary>
        private static void ConfigurePostgreSQL(DbContextOptionsBuilder options, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("PostgreSQL连接字符串未配置");
            }

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        }

        /// <summary>
        /// 配置MySQL
        /// </summary>
        private static void ConfigureMySQL(DbContextOptionsBuilder options, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MySQL连接字符串未配置");
            }

            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
    }
}
