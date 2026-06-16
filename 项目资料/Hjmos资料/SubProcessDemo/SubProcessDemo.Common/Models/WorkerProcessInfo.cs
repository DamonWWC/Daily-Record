namespace SubProcessDemo.Common.Models;

/// <summary>
/// 预热池中进程的状态信息
/// </summary>
public class WorkerProcessInfo
{
    public string WorkerId { get; set; } = "";
    public int Pid { get; set; }
    public string PipeName { get; set; } = "";
    public WorkerState State { get; set; } = WorkerState.WarmingUp;
    public string? AssignedModule { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public double WarmupElapsedMs { get; set; }
    public double ActivationElapsedMs { get; set; }
}

public enum WorkerState
{
    WarmingUp,
    Ready,
    Active,
    Faulted,
    Shutdown
}
