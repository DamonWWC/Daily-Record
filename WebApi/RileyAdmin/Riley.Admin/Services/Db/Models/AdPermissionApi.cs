using System;
using System.Collections.Generic;

namespace Riley.Admin.Services.Db.Models;

/// <summary>
/// 权限接口
/// </summary>
public partial class AdPermissionApi
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 权限Id
    /// </summary>
    public long PermissionId { get; set; }

    /// <summary>
    /// 接口Id
    /// </summary>
    public long ApiId { get; set; }

    /// <summary>
    /// 创建者用户Id
    /// </summary>
    public long? CreatedUserId { get; set; }

    /// <summary>
    /// 创建者用户名
    /// </summary>
    public string? CreatedUserName { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreatedUserRealName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}
