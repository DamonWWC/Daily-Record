using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riley.Server.Auth.Data;
using Riley.Server.Configuration;
using Riley.Server.Data;

namespace Riley.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthDbContext _authDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(ApplicationDbContext context,AuthDbContext authDbContext, IConfiguration configuration, ILogger<DatabaseController> logger)
        {
            _context = context;
            _authDbContext = authDbContext;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 获取数据库信息
        /// </summary>
        [HttpGet("info")]
        public async Task<ActionResult<object>> GetDatabaseInfo()
        {
            try
            {
                var databaseSettings = _configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
                
                var info = new
                {
                    Provider = databaseSettings?.Provider ?? "Unknown",
                    ProviderName = _context.Database.ProviderName,
                    DatabaseName = _context.Database.GetDbConnection().Database,
                    Server = _context.Database.GetDbConnection().DataSource,
                    IsConnected = await _context.Database.CanConnectAsync(),
                    ConnectionString = GetMaskedConnectionString(databaseSettings?.Provider ?? "", databaseSettings?.ConnectionStrings)
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据库信息时发生错误");
                return StatusCode(500, "获取数据库信息时发生内部错误");
            }
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<ActionResult<object>> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                
                var result = new
                {
                    Success = canConnect,
                    Message = canConnect ? "数据库连接成功" : "数据库连接失败",
                    Timestamp = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试数据库连接时发生错误");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "数据库连接测试失败",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 获取数据库统计信息
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var userCount = await _authDbContext.Users.CountAsync();
                var productCount = await _context.Products.CountAsync();
                var orderCount = await _context.Orders.CountAsync();
                var orderItemCount = await _context.OrderItems.CountAsync();
                var activeUserCount = await _authDbContext.Users.CountAsync(u => u.IsActive);
                var availableProductCount = await _context.Products.CountAsync(p => p.IsAvailable);

                var statistics = new
                {
                    TotalUsers = userCount,
                    ActiveUsers = activeUserCount,
                    TotalProducts = productCount,
                    AvailableProducts = availableProductCount,
                    TotalOrders = orderCount,
                    TotalOrderItems = orderItemCount,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据库统计信息时发生错误");
                return StatusCode(500, "获取数据库统计信息时发生内部错误");
            }
        }

        /// <summary>
        /// 获取掩码后的连接字符串（隐藏敏感信息）
        /// </summary>
        private string GetMaskedConnectionString(string provider, ConnectionStrings? connectionStrings)
        {
            if (connectionStrings == null) return "未配置";

            var connectionString = provider.ToLowerInvariant() switch
            {
                "sqlserver" => connectionStrings.SqlServer,
                "postgresql" => connectionStrings.PostgreSQL,
                "mysql" => connectionStrings.MySQL,
                _ => "未知提供程序"
            };

            // 简单掩码处理，隐藏密码等敏感信息
            if (connectionString.Contains("Password="))
            {
                var parts = connectionString.Split(';');
                var maskedParts = parts.Select(part =>
                {
                    if (part.TrimStart().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                        part.TrimStart().StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Password=***";
                    }
                    return part;
                });
                return string.Join(';', maskedParts);
            }

            return connectionString;
        }
    }
}
