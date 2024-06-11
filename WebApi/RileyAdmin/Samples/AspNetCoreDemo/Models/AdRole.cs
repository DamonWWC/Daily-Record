using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

/// <summary>
/// 角色
/// </summary>
public partial class AdRole
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
    /// 父级Id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 角色类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 数据范围
    /// </summary>
    public int DataScope { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 隐藏
    /// </summary>
    public ulong Hidden { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

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
