using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Riley.Server.Auth.Models;
using Riley.Server.Auth.Services;

namespace Riley.Server.Auth.Controllers
{
    /// <summary>
    /// 用户管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // 所有用户管理操作都需要认证
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有活跃用户
        /// </summary>
        /// <returns>用户列表</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserInfo>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserInfo>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserInfo>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserInfo>>>> GetUsers()
        {
            try
            {
                var users = await _userManagementService.GetActiveUsersAsync();
                return Ok(ApiResponse<IEnumerable<UserInfo>>.Ok(users, "获取用户列表成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表时发生未处理的异常");
                return StatusCode(500, ApiResponse<IEnumerable<UserInfo>>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserInfo>>> GetUser(int id)
        {
            try
            {
                var result = await _userManagementService.GetUserByIdAsync(id);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户ID {UserId} 时发生未处理的异常", id);
                return StatusCode(500, ApiResponse<UserInfo>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="request">创建用户请求</param>
        /// <returns>创建的用户信息</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserInfo>>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    
                    _logger.LogWarning("创建用户请求验证失败：{Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<UserInfo>.Fail($"请求参数验证失败: {string.Join(", ", errors)}"));
                }

                var result = await _userManagementService.CreateUserAsync(request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("已存在") || result.Message.Contains("已被注册"))
                    {
                        return Conflict(result);
                    }
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户时发生未处理的异常");
                return StatusCode(500, ApiResponse<UserInfo>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="request">更新用户请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserInfo>>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    
                    _logger.LogWarning("更新用户请求验证失败：{Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<UserInfo>.Fail($"请求参数验证失败: {string.Join(", ", errors)}"));
                }

                var result = await _userManagementService.UpdateUserAsync(id, request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("未找到"))
                    {
                        return NotFound(result);
                    }
                    if (result.Message.Contains("已被其他用户使用"))
                    {
                        return Conflict(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户ID {UserId} 时发生未处理的异常", id);
                return StatusCode(500, ApiResponse<UserInfo>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
        {
            try
            {
                var result = await _userManagementService.DeleteUserAsync(id);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("未找到"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户ID {UserId} 时发生未处理的异常", id);
                return StatusCode(500, ApiResponse<bool>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 检查邮箱是否已存在
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="excludeUserId">排除的用户ID（可选）</param>
        /// <returns>检查结果</returns>
        [HttpGet("check-email")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists([FromQuery] string email, [FromQuery] int? excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(ApiResponse<bool>.Fail("邮箱地址不能为空"));
                }

                var exists = await _userManagementService.IsEmailExistsAsync(email, excludeUserId);
                return Ok(ApiResponse<bool>.Ok(exists, exists ? "邮箱已存在" : "邮箱可用"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查邮箱是否存在时发生未处理的异常，邮箱: {Email}", email);
                return StatusCode(500, ApiResponse<bool>.Fail("服务器内部错误，请稍后重试"));
            }
        }

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="excludeUserId">排除的用户ID（可选）</param>
        /// <returns>检查结果</returns>
        [HttpGet("check-username")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckUsernameExists([FromQuery] string username, [FromQuery] int? excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(ApiResponse<bool>.Fail("用户名不能为空"));
                }

                var exists = await _userManagementService.IsUsernameExistsAsync(username, excludeUserId);
                return Ok(ApiResponse<bool>.Ok(exists, exists ? "用户名已存在" : "用户名可用"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户名是否存在时发生未处理的异常，用户名: {Username}", username);
                return StatusCode(500, ApiResponse<bool>.Fail("服务器内部错误，请稍后重试"));
            }
        }
    }
}
