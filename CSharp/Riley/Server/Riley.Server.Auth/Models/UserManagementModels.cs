using System.ComponentModel.DataAnnotations;

namespace Riley.Server.Auth.Models
{
    /// <summary>
    /// 创建用户请求模型
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [MaxLength(50, ErrorMessage = "用户名不能超过50个字符")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        [MaxLength(100, ErrorMessage = "姓名不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [MaxLength(100, ErrorMessage = "邮箱不能超过100个字符")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码至少6个字符")]
        [MaxLength(100, ErrorMessage = "密码不能超过100个字符")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        [MaxLength(20, ErrorMessage = "手机号不能超过20个字符")]
        public string? Phone { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [MaxLength(100, ErrorMessage = "角色不能超过100个字符")]
        public string? Role { get; set; } = "User";

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 更新用户请求模型
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        [MaxLength(100, ErrorMessage = "姓名不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [MaxLength(100, ErrorMessage = "邮箱不能超过100个字符")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        [MaxLength(20, ErrorMessage = "手机号不能超过20个字符")]
        public string? Phone { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [MaxLength(100, ErrorMessage = "角色不能超过100个字符")]
        public string? Role { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 用户列表查询请求
    /// </summary>
    public class UserListRequest
    {
        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 搜索关键词（匹配姓名、用户名、邮箱）
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// 是否包含非活跃用户
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// 角色筛选
        /// </summary>
        public string? Role { get; set; }
    }

    /// <summary>
    /// 分页用户列表响应
    /// </summary>
    public class PagedUserListResponse
    {
        /// <summary>
        /// 用户列表
        /// </summary>
        public IEnumerable<UserInfo> Users { get; set; } = new List<UserInfo>();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}
