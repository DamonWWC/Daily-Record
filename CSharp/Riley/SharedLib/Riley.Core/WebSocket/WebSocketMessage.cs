using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket消息类型
/// </summary>
public enum WebSocketMessageType
{
    /// <summary>
    /// 文本消息
    /// </summary>
    Text,
    
    /// <summary>
    /// 二进制消息
    /// </summary>
    Binary,
    
    /// <summary>
    /// 关闭消息
    /// </summary>
    Close
}

/// <summary>
/// WebSocket消息
/// </summary>
public class WebSocketMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public WebSocketMessageType MessageType { get; set; }

    /// <summary>
    /// 消息内容（文本）
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 消息内容（二进制）
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// 消息时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 消息ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 是否为最终消息片段
    /// </summary>
    public bool IsEndOfMessage { get; set; } = true;

    /// <summary>
    /// 创建文本消息
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <returns>WebSocket消息</returns>
    public static WebSocketMessage CreateText(string text)
    {
        return new WebSocketMessage
        {
            MessageType = WebSocketMessageType.Text,
            Text = text,
            Data = Encoding.UTF8.GetBytes(text)
        };
    }

    /// <summary>
    /// 创建二进制消息
    /// </summary>
    /// <param name="data">二进制数据</param>
    /// <returns>WebSocket消息</returns>
    public static WebSocketMessage CreateBinary(byte[] data)
    {
        return new WebSocketMessage
        {
            MessageType = WebSocketMessageType.Binary,
            Data = data,
            Text = Encoding.UTF8.GetString(data)
        };
    }

    /// <summary>
    /// 创建JSON消息
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="options">JSON序列化选项</param>
    /// <returns>WebSocket消息</returns>
    public static WebSocketMessage CreateJson<T>(T obj, JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(obj, options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        return CreateText(json);
    }

    /// <summary>
    /// 将消息反序列化为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="options">JSON反序列化选项</param>
    /// <returns>反序列化的对象</returns>
    public T? DeserializeJson<T>(JsonSerializerOptions? options = null)
    {
        if (MessageType != WebSocketMessageType.Text || string.IsNullOrEmpty(Text))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(Text, options ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 获取消息的字节表示
    /// </summary>
    /// <returns>字节数组</returns>
    public byte[] GetBytes()
    {
        return Data ?? Array.Empty<byte>();
    }

    /// <summary>
    /// 获取消息的文本表示
    /// </summary>
    /// <returns>文本内容</returns>
    public string GetText()
    {
        return Text ?? string.Empty;
    }
}

/// <summary>
/// WebSocket连接状态
/// </summary>
public enum WebSocketConnectionState
{
    /// <summary>
    /// 未连接
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// 连接中
    /// </summary>
    Connecting,
    
    /// <summary>
    /// 已连接
    /// </summary>
    Connected,
    
    /// <summary>
    /// 断开连接中
    /// </summary>
    Disconnecting,
    
    /// <summary>
    /// 重连中
    /// </summary>
    Reconnecting,
    
    /// <summary>
    /// 连接失败
    /// </summary>
    Failed
}

/// <summary>
/// WebSocket事件参数基类
/// </summary>
public abstract class WebSocketEventArgs : EventArgs
{
    /// <summary>
    /// 事件时间戳
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// 连接事件参数
/// </summary>
public class WebSocketConnectedEventArgs : WebSocketEventArgs
{
    /// <summary>
    /// 连接URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 选中的子协议
    /// </summary>
    public string? SubProtocol { get; set; }
}

/// <summary>
/// 断开连接事件参数
/// </summary>
public class WebSocketDisconnectedEventArgs : WebSocketEventArgs
{
    /// <summary>
    /// 断开原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 关闭状态码
    /// </summary>
    public WebSocketCloseStatus? CloseStatus { get; set; }

    /// <summary>
    /// 是否为预期的断开
    /// </summary>
    public bool IsExpected { get; set; }

    /// <summary>
    /// 是否会尝试重连
    /// </summary>
    public bool WillReconnect { get; set; }
}

/// <summary>
/// 消息接收事件参数
/// </summary>
public class WebSocketMessageReceivedEventArgs : WebSocketEventArgs
{
    /// <summary>
    /// 接收到的消息
    /// </summary>
    public WebSocketMessage Message { get; set; } = null!;
}

/// <summary>
/// 错误事件参数
/// </summary>
public class WebSocketErrorEventArgs : WebSocketEventArgs
{
    /// <summary>
    /// 异常信息
    /// </summary>
    public Exception Exception { get; set; } = null!;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message => Exception?.Message ?? "Unknown error";

    /// <summary>
    /// 错误上下文
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// 重连事件参数
/// </summary>
public class WebSocketReconnectingEventArgs : WebSocketEventArgs
{
    /// <summary>
    /// 重连尝试次数
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// 下次重连延迟时间（毫秒）
    /// </summary>
    public int DelayMs { get; set; }

    /// <summary>
    /// 是否取消重连
    /// </summary>
    public bool Cancel { get; set; }
}
