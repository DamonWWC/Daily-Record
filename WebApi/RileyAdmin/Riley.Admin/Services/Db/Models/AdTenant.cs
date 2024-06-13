using System;
using System.Collections.Generic;

namespace Riley.Admin.Services.Db.Models;

/// <summary>
/// 租户
/// </summary>
public partial class AdTenant
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 授权用户
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 授权部门
    /// </summary>
    public long OrgId { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    public string? TenantType { get; set; }

    /// <summary>
    /// 数据库注册键
    /// </summary>
    public string? DbKey { get; set; }

    /// <summary>
    /// 数据库
    /// </summary>
    public int? DbType { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 启用
    /// </summary>
    public ulong Enabled { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public string? Description { get; set; }

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

    /// <summary>
    /// 修改者用户Id
    /// </summary>
    public long? ModifiedUserId { get; set; }

    /// <summary>
    /// 修改者用户名
    /// </summary>
    public string? ModifiedUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? ModifiedUserRealName { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public ulong IsDeleted { get; set; }
}
