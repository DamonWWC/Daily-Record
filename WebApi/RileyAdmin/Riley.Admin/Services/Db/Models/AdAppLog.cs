using System;
using System.Collections.Generic;

namespace Riley.Admin.Services.Db.Models;

/// <summary>
/// 应用程序日志
/// </summary>
public partial class AdAppLog
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    public DateTime Logged { get; set; }

    public string? Level { get; set; }

    public string? Message { get; set; }

    public string? Logger { get; set; }

    public string? Properties { get; set; }

    public string? Callsite { get; set; }

    public string? Exception { get; set; }
}
