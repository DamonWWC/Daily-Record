using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Riley.Server.Auth.Data;
using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Services
{
    /// <summary>
    /// 用户仓储实现
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AuthDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 根据用户名获取用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户信息</returns>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据用户名获取用户信息时发生错误，用户名: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// 根据邮箱获取用户信息
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>用户信息</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据邮箱获取用户信息时发生错误，邮箱: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// 根据ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据ID获取用户信息时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>创建的用户信息</returns>
        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功创建用户：{Username} (ID: {UserId})", user.Username, user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户时发生错误，用户名: {Username}", user.Username);
                throw;
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>更新的用户信息</returns>
        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功更新用户信息：{Username} (ID: {UserId})", user.Username, user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息时发生错误，用户ID: {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return false;
                }

                // 软删除：设置IsActive为false
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功删除用户：{Username} (ID: {UserId})", user.Username, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 获取所有活跃用户
        /// </summary>
        /// <returns>活跃用户列表</returns>
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Username)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活跃用户列表时发生错误");
                throw;
            }
        }
    }
}
