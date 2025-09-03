using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Riley.Server.Auth.Models;
using Riley.Server.Auth.Services;
using System.Security.Claims;

namespace Riley.Server.Auth.Controllers
{
    /// <summary>
    /// 认证控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    
                    _logger.LogWarning("登录请求验证失败：{Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<LoginResponse>.Fail($"请求参数验证失败: {string.Join(", ", errors)}"));
                }

                var result = await _authService.LoginAsync(request);
                
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录时发生未处理的异常");
                return StatusCode(500, ApiResponse<LoginResponse>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>注册响应</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserInfo>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    
                    _logger.LogWarning("注册请求验证失败：{Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<UserInfo>.Fail($"请求参数验证失败: {string.Join(", ", errors)}"));
                }

                var result = await _authService.RegisterAsync(request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("已存在") || result.Message.Contains("已被注册"))
                    {
                        return Conflict(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册时发生未处理的异常");
                return StatusCode(500, ApiResponse<UserInfo>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>当前用户信息</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("无法从JWT令牌中获取用户ID");
                    return Unauthorized(ApiResponse<UserInfo>.Fail("无效的访问令牌"));
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("用户不存在，用户ID: {UserId}", userId);
                    return NotFound(ApiResponse<UserInfo>.Fail("用户不存在"));
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("用户账户已被禁用，用户ID: {UserId}", userId);
                    return Unauthorized(ApiResponse<UserInfo>.Fail("账户已被禁用"));
                }

                var userInfo = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Ok(ApiResponse<UserInfo>.Ok(userInfo, "获取用户信息成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前用户信息时发生未处理的异常");
                return StatusCode(500, ApiResponse<UserInfo>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <returns>新的访问令牌</returns>
        [HttpPost("refresh")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("无法从JWT令牌中获取用户ID");
                    return Unauthorized(ApiResponse<LoginResponse>.Fail("无效的访问令牌"));
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("用户不存在，用户ID: {UserId}", userId);
                    return NotFound(ApiResponse<LoginResponse>.Fail("用户不存在"));
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("用户账户已被禁用，用户ID: {UserId}", userId);
                    return Unauthorized(ApiResponse<LoginResponse>.Fail("账户已被禁用"));
                }

                // 重新生成令牌
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

                _logger.LogInformation("令牌刷新成功，用户: {Username} (ID: {UserId})", user.Username, user.Id);
                return Ok(ApiResponse<LoginResponse>.Ok(response, "令牌刷新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新令牌时发生未处理的异常");
                return StatusCode(500, ApiResponse<LoginResponse>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 用户登出（可选实现，主要用于记录日志）
        /// </summary>
        /// <returns>登出响应</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object?>> Logout()
        {
            try
            {
                var usernameClaim = User.FindFirst(ClaimTypes.Name) ?? User.FindFirst("Username");
                var username = usernameClaim?.Value ?? "Unknown";

                _logger.LogInformation("用户登出，用户名: {Username}", username);
                return Ok(ApiResponse<object?>.Ok(null, "登出成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登出时发生未处理的异常");
                return StatusCode(500, ApiResponse<object?>.Fail("服务器内部错误，请稍后重试"));
            }
        }
    }
}
