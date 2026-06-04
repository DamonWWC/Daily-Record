using BCrypt.Net;
using Microsoft.Extensions.Logging;
using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Services
{
    /// <summary>
    /// 用户管理服务实现
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(IUserRepository userRepository, ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有活跃用户
        /// </summary>
        /// <returns>用户列表</returns>
        public async Task<IEnumerable<UserInfo>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetActiveUsersAsync();
                return users.Select(ConvertToUserInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活跃用户列表时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 根据ID获取用户详细信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        public async Task<ApiResponse<UserInfo>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserInfo>.Fail($"未找到ID为{userId}的用户");
                }

                var userInfo = ConvertToUserInfo(user);
                return ApiResponse<UserInfo>.Ok(userInfo, "获取用户信息成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户ID {UserId} 时发生错误", userId);
                return ApiResponse<UserInfo>.Fail("获取用户信息时发生错误", ex.Message);
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="request">创建用户请求</param>
        /// <returns>创建结果</returns>
        public async Task<ApiResponse<UserInfo>> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                // 检查用户名是否已存在
                if (await IsUsernameExistsAsync(request.Username))
                {
                    return ApiResponse<UserInfo>.Fail("用户名已存在");
                }

                // 检查邮箱是否已存在
                if (await IsEmailExistsAsync(request.Email))
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
                    Role = request.Role ?? "User",
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateUserAsync(user);
                var userInfo = ConvertToUserInfo(createdUser);

                _logger.LogInformation("创建新用户成功：{Username} (ID: {UserId})", createdUser.Username, createdUser.Id);
                return ApiResponse<UserInfo>.Ok(userInfo, "用户创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户时发生错误，用户名: {Username}", request.Username);
                return ApiResponse<UserInfo>.Fail("创建用户时发生错误", ex.Message);
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新用户请求</param>
        /// <returns>更新结果</returns>
        public async Task<ApiResponse<UserInfo>> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    return ApiResponse<UserInfo>.Fail($"未找到ID为{userId}的用户");
                }

                // 检查邮箱是否被其他用户使用
                if (await IsEmailExistsAsync(request.Email, userId))
                {
                    return ApiResponse<UserInfo>.Fail("该邮箱已被其他用户使用");
                }

                // 更新用户信息
                existingUser.Name = request.Name;
                existingUser.Email = request.Email;
                existingUser.Phone = request.Phone;
                existingUser.Role = request.Role;
                existingUser.IsActive = request.IsActive;
                existingUser.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
                var userInfo = ConvertToUserInfo(updatedUser);

                _logger.LogInformation("更新用户信息成功：{Username} (ID: {UserId})", updatedUser.Username, updatedUser.Id);
                return ApiResponse<UserInfo>.Ok(userInfo, "用户信息更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户ID {UserId} 时发生错误", userId);
                return ApiResponse<UserInfo>.Fail("更新用户信息时发生错误", ex.Message);
            }
        }

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>删除结果</returns>
        public async Task<ApiResponse<bool>> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.Fail($"未找到ID为{userId}的用户");
                }

                var result = await _userRepository.DeleteUserAsync(userId);
                if (result)
                {
                    _logger.LogInformation("删除用户成功：{Username} (ID: {UserId})", user.Username, userId);
                    return ApiResponse<bool>.Ok(true, "用户删除成功");
                }
                else
                {
                    return ApiResponse<bool>.Fail("删除用户失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户ID {UserId} 时发生错误", userId);
                return ApiResponse<bool>.Fail("删除用户时发生错误", ex.Message);
            }
        }

        /// <summary>
        /// 检查邮箱是否已存在
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="excludeUserId">排除的用户ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                return excludeUserId == null || user.Id != excludeUserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查邮箱是否存在时发生错误，邮箱: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="excludeUserId">排除的用户ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return false;
                }

                return excludeUserId == null || user.Id != excludeUserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户名是否存在时发生错误，用户名: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// 将User实体转换为UserInfo
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>用户信息</returns>
        private static UserInfo ConvertToUserInfo(User user)
        {
            return new UserInfo
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
        }
    }
}
