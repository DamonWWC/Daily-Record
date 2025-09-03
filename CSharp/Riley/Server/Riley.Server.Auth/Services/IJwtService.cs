using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Services
{
    /// <summary>
    /// JWT服务接口
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成JWT令牌
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>JWT令牌</returns>
        string GenerateToken(User user);

        /// <summary>
        /// 验证JWT令牌
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>用户ID，验证失败返回null</returns>
        int? ValidateToken(string token);

        /// <summary>
        /// 获取令牌过期时间（分钟）
        /// </summary>
        /// <returns>过期时间（分钟）</returns>
        int GetTokenExpirationMinutes();
    }
}
