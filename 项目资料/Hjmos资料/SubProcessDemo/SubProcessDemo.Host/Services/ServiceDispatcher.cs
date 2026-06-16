using System.Reflection;
using System.Text.Json;
using SubProcessDemo.Common.Ipc;
using SubProcessDemo.Common.Services;
using SubProcessDemo.Host.Pool;

namespace SubProcessDemo.Host.Services;

/// <summary>
/// 策略③：服务调度器。
/// 接收子进程的服务代理调用请求，反射调用主进程中的真实服务，返回结果。
/// </summary>
public class ServiceDispatcher
{
    private readonly ProcessPoolManager _poolManager;
    private readonly Dictionary<string, object> _services = new();

    public event Action<string>? OnLog;

    public ServiceDispatcher(ProcessPoolManager poolManager)
    {
        _poolManager = poolManager;

        // 注册真实服务
        _services[typeof(IDataService).FullName!] = new DataService();

        // 监听子进程的服务调用
        _poolManager.ServiceCallReceived += HandleServiceCallAsync;
    }

    private async Task HandleServiceCallAsync(string workerId, IpcMessage msg)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<ServiceCallPayload>(msg.Payload ?? "{}");
            if (payload == null) return;

            Log($"服务代理调用: {workerId} → {payload.ServiceType}.{payload.Method}()");

            // 查找服务实例
            if (!_services.TryGetValue(payload.ServiceType, out var service))
            {
                Log($"  未找到服务: {payload.ServiceType}");
                await SendResponse(workerId, msg.CallId, null);
                return;
            }

            // 反射查找方法
            var method = service.GetType().GetMethod(payload.Method);
            if (method == null)
            {
                Log($"  未找到方法: {payload.Method}");
                await SendResponse(workerId, msg.CallId, null);
                return;
            }

            // 反序列化参数
            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length && i < payload.Args.Length; i++)
            {
                args[i] = JsonSerializer.Deserialize(payload.Args[i], parameters[i].ParameterType);
            }

            // 调用真实服务
            var result = method.Invoke(service, args);

            // 序列化结果并返回
            var resultJson = JsonSerializer.Serialize(result, result?.GetType() ?? typeof(object));
            Log($"  返回结果: {resultJson[..Math.Min(resultJson.Length, 80)]}...");

            await SendResponse(workerId, msg.CallId, resultJson);
        }
        catch (Exception ex)
        {
            Log($"  服务调用异常: {ex.Message}");
            await SendResponse(workerId, msg.CallId, null);
        }
    }

    private async Task SendResponse(string workerId, string? callId, string? resultJson)
    {
        await _poolManager.SendToWorkerAsync(workerId, new IpcMessage
        {
            Type = IpcMessageType.ServiceResponse,
            CallId = callId,
            Payload = resultJson
        });
    }

    private void Log(string msg) => OnLog?.Invoke($"[ServiceDispatcher] {msg}");
}
