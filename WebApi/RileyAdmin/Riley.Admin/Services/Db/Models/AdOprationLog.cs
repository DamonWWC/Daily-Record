using System;
using System.Collections.Generic;

namespace Riley.Admin.Services.Db.Models;

/// <summary>
/// 操作日志
/// </summary>
public partial class AdOprationLog
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
    /// 接口名称
    /// </summary>
    public string? ApiLabel { get; set; }

    /// <summary>
    /// 接口地址
    /// </summary>
    public string? ApiPath { get; set; }

    /// <summary>
    /// 接口提交方法
    /// </summary>
    public string? ApiMethod { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public string? Ip { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? Os { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public string? BrowserInfo { get; set; }

    /// <summary>
    /// 耗时（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// 操作状态
    /// </summary>
    public ulong Status { get; set; }

    /// <summary>
    /// 操作消息
    /// </summary>
    public string? Msg { get; set; }

    /// <summary>
    /// 操作结果
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// 操作参数
    /// </summary>
    public string? Params { get; set; }

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
