using BCrypt.Net;
using Microsoft.Extensions.Logging;
using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Services
{
    /// <summary>
    /// 认证服务实现
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // 验证用户凭据
                var user = await ValidateUserAsync(request.Username, request.Password);
                if (user == null)
                {
                    _logger.LogWarning("用户登录失败：用户名或密码错误，用户名: {Username}", request.Username);
                    return ApiResponse<LoginResponse>.Fail("用户名或密码错误");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("用户登录失败：账户已被禁用，用户名: {Username}", request.Username);
                    return ApiResponse<LoginResponse>.Fail("账户已被禁用，请联系管理员");
                }

                // 生成JWT令牌
                var token = _jwtService.GenerateToken(user);
                var expirationMinutes = _jwtService.GetTokenExpirationMinutes();

                var response = new LoginResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds(),
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    }
                };

                _logger.LogInformation("用户登录成功：{Username} (ID: {UserId})", user.Username, user.Id);
                return ApiResponse<LoginResponse>.Ok(response, "登录成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录时发生错误，用户名: {Username}", request.Username);
                return ApiResponse<LoginResponse>.Fail("登录失败，请稍后重试", ex.Message);
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>注册响应</returns>
        public async Task<ApiResponse<UserInfo>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // 检查用户名是否已存在
                var existingUser = await GetUserByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    return ApiResponse<UserInfo>.Fail("用户名已存在");
                }

                // 检查邮箱是否已存在
                var existingEmail = await GetUserByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    return ApiResponse<UserInfo>.Fail("邮箱已被注册");
                }

                // 创建新用户
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = new User
                {
                    Username = request.Username,
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Phone = request.Phone,
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateUserAsync(user);

                var userInfo = new UserInfo
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Name = createdUser.Name,
                    Email = createdUser.Email,
                    Phone = createdUser.Phone,
                    Role = createdUser.Role,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt
                };

                _logger.LogInformation("用户注册成功：{Username} (ID: {UserId})", createdUser.Username, createdUser.Id);
                return ApiResponse<UserInfo>.Ok(userInfo, "注册成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册时发生错误，用户名: {Username}", request.Username);
                return ApiResponse<UserInfo>.Fail("注册失败，请稍后重试", ex.Message);
            }
        }

        /// <summary>
        /// 验证用户凭据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>用户信息，验证失败返回null</returns>
        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            try
            {
                var user = await GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return null;
                }
                var aa = BCrypt.Net.BCrypt.InterrogateHash("$2a$11$J5KZJq1vSbF8i3dS9WHnoOLVJbHgMB8kClHKVm8wZZ9V9UPhvB2zG");
                // 验证密码
                var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                return isPasswordValid ? user : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证用户凭据时发生错误，用户名: {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// 根据用户名获取用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户信息</returns>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetUserByUsernameAsync(username);
        }

        /// <summary>
        /// 根据邮箱获取用户信息
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>用户信息</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        /// <summary>
        /// 根据ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }
    }
}
