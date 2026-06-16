using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using SubProcessDemo.Common.Ipc;

namespace SubProcessDemo.Common.Services;

/// <summary>
/// 策略③：服务代理。
/// 使用 DispatchProxy 拦截接口方法调用，通过命名管道转发到主进程执行。
/// 子进程中使用此代理替代本地服务实例，避免重复创建数据库连接、MQ 连接等。
/// </summary>
public class ServiceProxy : DispatchProxy
{
    private SimpleIpcChannel? _channel;
    private string _serviceTypeName = "";
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _pending = new();

    /// <summary>创建服务代理实例</summary>
    public static T Create<T>(SimpleIpcChannel channel) where T : class
    {
        var proxy = DispatchProxy.Create<T, ServiceProxy>();
        var sp = (ServiceProxy)(object)proxy;
        sp._channel = channel;
        sp._serviceTypeName = typeof(T).FullName ?? typeof(T).Name;

        // 监听响应
        channel.MessageReceived += msg =>
        {
            if (msg.Type == IpcMessageType.ServiceResponse && msg.CallId != null)
            {
                if (sp._pending.TryRemove(msg.CallId, out var tcs))
                    tcs.TrySetResult(msg.Payload);
            }
        };

        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || _channel == null)
            throw new InvalidOperationException("代理未初始化");

        var callId = Guid.NewGuid().ToString("N");
        var tcs = new TaskCompletionSource<string?>();
        _pending[callId] = tcs;

        // 构造 ServiceCall 消息
        var callPayload = JsonSerializer.Serialize(new ServiceCallPayload
        {
            ServiceType = _serviceTypeName,
            Method = targetMethod.Name,
            Args = args?.Select(a => JsonSerializer.Serialize(a, a?.GetType() ?? typeof(object))).ToArray() ?? Array.Empty<string>(),
            ArgTypeNames = targetMethod.GetParameters()
                .Select(p => p.ParameterType.FullName ?? p.ParameterType.Name)
                .ToArray()
        });

        // 发送请求
        _channel.SendAsync(new IpcMessage
        {
            Type = IpcMessageType.ServiceCall,
            CallId = callId,
            Payload = callPayload
        }).Wait();

        // 同步等待响应（带超时）
        if (!tcs.Task.Wait(TimeSpan.FromSeconds(10)))
        {
            _pending.TryRemove(callId, out _);
            throw new TimeoutException($"服务代理调用超时: {_serviceTypeName}.{targetMethod.Name}");
        }

        var resultJson = tcs.Task.Result;
        if (resultJson == null) return GetDefault(targetMethod.ReturnType);

        return JsonSerializer.Deserialize(resultJson, targetMethod.ReturnType);
    }

    private static object? GetDefault(Type type)
    {
        if (type.IsValueType) return Activator.CreateInstance(type);
        return null;
    }
}

/// <summary>服务调用负载</summary>
public class ServiceCallPayload
{
    public string ServiceType { get; set; } = "";
    public string Method { get; set; } = "";
    public string[] Args { get; set; } = Array.Empty<string>();
    public string[] ArgTypeNames { get; set; } = Array.Empty<string>();
}
