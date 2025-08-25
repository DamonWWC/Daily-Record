namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket客户端配置选项
/// </summary>
public class WebSocketOptions
{
    /// <summary>
    /// WebSocket服务器URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 连接超时时间（毫秒）
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// 接收缓冲区大小
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 4096;

    /// <summary>
    /// 发送缓冲区大小
    /// </summary>
    public int SendBufferSize { get; set; } = 4096;

    /// <summary>
    /// 是否启用自动重连
    /// </summary>
    public bool EnableAutoReconnect { get; set; } = true;

    /// <summary>
    /// 重连间隔时间（毫秒）
    /// </summary>
    public int ReconnectIntervalMs { get; set; } = 5000;

    /// <summary>
    /// 最大重连次数（-1表示无限重连）
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = -1;

    /// <summary>
    /// 心跳间隔时间（毫秒，0表示禁用心跳）
    /// </summary>
    public int HeartbeatIntervalMs { get; set; } = 30000;

    /// <summary>
    /// 心跳超时时间（毫秒）
    /// </summary>
    public int HeartbeatTimeoutMs { get; set; } = 10000;

    /// <summary>
    /// 心跳消息内容
    /// </summary>
    public string HeartbeatMessage { get; set; } = "ping";

    /// <summary>
    /// 预期的心跳响应消息
    /// </summary>
    public string HeartbeatResponse { get; set; } = "pong";

    /// <summary>
    /// 自定义请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 支持的子协议
    /// </summary>
    public List<string> SubProtocols { get; set; } = new();

    /// <summary>
    /// 是否忽略SSL证书错误
    /// </summary>
    public bool IgnoreSslErrors { get; set; } = false;

    /// <summary>
    /// 用户代理
    /// </summary>
    public string UserAgent { get; set; } = "Riley.Core.WebSocket/1.0";

    /// <summary>
    /// 连接关闭时是否自动清理资源
    /// </summary>
    public bool AutoCleanupOnClose { get; set; } = true;

    /// <summary>
    /// 消息队列最大长度（0表示无限制）
    /// </summary>
    public int MaxMessageQueueLength { get; set; } = 1000;

    /// <summary>
    /// 是否启用消息压缩
    /// </summary>
    public bool EnableCompression { get; set; } = false;
}
