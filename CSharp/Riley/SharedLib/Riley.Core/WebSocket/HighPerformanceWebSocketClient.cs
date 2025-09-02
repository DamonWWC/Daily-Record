using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Riley.Core.WebSocket;

/// <summary>
/// 高性能WebSocket客户端实现
/// 针对高吞吐量、大量小消息、低延迟、内存敏感、高并发连接等场景优化
/// </summary>
public class HighPerformanceWebSocketClient : IWebSocketClient
{
    private readonly ILogger<HighPerformanceWebSocketClient>? _logger;
    private readonly WebSocketOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WebSocketStatistics _statistics;
    
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private CancellationTokenSource? _heartbeatCancellationTokenSource;
    
    // 高性能优化：使用更轻量的锁和队列
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    
    // 使用Channel替代ConcurrentQueue，提供更好的异步性能
    private readonly Channel<WebSocketMessage> _messageChannel;
    private readonly ChannelWriter<WebSocketMessage> _messageWriter;
    private readonly ChannelReader<WebSocketMessage> _messageReader;
    
    // 内存池优化
    private readonly ArrayPool<byte> _arrayPool;
    private readonly MemoryPool<byte> _memoryPool;
    
    // 批处理优化
    private readonly List<WebSocketMessage> _batchBuffer;
    private readonly Timer _batchFlushTimer;
    private volatile bool _hasPendingMessages;
    
    // 状态管理优化
    private volatile WebSocketConnectionState _state = WebSocketConnectionState.Disconnected;
    private volatile bool _disposed = false;
    private volatile bool _isReceiving = false;
    private volatile int _reconnectAttempts = 0;
    
    // 性能计数器
    private long _totalMessagesProcessed;
    private long _totalBytesProcessed;
    private readonly object _statsLock = new();

    public HighPerformanceWebSocketClient(IOptions<WebSocketOptions> options, ILogger<HighPerformanceWebSocketClient>? logger = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _statistics = new WebSocketStatistics();
        
        // 创建高性能Channel
        var channelOptions = new BoundedChannelOptions(_options.MaxMessageQueueLength > 0 ? _options.MaxMessageQueueLength : 10000)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // 丢弃最旧的消息
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = false // 避免同步延续
        };
        
        _messageChannel = Channel.CreateBounded<WebSocketMessage>(channelOptions);
        _messageWriter = _messageChannel.Writer;
        _messageReader = _messageChannel.Reader;
        
        // 初始化内存池
        _arrayPool = ArrayPool<byte>.Shared;
        _memoryPool = MemoryPool<byte>.Shared;
        
        // 初始化批处理
        _batchBuffer = new List<WebSocketMessage>(100); // 预分配批处理缓冲区
        _batchFlushTimer = new Timer(FlushBatchMessages, null, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultBufferSize = 1024 // 优化JSON缓冲区大小
        };

        ValidateOptions();
    }

    #region 属性

    public WebSocketConnectionState State => _state;
    public string Url => _options.Url;
    public bool IsConnected => _state == WebSocketConnectionState.Connected && _webSocket?.State == System.Net.WebSockets.WebSocketState.Open;
    public string? SubProtocol => _webSocket?.SubProtocol;
    public int ReconnectAttempts => _reconnectAttempts;
    public DateTime? LastHeartbeat => _statistics.LastHeartbeatAt;
    public WebSocketOptions Options => _options;

    #endregion

    #region 事件

    public event EventHandler<WebSocketConnectedEventArgs>? Connected;
    public event EventHandler<WebSocketDisconnectedEventArgs>? Disconnected;
    public event EventHandler<WebSocketMessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<WebSocketErrorEventArgs>? Error;
    public event EventHandler<WebSocketReconnectingEventArgs>? Reconnecting;

    #endregion

    #region 连接管理

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                _logger?.LogDebug("WebSocket is already connected");
                return;
            }

            _logger?.LogInformation("Connecting to WebSocket: {Url}", _options.Url);
            
            SetState(WebSocketConnectionState.Connecting);
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, 
                _cancellationTokenSource.Token
            ).Token;

            await ConnectInternalAsync(combinedToken);
            
            _reconnectAttempts = 0;
            _statistics.ConnectedAt = DateTime.UtcNow;
            
            SetState(WebSocketConnectionState.Connected);
            
            // 启动心跳
            if (_options.HeartbeatIntervalMs > 0)
            {
                _ = Task.Run(() => StartHeartbeatAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            
            OnConnected(new WebSocketConnectedEventArgs 
            { 
                Url = _options.Url, 
                SubProtocol = SubProtocol 
            });
            
            _logger?.LogInformation("WebSocket connected successfully");
        }
        catch (Exception ex)
        {
            SetState(WebSocketConnectionState.Failed);
            _logger?.LogError(ex, "Failed to connect to WebSocket");
            OnError(new WebSocketErrorEventArgs { Exception = ex, Context = "Connect" });
            throw;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task DisconnectAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string? statusDescription = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            if (_state == WebSocketConnectionState.Disconnected)
            {
                return;
            }

            _logger?.LogInformation("Disconnecting WebSocket");
            
            SetState(WebSocketConnectionState.Disconnecting);
            
            // 取消所有操作
            _cancellationTokenSource?.Cancel();
            _receiveCancellationTokenSource?.Cancel();
            _heartbeatCancellationTokenSource?.Cancel();
            
            StopReceiving();

            if (_webSocket?.State == System.Net.WebSockets.WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error during WebSocket close");
                }
            }

            await CleanupResourcesAsync();
            
            SetState(WebSocketConnectionState.Disconnected);
            
            OnDisconnected(new WebSocketDisconnectedEventArgs 
            { 
                CloseStatus = closeStatus,
                Reason = statusDescription,
                IsExpected = true,
                WillReconnect = false
            });
            
            _logger?.LogInformation("WebSocket disconnected");
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task ReconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        _logger?.LogInformation("Reconnecting WebSocket");
        
        await DisconnectAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", cancellationToken);
        await ConnectAsync(cancellationToken);
    }

    #endregion

    #region 高性能消息发送

    public async Task SendTextAsync(string message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var bytes = Encoding.UTF8.GetBytes(message);
        await SendMessageInternalAsync(bytes, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task SendBinaryAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await SendMessageInternalAsync(data, System.Net.WebSockets.WebSocketMessageType.Binary, true, cancellationToken);
    }

    public async Task SendJsonAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        // 使用ArrayWriter避免额外的内存分配
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false });
        JsonSerializer.Serialize(writer, obj, _jsonOptions);
        writer.Flush();
        
        var data = bufferWriter.WrittenSpan.ToArray();
        await SendMessageInternalAsync(data, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task SendMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        var data = message.GetBytes();
        var messageType = message.MessageType == WebSocketMessageType.Text 
            ? System.Net.WebSockets.WebSocketMessageType.Text 
            : System.Net.WebSockets.WebSocketMessageType.Binary;
            
        await SendMessageInternalAsync(data, messageType, message.IsEndOfMessage, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task SendMessageInternalAsync(byte[] data, System.Net.WebSockets.WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await _webSocket!.SendAsync(
                new ArraySegment<byte>(data),
                messageType,
                endOfMessage,
                cancellationToken
            );

            // 更新统计信息
            _statistics.MessagesSent++;
            _statistics.BytesSent += data.Length;
            
            _logger?.LogDebug("Sent WebSocket message: {Type}, Length: {Length}", messageType, data.Length);
        }
        catch (Exception ex)
        {
            _statistics.ErrorCount++;
            _statistics.LastErrorAt = DateTime.UtcNow;
            _statistics.LastError = ex.Message;
            
            _logger?.LogError(ex, "Failed to send WebSocket message");
            OnError(new WebSocketErrorEventArgs { Exception = ex, Context = "Send" });
            
            // 如果是连接错误，触发重连
            if (ex is WebSocketException && _options.EnableAutoReconnect)
            {
                _ = Task.Run(() => HandleReconnectAsync());
            }
            
            throw;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task SendHeartbeatAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected || string.IsNullOrEmpty(_options.HeartbeatMessage))
        {
            return;
        }

        try
        {
            await SendTextAsync(_options.HeartbeatMessage, cancellationToken);
            _statistics.HeartbeatCount++;
            _statistics.LastHeartbeatAt = DateTime.UtcNow;
            
            _logger?.LogDebug("Heartbeat sent");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to send heartbeat");
        }
    }

    #endregion

    #region 高性能消息接收

    public async Task StartReceivingAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_isReceiving)
        {
            return;
        }

        _isReceiving = true;
        _receiveCancellationTokenSource?.Cancel();
        _receiveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            // 启动高性能接收循环
            await HighPerformanceReceiveLoopAsync(_receiveCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogDebug("Receive loop cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in receive loop");
            OnError(new WebSocketErrorEventArgs { Exception = ex, Context = "Receive" });
            
            if (_options.EnableAutoReconnect && !_disposed)
            {
                _ = Task.Run(() => HandleReconnectAsync());
            }
        }
        finally
        {
            _isReceiving = false;
        }
    }

    public void StopReceiving()
    {
        _receiveCancellationTokenSource?.Cancel();
        _isReceiving = false;
    }

    /// <summary>
    /// 高性能消息接收循环
    /// </summary>
    private async Task HighPerformanceReceiveLoopAsync(CancellationToken cancellationToken)
    {
        // 使用内存池减少分配
        var bufferOwner = _memoryPool.Rent(_options.ReceiveBufferSize);
        var buffer = bufferOwner.Memory;
        
        try
        {
            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 使用可重用的内存缓冲区
                    var messageData = await ReceiveCompleteMessageAsync(buffer, cancellationToken);
                    if (messageData == null) break;

                    var (data, messageType) = messageData.Value;
                    
                    // 快速创建消息对象
                    var message = CreateMessageFast(data, messageType);
                    
                    // 更新统计信息
                    _statistics.MessagesReceived++;
                    _statistics.BytesReceived += data.Length;
                    Interlocked.Increment(ref _totalMessagesProcessed);
                    Interlocked.Add(ref _totalBytesProcessed, data.Length);

                    // 检查是否为心跳响应
                    if (IsHeartbeatResponse(message))
                    {
                        _logger?.LogDebug("Received heartbeat response");
                        continue;
                    }

                    // 高性能消息入队
                    if (!await _messageWriter.WaitToWriteAsync(cancellationToken))
                    {
                        break; // Channel已关闭
                    }

                    if (_messageWriter.TryWrite(message))
                    {
                        _hasPendingMessages = true;
                    }
                    else
                    {
                        // Channel满了，记录警告但不阻塞
                        _logger?.LogWarning("Message channel is full, message dropped");
                    }
                    
                    _logger?.LogDebug("Received WebSocket message: {Type}, Length: {Length}", messageType, data.Length);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    _logger?.LogWarning("WebSocket connection closed prematurely");
                    await HandleUnexpectedDisconnectAsync("Connection closed prematurely");
                    break;
                }
                catch (Exception ex)
                {
                    _statistics.ErrorCount++;
                    _statistics.LastErrorAt = DateTime.UtcNow;
                    _statistics.LastError = ex.Message;
                    
                    _logger?.LogError(ex, "Error receiving WebSocket message");
                    OnError(new WebSocketErrorEventArgs { Exception = ex, Context = "Receive" });
                    
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(100, cancellationToken); // 更短的延迟
                    }
                }
            }
        }
        finally
        {
            bufferOwner.Dispose();
        }
    }

    /// <summary>
    /// 接收完整消息（零拷贝优化）
    /// </summary>
    private async Task<(byte[] data, System.Net.WebSockets.WebSocketMessageType messageType)?> ReceiveCompleteMessageAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var segments = new List<ArraySegment<byte>>();
        ValueWebSocketReceiveResult result;
        System.Net.WebSockets.WebSocketMessageType? messageType = null;
        int totalLength = 0;

        do
        {
            result = await _webSocket!.ReceiveAsync(buffer, cancellationToken);
            
            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
            {
                _logger?.LogInformation("Received close message from server");
                await HandleServerCloseAsync(null, null);
                return null;
            }

            messageType ??= result.MessageType;
            
            // 租用数组存储片段
            var segmentArray = _arrayPool.Rent(result.Count);
            buffer.Span.Slice(0, result.Count).CopyTo(segmentArray);
            segments.Add(new ArraySegment<byte>(segmentArray, 0, result.Count));
            totalLength += result.Count;
            
        } while (!result.EndOfMessage);

        // 组装完整消息
        var completeMessage = new byte[totalLength];
        int offset = 0;
        
        foreach (var segment in segments)
        {
            Array.Copy(segment.Array!, segment.Offset, completeMessage, offset, segment.Count);
            offset += segment.Count;
            
            // 归还租用的数组
            _arrayPool.Return(segment.Array!);
        }

        return (completeMessage, messageType!.Value);
    }

    /// <summary>
    /// 快速创建消息对象
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static WebSocketMessage CreateMessageFast(byte[] data, System.Net.WebSockets.WebSocketMessageType messageType)
    {
        return messageType == System.Net.WebSockets.WebSocketMessageType.Text
            ? WebSocketMessage.CreateText(Encoding.UTF8.GetString(data))
            : WebSocketMessage.CreateBinary(data);
    }

    /// <summary>
    /// 批处理消息刷新
    /// </summary>
    private void FlushBatchMessages(object? state)
    {
        if (!_hasPendingMessages) return;

        try
        {
            _batchBuffer.Clear();
            
            // 批量读取消息
            while (_messageReader.TryRead(out var message) && _batchBuffer.Count < 100)
            {
                _batchBuffer.Add(message);
            }

            if (_batchBuffer.Count == 0)
            {
                _hasPendingMessages = false;
                return;
            }

            // 批量触发事件
            foreach (var message in _batchBuffer)
            {
                try
                {
                    OnMessageReceived(new WebSocketMessageReceivedEventArgs { Message = message });
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error in message received event handler");
                }
            }

            _hasPendingMessages = _messageReader.TryPeek(out _);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in batch message flush");
        }
    }

    #endregion

    #region 工具方法

    public async Task WaitForConnectionAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var timeoutMs = (int)(timeout?.TotalMilliseconds ?? _options.ConnectTimeoutMs);
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        while (!IsConnected && !combinedCts.Token.IsCancellationRequested)
        {
            await Task.Delay(10, combinedCts.Token); // 更短的检查间隔
        }

        if (!IsConnected)
        {
            throw new TimeoutException($"Failed to establish connection within {timeoutMs}ms");
        }
    }

    public bool CheckConnection()
    {
        return IsConnected && _webSocket?.State == System.Net.WebSockets.WebSocketState.Open;
    }

    public WebSocketStatistics GetStatistics()
    {
        // 返回增强的统计信息
        var stats = _statistics;
        stats.TotalMessagesProcessed = _totalMessagesProcessed;
        stats.TotalBytesProcessed = _totalBytesProcessed;
        return stats;
    }

    /// <summary>
    /// 获取实时性能指标
    /// </summary>
    public PerformanceMetrics GetPerformanceMetrics()
    {
        var uptime = DateTime.UtcNow - (_statistics.ConnectedAt ?? DateTime.UtcNow);
        var messagesPerSecond = uptime.TotalSeconds > 0 ? _totalMessagesProcessed / uptime.TotalSeconds : 0;
        var bytesPerSecond = uptime.TotalSeconds > 0 ? _totalBytesProcessed / uptime.TotalSeconds : 0;
        
        return new PerformanceMetrics
        {
            MessagesPerSecond = messagesPerSecond,
            BytesPerSecond = bytesPerSecond,
            TotalMessages = _totalMessagesProcessed,
            TotalBytes = _totalBytesProcessed,
            ErrorRate = _statistics.ErrorCount / (double)Math.Max(_totalMessagesProcessed, 1),
            Uptime = uptime,
            ChannelUtilization = GetChannelUtilization()
        };
    }

    private double GetChannelUtilization()
    {
        // 估算Channel使用率
        var maxCapacity = _options.MaxMessageQueueLength > 0 ? _options.MaxMessageQueueLength : 10000;
        var currentCount = _messageReader.CanCount ? _messageReader.Count : 0;
        return currentCount / (double)maxCapacity;
    }

    #endregion

    #region 私有方法

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Url))
        {
            throw new ArgumentException("WebSocket URL cannot be null or empty", nameof(_options.Url));
        }

        if (!Uri.TryCreate(_options.Url, UriKind.Absolute, out var uri) || 
            (uri.Scheme != "ws" && uri.Scheme != "wss"))
        {
            throw new ArgumentException("Invalid WebSocket URL format", nameof(_options.Url));
        }
    }

    private async Task ConnectInternalAsync(CancellationToken cancellationToken)
    {
        _webSocket?.Dispose();
        _webSocket = new ClientWebSocket();

        // 优化WebSocket配置
        _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        
        // 配置WebSocket选项
        foreach (var header in _options.Headers)
        {
            _webSocket.Options.SetRequestHeader(header.Key, header.Value);
        }

        foreach (var protocol in _options.SubProtocols)
        {
            _webSocket.Options.AddSubProtocol(protocol);
        }

        _webSocket.Options.SetBuffer(_options.ReceiveBufferSize, _options.SendBufferSize);
        
        if (!string.IsNullOrEmpty(_options.UserAgent))
        {
            _webSocket.Options.SetRequestHeader("User-Agent", _options.UserAgent);
        }

        // SSL配置
        if (_options.IgnoreSslErrors)
        {
            _webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }

        using var timeoutCts = new CancellationTokenSource(_options.ConnectTimeoutMs);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        await _webSocket.ConnectAsync(new Uri(_options.Url), combinedCts.Token);
    }

    private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
    {
        _heartbeatCancellationTokenSource?.Cancel();
        _heartbeatCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        while (!_heartbeatCancellationTokenSource.Token.IsCancellationRequested && IsConnected)
        {
            try
            {
                await Task.Delay(_options.HeartbeatIntervalMs, _heartbeatCancellationTokenSource.Token);
                
                if (IsConnected)
                {
                    await SendHeartbeatAsync(_heartbeatCancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Heartbeat error");
            }
        }
    }

    private bool IsHeartbeatResponse(WebSocketMessage message)
    {
        return message.MessageType == WebSocketMessageType.Text &&
               !string.IsNullOrEmpty(_options.HeartbeatResponse) &&
               message.Text?.Trim().Equals(_options.HeartbeatResponse, StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task HandleServerCloseAsync(WebSocketCloseStatus? closeStatus, string? closeStatusDescription)
    {
        SetState(WebSocketConnectionState.Disconnected);
        
        OnDisconnected(new WebSocketDisconnectedEventArgs
        {
            CloseStatus = closeStatus,
            Reason = closeStatusDescription,
            IsExpected = true,
            WillReconnect = _options.EnableAutoReconnect
        });

        if (_options.EnableAutoReconnect)
        {
            await HandleReconnectAsync();
        }
    }

    private async Task HandleUnexpectedDisconnectAsync(string reason)
    {
        SetState(WebSocketConnectionState.Disconnected);
        
        OnDisconnected(new WebSocketDisconnectedEventArgs
        {
            Reason = reason,
            IsExpected = false,
            WillReconnect = _options.EnableAutoReconnect
        });

        if (_options.EnableAutoReconnect)
        {
            await HandleReconnectAsync();
        }
    }

    private async Task HandleReconnectAsync()
    {
        if (_disposed || !_options.EnableAutoReconnect)
        {
            return;
        }

        if (_options.MaxReconnectAttempts > 0 && _reconnectAttempts >= _options.MaxReconnectAttempts)
        {
            _logger?.LogWarning("Maximum reconnect attempts ({MaxAttempts}) reached", _options.MaxReconnectAttempts);
            return;
        }

        await _connectLock.WaitAsync();
        try
        {
            if (IsConnected || _state == WebSocketConnectionState.Connecting)
            {
                return;
            }

            _reconnectAttempts++;
            _statistics.ReconnectCount++;
            
            SetState(WebSocketConnectionState.Reconnecting);
            
            var reconnectEventArgs = new WebSocketReconnectingEventArgs
            {
                AttemptCount = _reconnectAttempts,
                DelayMs = _options.ReconnectIntervalMs
            };
            
            OnReconnecting(reconnectEventArgs);
            
            if (reconnectEventArgs.Cancel)
            {
                _logger?.LogInformation("Reconnect cancelled by event handler");
                return;
            }

            _logger?.LogInformation("Attempting to reconnect ({Attempt}/{MaxAttempts})", 
                _reconnectAttempts, 
                _options.MaxReconnectAttempts > 0 ? _options.MaxReconnectAttempts.ToString() : "∞");

            await Task.Delay(reconnectEventArgs.DelayMs);
            
            try
            {
                await ConnectAsync();
                
                // 重新开始接收消息
                if (_isReceiving)
                {
                    _ = Task.Run(() => StartReceivingAsync());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Reconnect attempt {Attempt} failed", _reconnectAttempts);
                
                // 递归重试
                _ = Task.Run(() => HandleReconnectAsync());
            }
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task CleanupResourcesAsync()
    {
        StopReceiving();
        
        _cancellationTokenSource?.Cancel();
        _receiveCancellationTokenSource?.Cancel();
        _heartbeatCancellationTokenSource?.Cancel();

        if (_webSocket != null)
        {
            if (_webSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cleanup", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Error during WebSocket cleanup close");
                }
            }
            
            _webSocket.Dispose();
            _webSocket = null;
        }

        // 清理Channel
        _messageWriter.Complete();
    }

    private void SetState(WebSocketConnectionState newState)
    {
        if (_state != newState)
        {
            var oldState = _state;
            _state = newState;
            _logger?.LogDebug("WebSocket state changed: {OldState} -> {NewState}", oldState, newState);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HighPerformanceWebSocketClient));
        }
    }

    #endregion

    #region 事件触发

    private void OnConnected(WebSocketConnectedEventArgs args)
    {
        try
        {
            Connected?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Connected event handler");
        }
    }

    private void OnDisconnected(WebSocketDisconnectedEventArgs args)
    {
        try
        {
            Disconnected?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Disconnected event handler");
        }
    }

    private void OnMessageReceived(WebSocketMessageReceivedEventArgs args)
    {
        try
        {
            MessageReceived?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in MessageReceived event handler");
        }
    }

    private void OnError(WebSocketErrorEventArgs args)
    {
        try
        {
            Error?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Error event handler");
        }
    }

    private void OnReconnecting(WebSocketReconnectingEventArgs args)
    {
        try
        {
            Reconnecting?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Reconnecting event handler");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            DisconnectAsync().Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error during dispose");
        }

        _batchFlushTimer?.Dispose();
        _cancellationTokenSource?.Dispose();
        _receiveCancellationTokenSource?.Dispose();
        _heartbeatCancellationTokenSource?.Dispose();
        _sendLock?.Dispose();
        _connectLock?.Dispose();
        _webSocket?.Dispose();
        _memoryPool?.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// 性能指标
/// </summary>
public class PerformanceMetrics
{
    public double MessagesPerSecond { get; set; }
    public double BytesPerSecond { get; set; }
    public long TotalMessages { get; set; }
    public long TotalBytes { get; set; }
    public double ErrorRate { get; set; }
    public TimeSpan Uptime { get; set; }
    public double ChannelUtilization { get; set; }
}
