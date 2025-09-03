using Microsoft.IdentityModel.Tokens;
using Riley.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Riley.Server.Services
{
    /// <summary>
    /// JWT服务实现
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 生成JWT令牌
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>JWT令牌</returns>
        public string GenerateToken(User user)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey未配置");
                var issuer = jwtSettings["Issuer"] ?? "Riley.Server";
                var audience = jwtSettings["Audience"] ?? "Riley.Client";
                var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.GivenName, user.Name),
                    new(ClaimTypes.Role, user.Role ?? "User"),
                    new("UserId", user.Id.ToString()),
                    new("Username", user.Username),
                    new("IsActive", user.IsActive.ToString())
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                _logger.LogInformation("为用户 {Username} (ID: {UserId}) 生成JWT令牌", user.Username, user.Id);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成JWT令牌时发生错误，用户: {Username}", user.Username);
                throw;
            }
        }

        /// <summary>
        /// 验证JWT令牌
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>用户ID，验证失败返回null</returns>
        public int? ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey未配置");
                var issuer = jwtSettings["Issuer"] ?? "Riley.Server";
                var audience = jwtSettings["Audience"] ?? "Riley.Client";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // 不允许时间偏差
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("JWT令牌验证失败：算法不匹配");
                    return null;
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("JWT令牌验证失败：无法获取用户ID");
                    return null;
                }

                _logger.LogDebug("JWT令牌验证成功，用户ID: {UserId}", userId);
                return userId;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT令牌验证失败：令牌已过期");
                return null;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT令牌验证失败：{Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT令牌验证时发生未知错误");
                return null;
            }
        }

        /// <summary>
        /// 获取令牌过期时间（分钟）
        /// </summary>
        /// <returns>过期时间（分钟）</returns>
        public int GetTokenExpirationMinutes()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            return int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        }
    }
}
