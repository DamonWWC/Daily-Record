using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

/// <summary>
/// 用户角色
/// </summary>
public partial class AdUserRole
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    public long RoleId { get; set; }

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
