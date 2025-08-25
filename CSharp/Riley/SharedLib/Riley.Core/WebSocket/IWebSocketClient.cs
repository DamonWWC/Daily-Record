using System.Net.WebSockets;

namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket客户端接口
/// </summary>
public interface IWebSocketClient : IDisposable
{
    #region 属性

    /// <summary>
    /// 当前连接状态
    /// </summary>
    WebSocketConnectionState State { get; }

    /// <summary>
    /// 连接URL
    /// </summary>
    string Url { get; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 选中的子协议
    /// </summary>
    string? SubProtocol { get; }

    /// <summary>
    /// 重连尝试次数
    /// </summary>
    int ReconnectAttempts { get; }

    /// <summary>
    /// 最后一次心跳时间
    /// </summary>
    DateTime? LastHeartbeat { get; }

    /// <summary>
    /// 配置选项
    /// </summary>
    WebSocketOptions Options { get; }

    #endregion

    #region 事件

    /// <summary>
    /// 连接成功事件
    /// </summary>
    event EventHandler<WebSocketConnectedEventArgs>? Connected;

    /// <summary>
    /// 断开连接事件
    /// </summary>
    event EventHandler<WebSocketDisconnectedEventArgs>? Disconnected;

    /// <summary>
    /// 消息接收事件
    /// </summary>
    event EventHandler<WebSocketMessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// 错误事件
    /// </summary>
    event EventHandler<WebSocketErrorEventArgs>? Error;

    /// <summary>
    /// 重连事件
    /// </summary>
    event EventHandler<WebSocketReconnectingEventArgs>? Reconnecting;

    #endregion

    #region 连接管理

    /// <summary>
    /// 连接到WebSocket服务器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>连接任务</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开WebSocket连接
    /// </summary>
    /// <param name="closeStatus">关闭状态</param>
    /// <param name="statusDescription">状态描述</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>断开任务</returns>
    Task DisconnectAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string? statusDescription = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重新连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>重连任务</returns>
    Task ReconnectAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 消息发送

    /// <summary>
    /// 发送文本消息
    /// </summary>
    /// <param name="message">文本消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendTextAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送二进制消息
    /// </summary>
    /// <param name="data">二进制数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendBinaryAsync(byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送JSON消息
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要发送的对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendJsonAsync<T>(T obj, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送WebSocket消息
    /// </summary>
    /// <param name="message">WebSocket消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送心跳消息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendHeartbeatAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 消息接收

    /// <summary>
    /// 开始接收消息（异步循环）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>接收任务</returns>
    Task StartReceivingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止接收消息
    /// </summary>
    void StopReceiving();

    #endregion

    #region 工具方法

    /// <summary>
    /// 等待连接建立
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>等待任务</returns>
    Task WaitForConnectionAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查连接状态
    /// </summary>
    /// <returns>连接是否正常</returns>
    bool CheckConnection();

    /// <summary>
    /// 获取连接统计信息
    /// </summary>
    /// <returns>连接统计</returns>
    WebSocketStatistics GetStatistics();

    #endregion
}

/// <summary>
/// WebSocket连接统计信息
/// </summary>
public class WebSocketStatistics
{
    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime? ConnectedAt { get; set; }

    /// <summary>
    /// 连接持续时间
    /// </summary>
    public TimeSpan? ConnectedDuration => ConnectedAt.HasValue ? DateTime.UtcNow - ConnectedAt.Value : null;

    /// <summary>
    /// 发送消息数量
    /// </summary>
    public long MessagesSent { get; set; }

    /// <summary>
    /// 接收消息数量
    /// </summary>
    public long MessagesReceived { get; set; }

    /// <summary>
    /// 发送字节数
    /// </summary>
    public long BytesSent { get; set; }

    /// <summary>
    /// 接收字节数
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 重连次数
    /// </summary>
    public int ReconnectCount { get; set; }

    /// <summary>
    /// 错误次数
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 最后一次错误时间
    /// </summary>
    public DateTime? LastErrorAt { get; set; }

    /// <summary>
    /// 最后一次错误信息
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// 心跳次数
    /// </summary>
    public long HeartbeatCount { get; set; }

    /// <summary>
    /// 最后一次心跳时间
    /// </summary>
    public DateTime? LastHeartbeatAt { get; set; }
}
