using System.IO.Pipes;
using System.Text;

namespace SubProcessDemo.Common.Ipc;

/// <summary>
/// 简化版命名管道通信通道（支持自动重连）。
/// 协议：4 字节长度头（big-endian int32）+ UTF-8 JSON body。
/// </summary>
public class SimpleIpcChannel : IAsyncDisposable
{
    private PipeStream _stream;
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    // 重连相关
    private readonly string? _pipeName;
    private readonly bool _isClient;
    private Task? _readLoop;
    private volatile bool _isReconnecting;

    /// <summary>是否启用自动重连（仅 Client 端有效）</summary>
    public bool AutoReconnect { get; set; }

    /// <summary>重连间隔基数（毫秒），实际间隔按指数退避递增</summary>
    public int ReconnectBaseDelayMs { get; set; } = 500;

    /// <summary>最大重连间隔（毫秒）</summary>
    public int ReconnectMaxDelayMs { get; set; } = 10000;

    public event Action<IpcMessage>? MessageReceived;
    public event Action<Exception>? ErrorOccurred;
    public event Action? Disconnected;
    public event Action? Reconnected;

    // ── 构造函数 ──

    private SimpleIpcChannel(PipeStream stream, string? pipeName, bool isClient)
    {
        _stream = stream;
        _pipeName = pipeName;
        _isClient = isClient;
        _readLoop = Task.Run(ReadLoopAsync);
    }

    // ── 工厂方法 ──

    /// <summary>创建 Server 端通道（等待客户端连接后返回）</summary>
    public static async Task<SimpleIpcChannel> CreateServerAsync(string pipeName, CancellationToken ct = default)
    {
        var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        await server.WaitForConnectionAsync(ct);
        return new SimpleIpcChannel(server, pipeName, isClient: false);
    }

    /// <summary>创建 Client 端通道</summary>
    public static async Task<SimpleIpcChannel> CreateClientAsync(string pipeName, CancellationToken ct = default)
    {
        var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(5000, ct);
        return new SimpleIpcChannel(client, pipeName, isClient: true);
    }

    // ── 发送（加写锁，防止并发写破坏协议格式） ──

    public async Task SendAsync(IpcMessage message)
    {
        var json = message.ToJson();
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);

        var packet = new byte[4 + bytes.Length];
        lengthBytes.CopyTo(packet, 0);
        bytes.CopyTo(packet, 4);

        await _writeLock.WaitAsync();
        try
        {
            if (!_stream.IsConnected)
                throw new IOException("管道未连接");
            await _stream.WriteAsync(packet);
            await _stream.FlushAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>当前是否处于已连接状态</summary>
    public bool IsConnected => _stream.IsConnected && !_isReconnecting;

    /// <summary>
    /// 强制断开管道（用于测试/模拟断连）。
    /// 关闭底层流但不取消 CTS，使 ReadLoopAsync 自然退出并触发 Disconnected + AutoReconnect。
    /// </summary>
    public async Task BreakConnectionAsync()
    {
        await _writeLock.WaitAsync();
        try
        {
            try { _stream.Dispose(); } catch { }
        }
        finally
        {
            _writeLock.Release();
        }
        // ReadLoopAsync 检测到流关闭后会自然退出 → 触发 Disconnected → 触发 ReconnectLoopAsync
    }

    // ── 读取循环 ──

    private async Task ReadLoopAsync()
    {
        var lengthBuf = new byte[4];
        try
        {
            while (!_cts.IsCancellationRequested && _stream.IsConnected)
            {
                if (!await ReadExactAsync(lengthBuf, 4)) break;
                if (BitConverter.IsLittleEndian) Array.Reverse(lengthBuf);
                var length = BitConverter.ToInt32(lengthBuf);

                if (length <= 0 || length > 10 * 1024 * 1024) break;

                var body = new byte[length];
                if (!await ReadExactAsync(body, length)) break;

                var json = Encoding.UTF8.GetString(body);
                var msg = IpcMessage.FromJson(json);
                if (msg != null)
                {
                    var captured = msg;
                    ThreadPool.QueueUserWorkItem(_ => MessageReceived?.Invoke(captured));
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorOccurred?.Invoke(ex);
        }

        // 读取循环退出 = 连接断开
        if (!_cts.IsCancellationRequested)
        {
            Disconnected?.Invoke();

            // Client 端自动重连
            if (_isClient && AutoReconnect && _pipeName != null)
            {
                _ = Task.Run(ReconnectLoopAsync);
            }
        }
    }

    private async Task<bool> ReadExactAsync(byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read;
            try
            {
                read = await _stream.ReadAsync(buffer.AsMemory(offset, count - offset));
            }
            catch (IOException) { return false; }
            catch (ObjectDisposedException) { return false; }
            if (read == 0) return false;
            offset += read;
        }
        return true;
    }

    // ── 自动重连（指数退避） ──

    private async Task ReconnectLoopAsync()
    {
        _isReconnecting = true;
        var delay = ReconnectBaseDelayMs;
        var attempt = 0;

        while (!_cts.IsCancellationRequested)
        {
            attempt++;
            try
            {
                // 指数退避等待
                await Task.Delay(delay, _cts.Token);

                // 创建新的管道客户端并尝试连接
                var newClient = new NamedPipeClientStream(".", _pipeName!, PipeDirection.InOut, PipeOptions.Asynchronous);
                await newClient.ConnectAsync(3000, _cts.Token);

                // 连接成功：替换流，重启读取循环
                await _writeLock.WaitAsync();
                try
                {
                    var oldStream = _stream;
                    _stream = newClient;
                    try { await oldStream.DisposeAsync(); } catch { }
                }
                finally
                {
                    _writeLock.Release();
                }

                _isReconnecting = false;
                Reconnected?.Invoke();

                // 重启读取循环
                _readLoop = Task.Run(ReadLoopAsync);
                return;
            }
            catch (OperationCanceledException)
            {
                return; // 被 Dispose 取消
            }
            catch
            {
                // 重连失败，指数退避（上限封顶）
                delay = Math.Min(delay * 2, ReconnectMaxDelayMs);
            }
        }
    }

    // ── 释放 ──

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try { if (_readLoop != null) await _readLoop; } catch { }
        _cts.Dispose();
        _writeLock.Dispose();
        try { await _stream.DisposeAsync(); } catch { }
    }
}
