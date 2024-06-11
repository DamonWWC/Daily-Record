using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

/// <summary>
/// 角色部门
/// </summary>
public partial class AdRoleOrg
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 部门Id
    /// </summary>
    public long OrgId { get; set; }

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
