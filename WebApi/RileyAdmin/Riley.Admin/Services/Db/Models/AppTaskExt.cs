using System;
using System.Collections.Generic;

namespace Riley.Admin.Services.Db.Models;

/// <summary>
/// 任务邮件
/// </summary>
public partial class AppTaskExt
{
    /// <summary>
    /// 任务Id
    /// </summary>
    public string TaskId { get; set; } = null!;

    /// <summary>
    /// 报警邮件，多个邮件地址则逗号分隔
    /// </summary>
    public string? AlarmEmail { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 添加用户Id
    /// </summary>
    public long? CreatedUserId { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// 修改用户Id
    /// </summary>
    public long? ModifiedUserId { get; set; }
}
