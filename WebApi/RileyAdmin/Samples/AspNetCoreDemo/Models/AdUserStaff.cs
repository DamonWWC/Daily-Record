using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

/// <summary>
/// 用户员工
/// </summary>
public partial class AdUserStaff
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    public string? JobNumber { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    /// 入职时间
    /// </summary>
    public DateTime? EntryTime { get; set; }

    /// <summary>
    /// 企业微信名片
    /// </summary>
    public string? WorkWeChatCard { get; set; }

    /// <summary>
    /// 个人简介
    /// </summary>
    public string? Introduce { get; set; }

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
