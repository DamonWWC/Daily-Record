using MessagePack;
using Microsoft.Extensions.Options;
using Polly;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Riley.Core.WebSocket
{
    public class RileyWebSocketClient:IDisposable
    {
        private ClientWebSocket _webSocket;
        /// <summary>
        /// 最后的心跳时间
        /// </summary>
        private DateTime heartBeatTime;
        private readonly WebSocketOptions _options;
        /// <summary>
        /// 任务Token
        /// </summary>
        private CancellationTokenSource cts = new CancellationTokenSource();
        /// <summary>
        /// 消息发送
        /// </summary>
        public Channel<WebSocketMessage> SendChannel { get; private set; }
        /// <summary>
        /// 消息接收
        /// </summary>
        public Channel<WebSocketMessage> ReceiveChannel { get; private set; }
        /// <summary>
        /// 连接Url
        /// </summary>
        private string url = string.Empty;
        public RileyWebSocketClient(IOptions<WebSocketOptions>  options)
        {
            _options=options?.Value??throw new ArgumentNullException(nameof(options));
            var channelOptions = new BoundedChannelOptions(_options.MaxMessageQueueLength > 0 ? _options.MaxMessageQueueLength : 10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest, // 丢弃最旧的消息
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false // 避免同步延续
            };
            SendChannel = Channel.CreateUnbounded<WebSocketMessage>();
            ReceiveChannel=Channel.CreateUnbounded<WebSocketMessage>();

        }

        public async Task ConnectAsync(string url)
        {
            this.url = url;
            
                  // 配置重试策略
            var retry = new ResiliencePipelineBuilder()
                .AddRetry(new Polly.Retry.RetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Linear,
                    MaxRetryAttempts = int.MaxValue,
                    MaxDelay = TimeSpan.FromSeconds(30),
                    Delay = TimeSpan.FromSeconds(1),
                }).Build();
            await retry.ExecuteAsync(async token => await StartConnectAsync(token));
            heartBeatTime = DateTime.Now;
            StartSendMessage();
            StartReciveMessage();
            StartHeartBeat();
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        private void Disconnect()
        {
            try
            {
                if (_webSocket?.State == WebSocketState.Aborted)
                {
                    _webSocket = new ClientWebSocket();
                    return;
                }
                _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", cts.Token).Wait();
            }
            catch (Exception ex)
            {
                
            }
        }
        private async Task Reconnect()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            Disconnect();
            await ConnectAsync(url);
        }
        private async Task StartConnectAsync(CancellationToken token)
        {
            try
            {
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(_options.Url + url), token);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private void StartSendMessage()
        {
            var _ = Task.Factory.StartNew(async () =>
            {
                await foreach (var message in SendChannel.Reader.ReadAllAsync(cts.Token))
                {
                    if(message != null)
                    {
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
                                cts.Token
                            );
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        private void StartReciveMessage()
        {
            var _ = Task.Factory.StartNew(async () =>
            {
                var pipe = new Pipe();
                while (_webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var reciveBytes = ReciveAllBytes(_webSocket, pipe.Writer);
                        var readBytes = ReadAllReciveBytes(pipe.Reader);
                        await Task.WhenAll(reciveBytes, readBytes);
                        pipe.Reset();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            },TaskCreationOptions.LongRunning);
        }

        private async Task ReciveAllBytes(ClientWebSocket webSocket,PipeWriter writer)
        {
            const int bufferSize = 4096;
            while (!cts.Token.IsCancellationRequested)
            {
                var memory = writer.GetMemory(bufferSize);
                try
                {
                    var recived = await _webSocket.ReceiveAsync(memory, cts.Token);
                    if(recived.MessageType==System.Net.WebSockets.WebSocketMessageType.Text)
                    {
                        writer.Advance(recived.Count);

                        if(recived.EndOfMessage)
                        {
                            break;
                        }
                    }
                    if(recived.MessageType==System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
                        break;
                    }
                }
                catch(TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    return;
                }
                var result = await writer.FlushAsync();
                if(result.IsCompleted)
                {
                    break;
                }
            }
            await writer.CompleteAsync();
        }
        private SerializerType serializerType = SerializerType.Json;
        private async Task ReadAllReciveBytes(PipeReader reader)
        {
            try
            {
                var msg = "";
                if(serializerType==SerializerType.Json)
                {
                    while(!cts.Token.IsCancellationRequested)
                    {
                        var result = await reader.ReadAsync();
                        msg += Encoding.UTF8.GetString(result.Buffer).Trim('\0');
                        reader.AdvanceTo(result.Buffer.End);

                        if(result.IsCompleted)
                        {
                            break;
                        }
                    }
                }
                else if(serializerType==SerializerType.MessagePack)
                {
                    var buffers = new List<ReadOnlySequence<byte>>();
                    var totalLength = 0L;
                    while(!cts.Token.IsCancellationRequested)
                    {
                        var result=await reader.ReadAsync();
                        buffers.Add(result.Buffer);
                        totalLength += result.Buffer.Length;
                        reader.AdvanceTo(result.Buffer.End);
                        if (result.IsCompleted)
                            break;
                    }
                    var bytes = new byte[totalLength];
                    var offset = 0;
                    foreach(var buffer in buffers)
                    {
                        foreach(var segment in buffer)
                        {
                            segment.CopyTo(bytes.AsMemory(offset));
                            offset += segment.Length;
                        }
                    }
                    msg = MessagePackSerializer.ConvertToJson(bytes);
                }
                await reader.CompleteAsync();
                msg.Trim('\0');
                await HandleReciveMsg(msg);


            }
            catch(Exception ex)
            {
                return;
            }
        }
        private async Task HandleReciveMsg(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg)) return;
                var message=JsonSerializer.Deserialize<WebSocketMessage>(msg);
                if (message != null)
                {
                    if (IsHeartbeatResponse(message))
                        heartBeatTime = DateTime.Now;
                    else
                    {
                        await ReceiveChannel.Writer.WriteAsync(message);
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
        private void StartHeartBeat()
        {
            // 心跳判断
            var _= Task.Factory.StartNew(async () =>
            {
                // 捕获当前的Token,避免读取到新Token
                var token = cts.Token;
                while (!token.IsCancellationRequested)
                {
                    // 5秒未读到心跳则重连
                    if (DateTime.Now - heartBeatTime > TimeSpan.FromSeconds(5))
                    {
                        await Reconnect();
                        break;
                    }
                    await Task.Delay(1000, token);
                }
            }, TaskCreationOptions.LongRunning);
        }
        private bool IsHeartbeatResponse(WebSocketMessage message)
        {
            return message.MessageType == WebSocketMessageType.Text &&
                   !string.IsNullOrEmpty(_options.HeartbeatResponse) &&
                   message.Text?.Trim().Equals(_options.HeartbeatResponse, StringComparison.OrdinalIgnoreCase) == true;
        }
        public void Dispose()
        {
            try
            {
                cts?.Cancel();
                Disconnect();
                SendChannel?.Writer.Complete();
                ReceiveChannel?.Writer.Complete();
                _webSocket?.Dispose();
            }
            catch (Exception ex)
            {
                
            }
        }

    }
    public enum SerializerType
    {
        Json,
        MessagePack
    }
}
