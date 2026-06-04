namespace Riley.Server.Configuration
{
    /// <summary>
    /// 数据库设置配置
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// 数据库提供程序类型
        /// </summary>
        public string Provider { get; set; } = "SqlServer";

        /// <summary>
        /// 连接字符串配置
        /// </summary>
        public ConnectionStrings ConnectionStrings { get; set; } = new();
    }

    /// <summary>
    /// 连接字符串配置
    /// </summary>
    public class ConnectionStrings
    {
        /// <summary>
        /// SQL Server连接字符串
        /// </summary>
        public string SqlServer { get; set; } = string.Empty;

        /// <summary>
        /// PostgreSQL连接字符串
        /// </summary>
        public string PostgreSQL { get; set; } = string.Empty;

        /// <summary>
        /// MySQL连接字符串
        /// </summary>
        public string MySQL { get; set; } = string.Empty;
    }

    /// <summary>
    /// 支持的数据库提供程序
    /// </summary>
    public static class DatabaseProviders
    {
        public const string SqlServer = "SqlServer";
        public const string PostgreSQL = "PostgreSQL";
        public const string MySQL = "MySQL";
    }
}
