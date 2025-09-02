using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Riley.Core.WebSocket;

/// <summary>
/// WebSocket客户端性能基准测试
/// 对比普通版本和高性能版本的差异
/// </summary>
public class WebSocketPerformanceBenchmark
{
    /// <summary>
    /// 基准测试配置
    /// </summary>
    public class BenchmarkConfig
    {
        public string TestUrl { get; set; } = "wss://echo.websocket.org";
        public int MessageCount { get; set; } = 1000;
        public int MessageSize { get; set; } = 100;
        public int ConcurrentConnections { get; set; } = 1;
        public int WarmupMessages { get; set; } = 100;
        public TimeSpan TestTimeout { get; set; } = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// 基准测试结果
    /// </summary>
    public class BenchmarkResult
    {
        public string ClientType { get; set; } = string.Empty;
        public int MessagesSent { get; set; }
        public int MessagesReceived { get; set; }
        public TimeSpan TotalTime { get; set; }
        public double MessagesPerSecond { get; set; }
        public double BytesPerSecond { get; set; }
        public long MemoryUsed { get; set; }
        public int ErrorCount { get; set; }
        public double SuccessRate => MessagesSent > 0 ? (MessagesReceived / (double)MessagesSent) * 100 : 0;
        
        // 延迟统计
        public double AverageLatency { get; set; }
        public double MinLatency { get; set; }
        public double MaxLatency { get; set; }
        public double P95Latency { get; set; }
        public double P99Latency { get; set; }
    }

    /// <summary>
    /// 运行完整的基准测试
    /// </summary>
    public static async Task<List<BenchmarkResult>> RunComprehensiveBenchmarkAsync(BenchmarkConfig config)
    {
        Console.WriteLine("🚀 开始WebSocket性能基准测试\n");
        
        var results = new List<BenchmarkResult>();
        
        // 测试普通版本
        Console.WriteLine("📊 测试普通WebSocket客户端...");
        var standardResult = await RunBenchmarkAsync<WebSocketClient>("Standard WebSocket Client", config);
        results.Add(standardResult);
        
        await Task.Delay(2000); // 短暂休息
        
        // 测试高性能版本
        Console.WriteLine("📊 测试高性能WebSocket客户端...");
        var highPerfResult = await RunBenchmarkAsync<HighPerformanceWebSocketClient>("High Performance WebSocket Client", config);
        results.Add(highPerfResult);
        
        // 显示对比结果
        DisplayComparisonResults(results);
        
        return results;
    }

    /// <summary>
    /// 运行单个客户端的基准测试
    /// </summary>
    private static async Task<BenchmarkResult> RunBenchmarkAsync<TClient>(string clientType, BenchmarkConfig config) 
        where TClient : class, IWebSocketClient
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Error));
        
        var options = new WebSocketOptions
        {
            Url = config.TestUrl,
            ReceiveBufferSize = 16 * 1024,
            SendBufferSize = 16 * 1024,
            MaxMessageQueueLength = 10000,
            EnableAutoReconnect = false,
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
        
        services.AddTransient<IWebSocketClient, TClient>();
        
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IWebSocketClient>();
        
        var result = new BenchmarkResult { ClientType = clientType };
        var receivedCount = 0;
        var errorCount = 0;
        var latencies = new ConcurrentBag<double>();
        var pendingMessages = new ConcurrentDictionary<int, DateTime>();
        
        // 内存监控
        var initialMemory = GC.GetTotalMemory(true);
        
        try
        {
            // 事件处理
            client.MessageReceived += (sender, e) =>
            {
                var received = Interlocked.Increment(ref receivedCount);
                
                // 尝试解析消息ID计算延迟
                if (e.Message.Text?.StartsWith("Benchmark-") == true)
                {
                    var parts = e.Message.Text.Split('-');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var messageId))
                    {
                        if (pendingMessages.TryRemove(messageId, out var sendTime))
                        {
                            var latency = (DateTime.UtcNow - sendTime).TotalMilliseconds;
                            latencies.Add(latency);
                        }
                    }
                }
            };
            
            client.Error += (sender, e) =>
            {
                Interlocked.Increment(ref errorCount);
            };
            
            // 连接
            await client.ConnectAsync();
            _ = Task.Run(() => client.StartReceivingAsync());
            
            // 预热
            if (config.WarmupMessages > 0)
            {
                Console.WriteLine($"  🔥 预热 ({config.WarmupMessages} 条消息)...");
                for (int i = 0; i < config.WarmupMessages; i++)
                {
                    await client.SendTextAsync($"Warmup-{i}");
                }
                await Task.Delay(1000);
            }
            
            // 重置计数器
            receivedCount = 0;
            errorCount = 0;
            latencies = new ConcurrentBag<double>();
            pendingMessages.Clear();
            
            // 开始基准测试
            Console.WriteLine($"  ⏱️  开始基准测试 ({config.MessageCount} 条消息)...");
            var stopwatch = Stopwatch.StartNew();
            
            // 生成测试消息
            var testMessage = new string('A', config.MessageSize);
            
            // 发送消息
            var sendTasks = new List<Task>();
            for (int i = 0; i < config.MessageCount; i++)
            {
                var messageId = i;
                var message = $"Benchmark-{messageId}-{testMessage}";
                
                pendingMessages[messageId] = DateTime.UtcNow;
                sendTasks.Add(client.SendTextAsync(message));
                
                // 批量发送避免过载
                if (sendTasks.Count >= 100)
                {
                    await Task.WhenAll(sendTasks);
                    sendTasks.Clear();
                }
            }
            
            // 等待剩余消息发送
            if (sendTasks.Count > 0)
            {
                await Task.WhenAll(sendTasks);
            }
            
            result.MessagesSent = config.MessageCount;
            
            // 等待接收完成
            var timeout = DateTime.UtcNow.Add(config.TestTimeout);
            while (receivedCount < config.MessageCount && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100);
                
                // 进度报告
                if (receivedCount % 1000 == 0 && receivedCount > 0)
                {
                    var progress = receivedCount * 100.0 / config.MessageCount;
                    Console.WriteLine($"    📈 进度: {progress:F1}% ({receivedCount}/{config.MessageCount})");
                }
            }
            
            stopwatch.Stop();
            
            // 计算结果
            result.MessagesReceived = receivedCount;
            result.TotalTime = stopwatch.Elapsed;
            result.MessagesPerSecond = receivedCount / stopwatch.Elapsed.TotalSeconds;
            result.BytesPerSecond = (receivedCount * (config.MessageSize + 20)) / stopwatch.Elapsed.TotalSeconds; // 估算
            result.ErrorCount = errorCount;
            
            // 内存使用
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(true);
            result.MemoryUsed = finalMemory - initialMemory;
            
            // 延迟统计
            if (latencies.Count > 0)
            {
                var latencyArray = latencies.ToArray();
                Array.Sort(latencyArray);
                
                result.AverageLatency = latencyArray.Average();
                result.MinLatency = latencyArray.Min();
                result.MaxLatency = latencyArray.Max();
                result.P95Latency = latencyArray[(int)(latencyArray.Length * 0.95)];
                result.P99Latency = latencyArray[(int)(latencyArray.Length * 0.99)];
            }
            
            await client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ 基准测试失败: {ex.Message}");
            result.ErrorCount = int.MaxValue;
        }
        finally
        {
            client.Dispose();
        }
        
        return result;
    }

    /// <summary>
    /// 显示对比结果
    /// </summary>
    private static void DisplayComparisonResults(List<BenchmarkResult> results)
    {
        Console.WriteLine("\n📊 基准测试结果对比\n");
        
        if (results.Count < 2) return;
        
        var standard = results[0];
        var highPerf = results[1];
        
        Console.WriteLine("┌─────────────────────────────┬─────────────────┬─────────────────┬──────────────┐");
        Console.WriteLine("│ 指标                        │ 普通版本        │ 高性能版本      │ 性能提升     │");
        Console.WriteLine("├─────────────────────────────┼─────────────────┼─────────────────┼──────────────┤");
        
        // 吞吐量对比
        var throughputImprovement = (highPerf.MessagesPerSecond / standard.MessagesPerSecond - 1) * 100;
        Console.WriteLine($"│ 消息吞吐量 (msg/s)          │ {standard.MessagesPerSecond,15:F0} │ {highPerf.MessagesPerSecond,15:F0} │ {throughputImprovement,11:F1}% │");
        
        // 数据传输速率对比
        var bandwidthImprovement = (highPerf.BytesPerSecond / standard.BytesPerSecond - 1) * 100;
        Console.WriteLine($"│ 数据传输速率 (KB/s)         │ {standard.BytesPerSecond/1024,15:F0} │ {highPerf.BytesPerSecond/1024,15:F0} │ {bandwidthImprovement,11:F1}% │");
        
        // 成功率对比
        Console.WriteLine($"│ 成功率 (%)                  │ {standard.SuccessRate,15:F1} │ {highPerf.SuccessRate,15:F1} │ {highPerf.SuccessRate - standard.SuccessRate,11:F1}% │");
        
        // 内存使用对比
        var memoryImprovement = (1 - highPerf.MemoryUsed / (double)standard.MemoryUsed) * 100;
        Console.WriteLine($"│ 内存使用 (MB)               │ {standard.MemoryUsed/1024/1024,15:F1} │ {highPerf.MemoryUsed/1024/1024,15:F1} │ {memoryImprovement,10:F1}%↓ │");
        
        // 延迟对比
        if (standard.AverageLatency > 0 && highPerf.AverageLatency > 0)
        {
            var latencyImprovement = (1 - highPerf.AverageLatency / standard.AverageLatency) * 100;
            Console.WriteLine($"│ 平均延迟 (ms)               │ {standard.AverageLatency,15:F2} │ {highPerf.AverageLatency,15:F2} │ {latencyImprovement,10:F1}%↓ │");
            
            var p95LatencyImprovement = (1 - highPerf.P95Latency / standard.P95Latency) * 100;
            Console.WriteLine($"│ P95延迟 (ms)                │ {standard.P95Latency,15:F2} │ {highPerf.P95Latency,15:F2} │ {p95LatencyImprovement,10:F1}%↓ │");
        }
        
        // 错误率对比
        Console.WriteLine($"│ 错误数量                    │ {standard.ErrorCount,15} │ {highPerf.ErrorCount,15} │ {highPerf.ErrorCount - standard.ErrorCount,12} │");
        
        Console.WriteLine("└─────────────────────────────┴─────────────────┴─────────────────┴──────────────┘");
        
        // 总结
        Console.WriteLine("\n📈 性能提升总结:");
        
        if (throughputImprovement > 0)
        {
            Console.WriteLine($"✅ 消息吞吐量提升 {throughputImprovement:F1}%");
        }
        
        if (memoryImprovement > 0)
        {
            Console.WriteLine($"✅ 内存使用减少 {memoryImprovement:F1}%");
        }
        
        if (standard.AverageLatency > 0 && highPerf.AverageLatency > 0)
        {
            var latencyImprovement = (1 - highPerf.AverageLatency / standard.AverageLatency) * 100;
            if (latencyImprovement > 0)
            {
                Console.WriteLine($"✅ 平均延迟降低 {latencyImprovement:F1}%");
            }
        }
        
        if (highPerf.ErrorCount < standard.ErrorCount)
        {
            Console.WriteLine($"✅ 错误数量减少 {standard.ErrorCount - highPerf.ErrorCount}");
        }
        
        // 推荐使用场景
        Console.WriteLine("\n🎯 推荐使用场景:");
        
        if (throughputImprovement > 20)
        {
            Console.WriteLine("🔥 高吞吐量场景：高性能版本显著提升吞吐量");
        }
        
        if (memoryImprovement > 10)
        {
            Console.WriteLine("💾 内存敏感场景：高性能版本显著减少内存使用");
        }
        
        if (standard.AverageLatency > 0 && highPerf.AverageLatency > 0)
        {
            var latencyImprovement = (1 - highPerf.AverageLatency / standard.AverageLatency) * 100;
            if (latencyImprovement > 10)
            {
                Console.WriteLine("⚡ 低延迟场景：高性能版本显著降低延迟");
            }
        }
    }

    /// <summary>
    /// 运行简化的快速基准测试
    /// </summary>
    public static async Task RunQuickBenchmarkAsync()
    {
        var config = new BenchmarkConfig
        {
            MessageCount = 1000,
            MessageSize = 100,
            ConcurrentConnections = 1,
            WarmupMessages = 50,
            TestTimeout = TimeSpan.FromMinutes(2)
        };
        
        await RunComprehensiveBenchmarkAsync(config);
    }

    /// <summary>
    /// 运行压力测试
    /// </summary>
    public static async Task RunStressTestAsync()
    {
        var config = new BenchmarkConfig
        {
            MessageCount = 10000,
            MessageSize = 500,
            ConcurrentConnections = 1,
            WarmupMessages = 100,
            TestTimeout = TimeSpan.FromMinutes(10)
        };
        
        await RunComprehensiveBenchmarkAsync(config);
    }
}
