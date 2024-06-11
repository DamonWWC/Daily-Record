using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

public partial class AppTaskLog
{
    public string? TaskId { get; set; }

    public int Round { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public ulong Success { get; set; }

    public string? Exception { get; set; }

    public string? Remark { get; set; }

    public DateTime CreateTime { get; set; }
}
