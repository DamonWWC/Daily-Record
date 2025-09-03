using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Services
{
    /// <summary>
    /// 用户管理服务接口
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// 获取所有活跃用户
        /// </summary>
        /// <returns>用户列表</returns>
        Task<IEnumerable<UserInfo>> GetActiveUsersAsync();

        /// <summary>
        /// 根据ID获取用户详细信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<ApiResponse<UserInfo>> GetUserByIdAsync(int userId);

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="request">创建用户请求</param>
        /// <returns>创建结果</returns>
        Task<ApiResponse<UserInfo>> CreateUserAsync(CreateUserRequest request);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新用户请求</param>
        /// <returns>更新结果</returns>
        Task<ApiResponse<UserInfo>> UpdateUserAsync(int userId, UpdateUserRequest request);

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>删除结果</returns>
        Task<ApiResponse<bool>> DeleteUserAsync(int userId);

        /// <summary>
        /// 检查邮箱是否已存在
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="excludeUserId">排除的用户ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null);

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="excludeUserId">排除的用户ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null);
    }
}
