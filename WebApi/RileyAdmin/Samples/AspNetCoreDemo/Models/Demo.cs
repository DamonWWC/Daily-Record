using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

public partial class Demo
{
    public int Id { get; set; }

    public string? Label { get; set; }

    public DateTime? CreatedTime { get; set; }
}
