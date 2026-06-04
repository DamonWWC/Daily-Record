using System.Collections.Concurrent;
using System.Text.Json;

namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket消息处理器接口
/// </summary>
public interface IWebSocketMessageHandler
{
    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    /// <param name="message">WebSocket消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理任务</returns>
    Task HandleMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// 基于消息类型的WebSocket消息处理器
/// </summary>
public class TypedWebSocketMessageHandler : IWebSocketMessageHandler
{
    private readonly ConcurrentDictionary<string, Func<WebSocketMessage, CancellationToken, Task>> _handlers = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public TypedWebSocketMessageHandler()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="handler">处理器</param>
    public void RegisterHandler(string messageType, Func<WebSocketMessage, CancellationToken, Task> handler)
    {
        _handlers[messageType] = handler;
    }

    /// <summary>
    /// 注册强类型消息处理器
    /// </summary>
    /// <typeparam name="T">消息数据类型</typeparam>
    /// <param name="messageType">消息类型</param>
    /// <param name="handler">处理器</param>
    public void RegisterHandler<T>(string messageType, Func<T, CancellationToken, Task> handler)
    {
        _handlers[messageType] = async (message, cancellationToken) =>
        {
            var data = message.DeserializeJson<T>(_jsonOptions);
            if (data != null)
            {
                await handler(data, cancellationToken);
            }
        };
    }

    /// <summary>
    /// 注册JSON消息处理器
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="handler">处理器</param>
    public void RegisterJsonHandler<T>(Func<T, CancellationToken, Task> handler) where T : class
    {
        var messageType = typeof(T).Name;
        RegisterHandler(messageType, handler);
    }

    public async Task HandleMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        if (message.MessageType != WebSocketMessageType.Text || string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        // 尝试解析消息类型
        var messageType = ExtractMessageType(message.Text);
        if (string.IsNullOrEmpty(messageType))
        {
            return;
        }

        if (_handlers.TryGetValue(messageType, out var handler))
        {
            await handler(message, cancellationToken);
        }
    }

    /// <summary>
    /// 从消息中提取消息类型
    /// </summary>
    /// <param name="messageText">消息文本</param>
    /// <returns>消息类型</returns>
    protected virtual string? ExtractMessageType(string messageText)
    {
        try
        {
            using var document = JsonDocument.Parse(messageText);
            if (document.RootElement.TryGetProperty("type", out var typeProperty))
            {
                return typeProperty.GetString();
            }
            if (document.RootElement.TryGetProperty("messageType", out var messageTypeProperty))
            {
                return messageTypeProperty.GetString();
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return null;
    }
}

/// <summary>
/// WebSocket消息路由器
/// </summary>
public class WebSocketMessageRouter
{
    private readonly ConcurrentDictionary<string, List<IWebSocketMessageHandler>> _routeHandlers = new();
    private readonly List<IWebSocketMessageHandler> _globalHandlers = new();

    /// <summary>
    /// 添加路由处理器
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="handler">处理器</param>
    public void AddRouteHandler(string route, IWebSocketMessageHandler handler)
    {
        _routeHandlers.AddOrUpdate(route, 
            new List<IWebSocketMessageHandler> { handler },
            (key, existing) => 
            {
                existing.Add(handler);
                return existing;
            });
    }

    /// <summary>
    /// 添加全局处理器
    /// </summary>
    /// <param name="handler">处理器</param>
    public void AddGlobalHandler(IWebSocketMessageHandler handler)
    {
        _globalHandlers.Add(handler);
    }

    /// <summary>
    /// 路由消息到相应的处理器
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="message">消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>路由任务</returns>
    public async Task RouteMessageAsync(string route, WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();

        // 执行全局处理器
        foreach (var handler in _globalHandlers)
        {
            tasks.Add(handler.HandleMessageAsync(message, cancellationToken));
        }

        // 执行路由特定的处理器
        if (_routeHandlers.TryGetValue(route, out var handlers))
        {
            foreach (var handler in handlers)
            {
                tasks.Add(handler.HandleMessageAsync(message, cancellationToken));
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }
}

/// <summary>
/// 简单的函数式消息处理器
/// </summary>
public class FunctionalWebSocketMessageHandler : IWebSocketMessageHandler
{
    private readonly Func<WebSocketMessage, CancellationToken, Task> _handler;

    public FunctionalWebSocketMessageHandler(Func<WebSocketMessage, CancellationToken, Task> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public FunctionalWebSocketMessageHandler(Func<WebSocketMessage, Task> handler)
    {
        _handler = (message, _) => handler(message);
    }

    public FunctionalWebSocketMessageHandler(Action<WebSocketMessage> handler)
    {
        _handler = (message, _) =>
        {
            handler(message);
            return Task.CompletedTask;
        };
    }

    public async Task HandleMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        await _handler(message, cancellationToken);
    }
}

/// <summary>
/// 消息过滤器接口
/// </summary>
public interface IWebSocketMessageFilter
{
    /// <summary>
    /// 过滤消息
    /// </summary>
    /// <param name="message">消息</param>
    /// <returns>是否通过过滤</returns>
    bool Filter(WebSocketMessage message);
}

/// <summary>
/// 基于消息类型的过滤器
/// </summary>
public class MessageTypeFilter : IWebSocketMessageFilter
{
    private readonly HashSet<WebSocketMessageType> _allowedTypes;

    public MessageTypeFilter(params WebSocketMessageType[] allowedTypes)
    {
        _allowedTypes = new HashSet<WebSocketMessageType>(allowedTypes);
    }

    public bool Filter(WebSocketMessage message)
    {
        return _allowedTypes.Contains(message.MessageType);
    }
}

/// <summary>
/// 基于文本内容的过滤器
/// </summary>
public class TextContentFilter : IWebSocketMessageFilter
{
    private readonly Func<string, bool> _predicate;

    public TextContentFilter(Func<string, bool> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public bool Filter(WebSocketMessage message)
    {
        return message.MessageType == WebSocketMessageType.Text && 
               !string.IsNullOrEmpty(message.Text) && 
               _predicate(message.Text);
    }
}

/// <summary>
/// 组合过滤器
/// </summary>
public class CompositeFilter : IWebSocketMessageFilter
{
    private readonly List<IWebSocketMessageFilter> _filters;
    private readonly bool _requireAll;

    public CompositeFilter(bool requireAll = true, params IWebSocketMessageFilter[] filters)
    {
        _filters = new List<IWebSocketMessageFilter>(filters);
        _requireAll = requireAll;
    }

    public bool Filter(WebSocketMessage message)
    {
        if (_filters.Count == 0)
        {
            return true;
        }

        return _requireAll 
            ? _filters.All(filter => filter.Filter(message))
            : _filters.Any(filter => filter.Filter(message));
    }
}
