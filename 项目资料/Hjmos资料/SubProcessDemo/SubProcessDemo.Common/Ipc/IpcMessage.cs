using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubProcessDemo.Common.Ipc;

/// <summary>
/// IPC 消息类型
/// </summary>
public enum IpcMessageType
{
    // 生命周期
    Ready = 1,
    ActivateModule = 2,
    ModuleActivated = 3,
    Shutdown = 4,
    HealthCheck = 5,
    HealthReport = 6,
    DeactivateModule = 7,     // 主进程 → 子进程：回收模块，隐藏窗口，退回 Ready
    ModuleDeactivated = 8,    // 子进程 → 主进程：确认已退回 Ready

    // 服务代理
    ServiceCall = 10,
    ServiceResponse = 11,
    TestServiceProxy = 12,    // 主进程 → 子进程：触发服务代理测试

    // 预热阶段报告
    WarmupProgress = 20,
    ColdStartResult = 30,
}

/// <summary>
/// IPC 消息
/// </summary>
public class IpcMessage
{
    public IpcMessageType Type { get; set; }
    public string? CallId { get; set; }
    public string? Payload { get; set; }
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public string ToJson() => JsonSerializer.Serialize(this, IpcJsonContext.Default.IpcMessage);

    public static IpcMessage? FromJson(string json)
    {
        try { return JsonSerializer.Deserialize(json, IpcJsonContext.Default.IpcMessage); }
        catch { return null; }
    }
}

[JsonSerializable(typeof(IpcMessage))]
[JsonSourceGenerationOptions(WriteIndented = false)]
internal partial class IpcJsonContext : JsonSerializerContext { }
