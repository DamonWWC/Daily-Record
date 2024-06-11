using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

/// <summary>
/// 权限
/// </summary>
public partial class AdPermission
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父级节点
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 权限编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 权限类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 视图Id
    /// </summary>
    public long? ViewId { get; set; }

    /// <summary>
    /// 路由命名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 重定向地址
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 隐藏
    /// </summary>
    public ulong Hidden { get; set; }

    /// <summary>
    /// 展开分组
    /// </summary>
    public ulong Opened { get; set; }

    /// <summary>
    /// 打开新窗口
    /// </summary>
    public ulong NewWindow { get; set; }

    /// <summary>
    /// 链接外显
    /// </summary>
    public ulong External { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    public ulong IsKeepAlive { get; set; }

    /// <summary>
    /// 是否固定
    /// </summary>
    public ulong IsAffix { get; set; }

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// 是否内嵌窗口
    /// </summary>
    public ulong IsIframe { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 启用
    /// </summary>
    public ulong Enabled { get; set; }

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
