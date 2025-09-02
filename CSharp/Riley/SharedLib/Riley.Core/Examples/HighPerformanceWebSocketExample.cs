using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Riley.Core.WebSocket;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Riley.Core.Examples;

/// <summary>
/// 高性能WebSocket客户端使用示例
/// 展示针对高吞吐量、大量小消息、低延迟、内存敏感、高并发连接等场景的优化
/// </summary>
public class HighPerformanceWebSocketExample
{
    /// <summary>
    /// 高性能基本使用示例
    /// </summary>
    public static async Task BasicHighPerformanceUsageAsync()
    {
        Console.WriteLine("=== 高性能WebSocket客户端基本使用示例 ===\n");

        // 配置服务
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // 高性能配置
        var options = new WebSocketOptions
        {
            Url = "wss://echo.websocket.org",
            ReceiveBufferSize = 16 * 1024,      // 16KB 大缓冲区
            SendBufferSize = 16 * 1024,         // 16KB 发送缓冲区
            MaxMessageQueueLength = 10000,      // 大队列
            EnableAutoReconnect = true,
            ReconnectIntervalMs = 1000,         // 快速重连
            HeartbeatIntervalMs = 0,            // 禁用心跳提升性能
            ConnectTimeoutMs = 10000
        };

        services.Configure<WebSocketOptions>(opt =>
        {
            opt.Url = options.Url;
            opt.ReceiveBufferSize = options.ReceiveBufferSize;
            opt.SendBufferSize = options.SendBufferSize;
            opt.MaxMessageQueueLength = options.MaxMessageQueueLength;
            opt.EnableAutoReconnect = options.EnableAutoReconnect;
            opt.ReconnectIntervalMs = options.ReconnectIntervalMs;
            opt.HeartbeatIntervalMs = options.HeartbeatIntervalMs;
            opt.ConnectTimeoutMs = options.ConnectTimeoutMs;
        });

        services.AddTransient<IWebSocketClient, HighPerformanceWebSocketClient>();

        var serviceProvider = services.BuildServiceProvider();
        var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

        try
        {
            // 性能监控
            var receivedCount = 0;
            var stopwatch = Stopwatch.StartNew();

            // 订阅事件
            webSocketClient.Connected += (sender, e) =>
            {
                Console.WriteLine($"✅ 高性能连接成功: {e.Url}");
                stopwatch.Restart();
            };

            webSocketClient.MessageReceived += (sender, e) =>
            {
                Interlocked.Increment(ref receivedCount);
                
                // 每1000条消息报告一次性能
                if (receivedCount % 1000 == 0)
                {
                    var elapsed = stopwatch.Elapsed;
                    var rate = receivedCount / elapsed.TotalSeconds;
                    Console.WriteLine($"📊 已接收 {receivedCount} 条消息, 速率: {rate:F0} msg/s");
                }
            };

            webSocketClient.Error += (sender, e) =>
            {
                Console.WriteLine($"⚠️ 错误: {e.Exception.Message}");
            };

            // 连接并开始接收
            await webSocketClient.ConnectAsync();
            _ = Task.Run(() => webSocketClient.StartReceivingAsync());

            Console.WriteLine("开始高性能测试...\n");

            // 发送大量小消息测试
            const int messageCount = 5000;
            var sendTasks = new List<Task>();

            for (int i = 0; i < messageCount; i++)
            {
                var message = $"High performance message #{i}";
                sendTasks.Add(webSocketClient.SendTextAsync(message));

                // 每100条消息暂停一下，避免过载
                if ((i + 1) % 100 == 0)
                {
                    await Task.WhenAll(sendTasks);
                    sendTasks.Clear();
                    await Task.Delay(10); // 10ms暂停
                }
            }

            // 等待剩余消息发送完成
            if (sendTasks.Count > 0)
            {
                await Task.WhenAll(sendTasks);
            }

            Console.WriteLine($"✅ 已发送 {messageCount} 条消息");

            // 等待接收完成
            var timeout = DateTime.UtcNow.AddSeconds(30);
            while (receivedCount < messageCount && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100);
            }

            // 显示最终统计
            var finalElapsed = stopwatch.Elapsed;
            var finalRate = receivedCount / finalElapsed.TotalSeconds;
            var stats = webSocketClient.GetStatistics();

            Console.WriteLine($"\n📈 最终性能统计:");
            Console.WriteLine($"   发送消息: {stats.MessagesSent}");
            Console.WriteLine($"   接收消息: {stats.MessagesReceived}");
            Console.WriteLine($"   总耗时: {finalElapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"   平均速率: {finalRate:F0} msg/s");
            Console.WriteLine($"   发送速率: {stats.MessagesSent / finalElapsed.TotalSeconds:F0} msg/s");
            Console.WriteLine($"   数据传输: {stats.BytesPerSecond:F0} bytes/s");
            Console.WriteLine($"   错误率: {stats.ErrorRate:P2}");

            await webSocketClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 示例执行失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 超高并发测试
    /// </summary>
    public static async Task UltraHighConcurrencyTestAsync()
    {
        Console.WriteLine("=== 超高并发WebSocket测试 ===\n");

        const int concurrentConnections = 10;
        const int messagesPerConnection = 1000;
        
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // 超高性能配置
        var options = new WebSocketOptions
        {
            Url = "wss://echo.websocket.org",
            ReceiveBufferSize = 32 * 1024,      // 32KB 超大缓冲区
            SendBufferSize = 32 * 1024,
            MaxMessageQueueLength = 20000,      // 超大队列
            EnableAutoReconnect = true,
            ReconnectIntervalMs = 500,
            HeartbeatIntervalMs = 0,            // 禁用心跳
            ConnectTimeoutMs = 5000
        };

        services.Configure<WebSocketOptions>(opt =>
        {
            opt.Url = options.Url;
            opt.ReceiveBufferSize = options.ReceiveBufferSize;
            opt.SendBufferSize = options.SendBufferSize;
            opt.MaxMessageQueueLength = options.MaxMessageQueueLength;
            opt.EnableAutoReconnect = options.EnableAutoReconnect;
            opt.ReconnectIntervalMs = options.ReconnectIntervalMs;
            opt.HeartbeatIntervalMs = options.HeartbeatIntervalMs;
            opt.ConnectTimeoutMs = options.ConnectTimeoutMs;
        });

        services.AddTransient<IWebSocketClient, HighPerformanceWebSocketClient>();

        var serviceProvider = services.BuildServiceProvider();
        
        var clients = new List<IWebSocketClient>();
        var totalReceived = 0;
        var totalSent = 0;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 创建并连接多个客户端
            for (int i = 0; i < concurrentConnections; i++)
            {
                var client = serviceProvider.GetRequiredService<IWebSocketClient>();
                var clientId = i;

                client.Connected += (sender, e) =>
                {
                    Console.WriteLine($"✅ 客户端 {clientId} 连接成功");
                };

                client.MessageReceived += (sender, e) =>
                {
                    Interlocked.Increment(ref totalReceived);
                };

                client.Error += (sender, e) =>
                {
                    Console.WriteLine($"⚠️ 客户端 {clientId} 错误: {e.Exception.Message}");
                };

                clients.Add(client);
                await client.ConnectAsync();
                _ = Task.Run(() => client.StartReceivingAsync());
                
                // 短暂延迟避免连接过载
                await Task.Delay(50);
            }

            Console.WriteLine($"✅ {concurrentConnections} 个客户端连接完成\n");

            // 并发发送消息
            var sendTasks = new List<Task>();
            
            foreach (var client in clients)
            {
                sendTasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < messagesPerConnection; i++)
                    {
                        var message = $"Concurrent message #{i}";
                        await client.SendTextAsync(message);
                        Interlocked.Increment(ref totalSent);
                        
                        // 微小延迟避免过载
                        if (i % 100 == 0)
                        {
                            await Task.Delay(1);
                        }
                    }
                }));
            }

            Console.WriteLine("开始并发发送消息...");
            await Task.WhenAll(sendTasks);
            Console.WriteLine($"✅ 所有消息发送完成，总计: {totalSent}");

            // 等待接收完成
            var expectedTotal = concurrentConnections * messagesPerConnection;
            var timeout = DateTime.UtcNow.AddMinutes(2);
            
            while (totalReceived < expectedTotal && DateTime.UtcNow < timeout)
            {
                await Task.Delay(1000);
                Console.WriteLine($"📊 进度: {totalReceived}/{expectedTotal} ({totalReceived * 100.0 / expectedTotal:F1}%)");
            }

            stopwatch.Stop();

            // 统计结果
            Console.WriteLine($"\n📈 超高并发测试结果:");
            Console.WriteLine($"   并发连接数: {concurrentConnections}");
            Console.WriteLine($"   每连接消息数: {messagesPerConnection}");
            Console.WriteLine($"   总发送消息: {totalSent}");
            Console.WriteLine($"   总接收消息: {totalReceived}");
            Console.WriteLine($"   总耗时: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"   整体吞吐量: {totalReceived / stopwatch.Elapsed.TotalSeconds:F0} msg/s");
            Console.WriteLine($"   平均每连接: {totalReceived / concurrentConnections / stopwatch.Elapsed.TotalSeconds:F0} msg/s");

            // 显示每个客户端的统计
            Console.WriteLine("\n📊 各客户端统计:");
            for (int i = 0; i < clients.Count; i++)
            {
                var stats = clients[i].GetStatistics();
                Console.WriteLine($"   客户端 {i}: 发送={stats.MessagesSent}, 接收={stats.MessagesReceived}, 错误={stats.ErrorCount}");
            }

            // 断开所有连接
            var disconnectTasks = clients.Select(c => c.DisconnectAsync()).ToArray();
            await Task.WhenAll(disconnectTasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 超高并发测试失败: {ex.Message}");
        }
        finally
        {
            // 清理资源
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }
    }

    /// <summary>
    /// 内存效率测试
    /// </summary>
    public static async Task MemoryEfficiencyTestAsync()
    {
        Console.WriteLine("=== 内存效率测试 ===\n");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // 内存优化配置
        var options = new WebSocketOptions
        {
            Url = "wss://echo.websocket.org",
            ReceiveBufferSize = 8192,           // 适中的缓冲区
            MaxMessageQueueLength = 1000,       // 控制队列大小
            EnableAutoReconnect = true,
            HeartbeatIntervalMs = 0
        };

        services.Configure<WebSocketOptions>(opt =>
        {
            opt.Url = options.Url;
            opt.ReceiveBufferSize = options.ReceiveBufferSize;
            opt.MaxMessageQueueLength = options.MaxMessageQueueLength;
            opt.EnableAutoReconnect = options.EnableAutoReconnect;
            opt.HeartbeatIntervalMs = options.HeartbeatIntervalMs;
        });

        services.AddTransient<IWebSocketClient, HighPerformanceWebSocketClient>();

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IWebSocketClient>();

        // 内存监控
        var initialMemory = GC.GetTotalMemory(true);
        Console.WriteLine($"🔍 初始内存使用: {initialMemory / 1024 / 1024:F2} MB");

        try
        {
            var receivedCount = 0;
            var stopwatch = Stopwatch.StartNew();

            client.Connected += (sender, e) =>
            {
                Console.WriteLine("✅ 连接成功，开始内存效率测试");
            };

            client.MessageReceived += (sender, e) =>
            {
                Interlocked.Increment(ref receivedCount);
                
                if (receivedCount % 2000 == 0)
                {
                    var currentMemory = GC.GetTotalMemory(false);
                    var memoryIncrease = currentMemory - initialMemory;
                    Console.WriteLine($"📊 已处理 {receivedCount} 条消息, 内存增长: {memoryIncrease / 1024 / 1024:F2} MB");
                }
            };

            await client.ConnectAsync();
            _ = Task.Run(() => client.StartReceivingAsync());

            // 发送大量消息测试内存使用
            const int totalMessages = 10000;
            Console.WriteLine($"开始发送 {totalMessages} 条消息进行内存测试...");

            for (int i = 0; i < totalMessages; i++)
            {
                var message = new
                {
                    id = i,
                    timestamp = DateTime.UtcNow,
                    data = $"Memory efficiency test message #{i}",
                    payload = new byte[100] // 100字节负载
                };

                await client.SendJsonAsync(message);

                // 定期强制GC以测试内存释放
                if (i % 1000 == 0)
                {
                    GC.Collect();
                    await Task.Delay(10);
                }
            }

            // 等待处理完成
            var timeout = DateTime.UtcNow.AddSeconds(60);
            while (receivedCount < totalMessages && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100);
            }

            stopwatch.Stop();

            // 最终内存检查
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(true);
            var totalMemoryUsed = finalMemory - initialMemory;

            Console.WriteLine($"\n📈 内存效率测试结果:");
            Console.WriteLine($"   处理消息数: {receivedCount}");
            Console.WriteLine($"   总耗时: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"   初始内存: {initialMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"   最终内存: {finalMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"   内存增长: {totalMemoryUsed / 1024 / 1024:F2} MB");
            Console.WriteLine($"   平均每消息内存: {totalMemoryUsed / (double)receivedCount:F0} bytes");

            await client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 内存效率测试失败: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }

    /// <summary>
    /// 延迟测试
    /// </summary>
    public static async Task LatencyTestAsync()
    {
        Console.WriteLine("=== 延迟测试 ===\n");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // 低延迟优化配置
        var options = new WebSocketOptions
        {
            Url = "wss://echo.websocket.org",
            ReceiveBufferSize = 4096,           // 小缓冲区降低延迟
            SendBufferSize = 4096,
            MaxMessageQueueLength = 100,        // 小队列
            EnableAutoReconnect = true,
            HeartbeatIntervalMs = 0
        };

        services.Configure<WebSocketOptions>(opt =>
        {
            opt.Url = options.Url;
            opt.ReceiveBufferSize = options.ReceiveBufferSize;
            opt.SendBufferSize = options.SendBufferSize;
            opt.MaxMessageQueueLength = options.MaxMessageQueueLength;
            opt.EnableAutoReconnect = options.EnableAutoReconnect;
            opt.HeartbeatIntervalMs = options.HeartbeatIntervalMs;
        });

        services.AddTransient<IWebSocketClient, HighPerformanceWebSocketClient>();

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IWebSocketClient>();

        try
        {
            var latencies = new ConcurrentBag<double>();
            var pendingMessages = new ConcurrentDictionary<string, DateTime>();

            client.Connected += (sender, e) =>
            {
                Console.WriteLine("✅ 连接成功，开始延迟测试");
            };

            client.MessageReceived += (sender, e) =>
            {
                try
                {
                    var response = JsonSerializer.Deserialize<LatencyTestMessage>(e.Message.Text ?? "");
                    if (response?.Id != null && pendingMessages.TryRemove(response.Id, out var sendTime))
                    {
                        var latency = (DateTime.UtcNow - sendTime).TotalMilliseconds;
                        latencies.Add(latency);
                    }
                }
                catch
                {
                    // 忽略解析错误
                }
            };

            await client.ConnectAsync();
            _ = Task.Run(() => client.StartReceivingAsync());

            Console.WriteLine("开始延迟测试...");

            // 发送测试消息
            const int testCount = 100;
            for (int i = 0; i < testCount; i++)
            {
                var messageId = Guid.NewGuid().ToString();
                var message = new LatencyTestMessage
                {
                    Id = messageId,
                    Timestamp = DateTime.UtcNow,
                    SequenceNumber = i
                };

                pendingMessages[messageId] = DateTime.UtcNow;
                await client.SendJsonAsync(message);
                
                await Task.Delay(100); // 100ms间隔
            }

            // 等待响应
            var timeout = DateTime.UtcNow.AddSeconds(30);
            while (latencies.Count < testCount && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100);
            }

            // 计算延迟统计
            var latencyArray = latencies.ToArray();
            if (latencyArray.Length > 0)
            {
                Array.Sort(latencyArray);
                var avg = latencyArray.Average();
                var min = latencyArray.Min();
                var max = latencyArray.Max();
                var p50 = latencyArray[latencyArray.Length / 2];
                var p95 = latencyArray[(int)(latencyArray.Length * 0.95)];
                var p99 = latencyArray[(int)(latencyArray.Length * 0.99)];

                Console.WriteLine($"\n📈 延迟测试结果 (样本数: {latencyArray.Length}):");
                Console.WriteLine($"   平均延迟: {avg:F2} ms");
                Console.WriteLine($"   最小延迟: {min:F2} ms");
                Console.WriteLine($"   最大延迟: {max:F2} ms");
                Console.WriteLine($"   P50延迟: {p50:F2} ms");
                Console.WriteLine($"   P95延迟: {p95:F2} ms");
                Console.WriteLine($"   P99延迟: {p99:F2} ms");
            }

            await client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 延迟测试失败: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}

/// <summary>
/// 延迟测试消息
/// </summary>
public class LatencyTestMessage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("sequenceNumber")]
    public int SequenceNumber { get; set; }
}

/// <summary>
/// 高性能WebSocket性能监控器
/// </summary>
public class HighPerformanceMonitor
{
    private readonly IWebSocketClient _client;
    private readonly Timer _timer;
    private long _lastMessageCount;
    private long _lastByteCount;
    private DateTime _lastCheckTime;

    public HighPerformanceMonitor(IWebSocketClient client)
    {
        _client = client;
        _lastCheckTime = DateTime.UtcNow;
        
        // 每2秒检查一次性能
        _timer = new Timer(CheckPerformance, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
    }

    private void CheckPerformance(object? state)
    {
        try
        {
            var stats = _client.GetStatistics();
            var currentTime = DateTime.UtcNow;
            var elapsed = currentTime - _lastCheckTime;
            
            var currentMessageCount = stats.MessagesReceived;
            var currentByteCount = stats.BytesReceived;
            
            var messageDelta = currentMessageCount - _lastMessageCount;
            var byteDelta = currentByteCount - _lastByteCount;
            
            var messagesPerSecond = messageDelta / elapsed.TotalSeconds;
            var bytesPerSecond = byteDelta / elapsed.TotalSeconds;
            
            Console.WriteLine($"📊 实时监控 - 消息速率: {messagesPerSecond:F0} msg/s, " +
                             $"数据速率: {bytesPerSecond / 1024:F0} KB/s, " +
                             $"总消息: {currentMessageCount}, 错误: {stats.ErrorCount}");
            
            _lastMessageCount = currentMessageCount;
            _lastByteCount = currentByteCount;
            _lastCheckTime = currentTime;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ 监控器错误: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
