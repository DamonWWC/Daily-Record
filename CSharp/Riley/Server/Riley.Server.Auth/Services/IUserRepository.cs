using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Services
{
    /// <summary>
    /// 用户仓储接口
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 根据用户名获取用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户信息</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// 根据邮箱获取用户信息
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>用户信息</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// 根据ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<User?> GetUserByIdAsync(int userId);

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>创建的用户信息</returns>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>更新的用户信息</returns>
        Task<User> UpdateUserAsync(User user);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteUserAsync(int userId);

        /// <summary>
        /// 获取所有活跃用户
        /// </summary>
        /// <returns>活跃用户列表</returns>
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }
}
