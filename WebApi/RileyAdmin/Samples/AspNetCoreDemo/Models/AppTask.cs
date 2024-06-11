using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

public partial class AppTask
{
    public string Id { get; set; } = null!;

    public string? Topic { get; set; }

    public string? Body { get; set; }

    public int Round { get; set; }

    public int Interval { get; set; }

    public string? IntervalArgument { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime LastRunTime { get; set; }

    public int CurrentRound { get; set; }

    public int ErrorTimes { get; set; }

    public int Status { get; set; }
}
