using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket客户端实现
/// </summary>
public class WebSocketClient : IWebSocketClient
{
    private readonly ILogger<WebSocketClient>? _logger;
    private readonly WebSocketOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WebSocketStatistics _statistics;
    
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private CancellationTokenSource? _heartbeatCancellationTokenSource;
    
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly ConcurrentQueue<WebSocketMessage> _messageQueue = new();
    
    private volatile WebSocketConnectionState _state = WebSocketConnectionState.Disconnected;
    private volatile bool _disposed = false;
    private volatile bool _isReceiving = false;
    private volatile int _reconnectAttempts = 0;

    public WebSocketClient(IOptions<WebSocketOptions> options, ILogger<WebSocketClient>? logger = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _statistics = new WebSocketStatistics();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
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

    #region 消息发送

    public async Task SendTextAsync(string message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await SendMessageAsync(WebSocketMessage.CreateText(message), cancellationToken);
    }

    public async Task SendBinaryAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await SendMessageAsync(WebSocketMessage.CreateBinary(data), cancellationToken);
    }

    public async Task SendJsonAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await SendMessageAsync(WebSocketMessage.CreateJson(obj, _jsonOptions), cancellationToken);
    }

    public async Task SendMessageAsync(WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (!IsConnected)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            var data = message.GetBytes();
            var messageType = message.MessageType switch
            {
                WebSocketMessageType.Text => System.Net.WebSockets.WebSocketMessageType.Text,
                WebSocketMessageType.Binary => System.Net.WebSockets.WebSocketMessageType.Binary,
                _ => System.Net.WebSockets.WebSocketMessageType.Text
            };

            await _webSocket!.SendAsync(
                new ArraySegment<byte>(data),
                messageType,
                message.IsEndOfMessage,
                cancellationToken
            );

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

    #region 消息接收

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
            await ReceiveLoopAsync(_receiveCancellationTokenSource.Token);
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

    #endregion

    #region 工具方法

    public async Task WaitForConnectionAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var timeoutMs = (int)(timeout?.TotalMilliseconds ?? _options.ConnectTimeoutMs);
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        while (!IsConnected && !combinedCts.Token.IsCancellationRequested)
        {
            await Task.Delay(100, combinedCts.Token);
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
        return _statistics;
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

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[_options.ReceiveBufferSize];
        
        while (IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var ms = new MemoryStream();
                System.Net.WebSockets.WebSocketReceiveResult result;
                
                do
                {
                    result = await _webSocket!.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    ms.Write(buffer, 0, result.Count);
                } 
                while (!result.EndOfMessage);

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    _logger?.LogInformation("Received close message from server");
                    await HandleServerCloseAsync(result.CloseStatus, result.CloseStatusDescription);
                    break;
                }

                var messageData = ms.ToArray();
                var message = result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text
                    ? WebSocketMessage.CreateText(Encoding.UTF8.GetString(messageData))
                    : WebSocketMessage.CreateBinary(messageData);

                _statistics.MessagesReceived++;
                _statistics.BytesReceived += messageData.Length;

                // 检查是否为心跳响应
                if (IsHeartbeatResponse(message))
                {
                    _logger?.LogDebug("Received heartbeat response");
                    continue;
                }

                // 处理消息队列
                if (_options.MaxMessageQueueLength > 0 && _messageQueue.Count >= _options.MaxMessageQueueLength)
                {
                    _messageQueue.TryDequeue(out _); // 移除最旧的消息
                }
                
                _messageQueue.Enqueue(message);
                OnMessageReceived(new WebSocketMessageReceivedEventArgs { Message = message });
                
                _logger?.LogDebug("Received WebSocket message: {Type}, Length: {Length}", result.MessageType, messageData.Length);
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
                    await Task.Delay(1000, cancellationToken); // 短暂延迟后继续
                }
            }
        }
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

        if (_options.AutoCleanupOnClose)
        {
            while (_messageQueue.TryDequeue(out _)) { } // 清空消息队列
        }
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
            throw new ObjectDisposedException(nameof(WebSocketClient));
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

        _cancellationTokenSource?.Dispose();
        _receiveCancellationTokenSource?.Dispose();
        _heartbeatCancellationTokenSource?.Dispose();
        _sendLock?.Dispose();
        _connectLock?.Dispose();
        _webSocket?.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion
}
