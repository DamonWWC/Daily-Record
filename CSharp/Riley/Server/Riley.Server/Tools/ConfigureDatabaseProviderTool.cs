using Microsoft.EntityFrameworkCore;
using Riley.Server.Configuration;
using Riley.Server.Services;

namespace Riley.Server.Tools
{
    public static class ConfigureDatabaseProviderTool
    {
        /// <summary>
        /// 配置数据库提供程序
        /// </summary>
        /// <param name="options">DbContext选项构建器</param>
        /// <param name="databaseSettings">数据库设置</param>
        /// <param name="encryptionService">配置加密服务</param>
        public static void ConfigureDatabaseProvider(DbContextOptionsBuilder options, DatabaseSettings databaseSettings, IConfigurationEncryptionService encryptionService)
        {
            switch (databaseSettings.Provider.ToLowerInvariant())
            {
                case "sqlserver":
                    var sqlServerConnectionString = encryptionService.DecryptConnectionStringIfNeeded(databaseSettings.ConnectionStrings.SqlServer);
                    ConfigureSqlServer(options, sqlServerConnectionString);
                    break;
                case "postgresql":
                    var postgresqlConnectionString = encryptionService.DecryptConnectionStringIfNeeded(databaseSettings.ConnectionStrings.PostgreSQL);
                    ConfigurePostgreSQL(options, postgresqlConnectionString);
                    break;
                case "mysql":
                    var mysqlConnectionString = encryptionService.DecryptConnectionStringIfNeeded(databaseSettings.ConnectionStrings.MySQL);
                    ConfigureMySQL(options, mysqlConnectionString);
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
