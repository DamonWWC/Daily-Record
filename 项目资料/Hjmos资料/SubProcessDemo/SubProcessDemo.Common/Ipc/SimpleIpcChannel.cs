using System.IO.Pipes;
using System.Text;

namespace SubProcessDemo.Common.Ipc;

/// <summary>
/// 简化版命名管道通信通道。
/// 协议：4 字节长度头（big-endian int32）+ UTF-8 JSON body。
/// </summary>
public class SimpleIpcChannel : IAsyncDisposable
{
    private readonly PipeStream _stream;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _readLoop;
    private readonly SemaphoreSlim _writeLock = new(1, 1);  // 写锁：防止并发写导致字节交错

    public event Action<IpcMessage>? MessageReceived;
    public event Action<Exception>? ErrorOccurred;

    // ── 构造函数 ──

    private SimpleIpcChannel(PipeStream stream)
    {
        _stream = stream;
        _readLoop = Task.Run(ReadLoopAsync);
    }

    // ── 工厂方法 ──

    /// <summary>创建 Server 端通道（等待客户端连接后返回）</summary>
    public static async Task<SimpleIpcChannel> CreateServerAsync(string pipeName, CancellationToken ct = default)
    {
        var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        await server.WaitForConnectionAsync(ct);
        return new SimpleIpcChannel(server);
    }

    /// <summary>创建 Client 端通道</summary>
    public static async Task<SimpleIpcChannel> CreateClientAsync(string pipeName, CancellationToken ct = default)
    {
        var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(5000, ct);
        return new SimpleIpcChannel(client);
    }

    // ── 发送（加写锁，防止并发写破坏协议格式） ──

    public async Task SendAsync(IpcMessage message)
    {
        var json = message.ToJson();
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes); // → big-endian

        // 合并为单次写入，避免长度头与 body 被其他写操作插入
        var packet = new byte[4 + bytes.Length];
        lengthBytes.CopyTo(packet, 0);
        bytes.CopyTo(packet, 4);

        await _writeLock.WaitAsync();
        try
        {
            await _stream.WriteAsync(packet);
            await _stream.FlushAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    // ── 读取循环 ──

    private async Task ReadLoopAsync()
    {
        var lengthBuf = new byte[4];
        try
        {
            while (!_cts.IsCancellationRequested && _stream.IsConnected)
            {
                // 读 4 字节长度头
                if (!await ReadExactAsync(lengthBuf, 4)) break;
                if (BitConverter.IsLittleEndian) Array.Reverse(lengthBuf);
                var length = BitConverter.ToInt32(lengthBuf);

                if (length <= 0 || length > 10 * 1024 * 1024) break; // 安全上限 10MB

                // 读 body
                var body = new byte[length];
                if (!await ReadExactAsync(body, length)) break;

                var json = Encoding.UTF8.GetString(body);
                var msg = IpcMessage.FromJson(json);
                if (msg != null)
                {
                    // 在独立线程上触发事件，防止慢速处理器阻塞读取循环
                    var captured = msg;
                    ThreadPool.QueueUserWorkItem(_ => MessageReceived?.Invoke(captured));
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorOccurred?.Invoke(ex);
        }
    }

    private async Task<bool> ReadExactAsync(byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await _stream.ReadAsync(buffer.AsMemory(offset, count - offset));
            if (read == 0) return false; // 管道关闭
            offset += read;
        }
        return true;
    }

    // ── 释放 ──

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try { await _readLoop; } catch { /* 忽略 */ }
        _cts.Dispose();
        _writeLock.Dispose();
        await _stream.DisposeAsync();
    }
}
