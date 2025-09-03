using Riley.Server.Models;

namespace Riley.Server.Services
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>注册响应</returns>
        Task<ApiResponse<UserInfo>> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 验证用户凭据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>用户信息，验证失败返回null</returns>
        Task<User?> ValidateUserAsync(string username, string password);

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
    }
}
