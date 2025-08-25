using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Riley.Core.WebSocket;
using System.Text.Json.Serialization;

namespace Riley.Core.Examples;

/// <summary>
/// WebSocket客户端使用示例
/// </summary>
public static class WebSocketUsageExamples
{
    /// <summary>
    /// 基本WebSocket使用示例
    /// </summary>
    public static async Task BasicWebSocketExample()
    {
        Console.WriteLine("=== 基本WebSocket使用示例 ===");

        // 1. 配置服务
        var services = new ServiceCollection();
        //services.AddLogging(builder => builder.AddConsole());
        
        services.AddWebSocketClient(options =>
        {
            options.Url = "wss://echo.websocket.org"; // 使用公共的WebSocket测试服务
            options.ConnectTimeoutMs = 10000;
            options.EnableAutoReconnect = true;
            options.ReconnectIntervalMs = 3000;
            options.MaxReconnectAttempts = 5;
            options.HeartbeatIntervalMs = 30000;
            options.HeartbeatMessage = "ping";
            options.HeartbeatResponse = "ping"; // echo服务会回显消息
        });

        var serviceProvider = services.BuildServiceProvider();
        var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

        try
        {
            // 2. 订阅事件
            webSocketClient.Connected += (sender, e) =>
            {
                Console.WriteLine($"✅ 连接成功: {e.Url}");
                if (!string.IsNullOrEmpty(e.SubProtocol))
                {
                    Console.WriteLine($"   使用子协议: {e.SubProtocol}");
                }
            };

            webSocketClient.Disconnected += (sender, e) =>
            {
                Console.WriteLine($"❌ 连接断开: {e.Reason}");
                Console.WriteLine($"   状态码: {e.CloseStatus}");
                Console.WriteLine($"   是否预期: {e.IsExpected}");
                Console.WriteLine($"   将重连: {e.WillReconnect}");
            };

            webSocketClient.MessageReceived += (sender, e) =>
            {
                Console.WriteLine($"📨 收到消息 [{e.Message.MessageType}]: {e.Message.GetText()}");
            };

            webSocketClient.Error += (sender, e) =>
            {
                Console.WriteLine($"⚠️  发生错误: {e.Message}");
                if (!string.IsNullOrEmpty(e.Context))
                {
                    Console.WriteLine($"   上下文: {e.Context}");
                }
            };

            webSocketClient.Reconnecting += (sender, e) =>
            {
                Console.WriteLine($"🔄 正在重连 (第{e.AttemptCount}次), 延迟{e.DelayMs}ms");
            };

            // 3. 连接到WebSocket服务器
            await webSocketClient.ConnectAsync();
            
            // 4. 等待连接建立
            await webSocketClient.WaitForConnectionAsync(TimeSpan.FromSeconds(10));

            // 5. 开始接收消息
            _ = Task.Run(() => webSocketClient.StartReceivingAsync());

            // 6. 发送各种类型的消息
            Console.WriteLine("\n--- 发送消息测试 ---");
            
            // 发送文本消息
            await webSocketClient.SendTextAsync("Hello WebSocket!");
            await Task.Delay(1000);

            // 发送JSON消息
            var testData = new { Type = "test", Message = "这是一个JSON消息", Timestamp = DateTime.Now };
            await webSocketClient.SendJsonAsync(testData);
            await Task.Delay(1000);

            // 发送二进制消息
            var binaryData = System.Text.Encoding.UTF8.GetBytes("Binary message");
            await webSocketClient.SendBinaryAsync(binaryData);
            await Task.Delay(1000);

            // 7. 显示连接统计
            var stats = webSocketClient.GetStatistics();
            Console.WriteLine("\n--- 连接统计 ---");
            Console.WriteLine($"连接时间: {stats.ConnectedAt}");
            Console.WriteLine($"连接持续: {stats.ConnectedDuration}");
            Console.WriteLine($"发送消息: {stats.MessagesSent}");
            Console.WriteLine($"接收消息: {stats.MessagesReceived}");
            Console.WriteLine($"发送字节: {stats.BytesSent}");
            Console.WriteLine($"接收字节: {stats.BytesReceived}");

            // 8. 等待一段时间观察消息
            Console.WriteLine("\n等待5秒观察消息交互...");
            await Task.Delay(5000);

            // 9. 断开连接
            await webSocketClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"示例执行出错: {ex.Message}");
        }
        finally
        {
            webSocketClient.Dispose();
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 消息处理器示例
    /// </summary>
    public static async Task MessageHandlerExample()
    {
        Console.WriteLine("\n=== 消息处理器示例 ===");

        var services = new ServiceCollection();
        services.AddWebSocketClient(options =>
        {
            options.Url = "wss://echo.websocket.org";
            options.ConnectTimeoutMs = 10000;
        });

        var serviceProvider = services.BuildServiceProvider();
        var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

        try
        {
            // 创建类型化消息处理器
            var messageHandler = new TypedWebSocketMessageHandler();

            // 注册不同类型的消息处理器
            messageHandler.RegisterHandler("greeting", async (message, cancellationToken) =>
            {
                Console.WriteLine($"🎉 收到问候消息: {message.GetText()}");
                await Task.CompletedTask;
            });

            messageHandler.RegisterHandler<UserMessage>("userMessage", async (userMsg, cancellationToken) =>
            {
                Console.WriteLine($"👤 用户消息 - {userMsg.Username}: {userMsg.Content}");
                await Task.CompletedTask;
            });

            messageHandler.RegisterHandler<SystemNotification>("notification", async (notification, cancellationToken) =>
            {
                Console.WriteLine($"🔔 系统通知 [{notification.Level}]: {notification.Message}");
                await Task.CompletedTask;
            });

            // 订阅消息接收事件并使用处理器
            webSocketClient.MessageReceived += async (sender, e) =>
            {
                await messageHandler.HandleMessageAsync(e.Message);
            };

            // 连接
            await webSocketClient.ConnectAsync();
            _ = Task.Run(() => webSocketClient.StartReceivingAsync());

            // 发送不同类型的消息进行测试
            Console.WriteLine("\n--- 发送类型化消息 ---");

            // 发送问候消息
            var greeting = new { type = "greeting", message = "Hello from client!" };
            await webSocketClient.SendJsonAsync(greeting);
            await Task.Delay(500);

            // 发送用户消息
            var userMessage = new { type = "userMessage", username = "Alice", content = "这是用户消息", timestamp = DateTime.Now };
            await webSocketClient.SendJsonAsync(userMessage);
            await Task.Delay(500);

            // 发送系统通知
            var notification = new { type = "notification", level = "info", message = "系统运行正常", timestamp = DateTime.Now };
            await webSocketClient.SendJsonAsync(notification);
            await Task.Delay(500);

            Console.WriteLine("\n等待3秒观察消息处理...");
            await Task.Delay(3000);

            await webSocketClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"消息处理器示例出错: {ex.Message}");
        }
        finally
        {
            webSocketClient.Dispose();
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 多客户端示例
    /// </summary>
    public static async Task MultipleClientsExample()
    {
        Console.WriteLine("\n=== 多客户端示例 ===");

        var services = new ServiceCollection();
        
        // 配置多个WebSocket客户端
        services.AddWebSocketClients(new Dictionary<string, Action<WebSocketOptions>>
        {
            ["client1"] = options =>
            {
                options.Url = "wss://echo.websocket.org";
                options.UserAgent = "Riley.Core.Client1/1.0";
                options.HeartbeatIntervalMs = 15000;
            },
            ["client2"] = options =>
            {
                options.Url = "wss://echo.websocket.org";
                options.UserAgent = "Riley.Core.Client2/1.0";
                options.HeartbeatIntervalMs = 20000;
            }
        });

        var serviceProvider = services.BuildServiceProvider();
        var clientFactory = serviceProvider.GetRequiredService<IWebSocketClientFactory>();

        try
        {
            // 创建多个客户端
            var client1 = clientFactory.CreateClient("client1");
            var client2 = clientFactory.CreateClient("client2");

            // 为每个客户端设置事件处理
            SetupClientEvents(client1, "Client1");
            SetupClientEvents(client2, "Client2");

            // 并发连接
            var connectTasks = new[]
            {
                client1.ConnectAsync(),
                client2.ConnectAsync()
            };

            await Task.WhenAll(connectTasks);

            // 开始接收消息
            _ = Task.Run(() => client1.StartReceivingAsync());
            _ = Task.Run(() => client2.StartReceivingAsync());

            // 发送消息测试
            Console.WriteLine("\n--- 多客户端消息测试 ---");
            
            await client1.SendTextAsync("Message from Client1");
            await Task.Delay(500);
            
            await client2.SendTextAsync("Message from Client2");
            await Task.Delay(500);

            // 并发发送消息
            var sendTasks = new[]
            {
                client1.SendJsonAsync(new { client = "client1", message = "Concurrent message 1", time = DateTime.Now }),
                client2.SendJsonAsync(new { client = "client2", message = "Concurrent message 2", time = DateTime.Now })
            };

            await Task.WhenAll(sendTasks);

            Console.WriteLine("\n等待3秒观察消息交互...");
            await Task.Delay(3000);

            // 显示统计信息
            Console.WriteLine("\n--- 客户端统计 ---");
            ShowClientStats(client1, "Client1");
            ShowClientStats(client2, "Client2");

            // 断开连接
            await Task.WhenAll(
                client1.DisconnectAsync(),
                client2.DisconnectAsync()
            );

            client1.Dispose();
            client2.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"多客户端示例出错: {ex.Message}");
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 重连机制示例
    /// </summary>
    public static async Task ReconnectionExample()
    {
        Console.WriteLine("\n=== 重连机制示例 ===");

        var services = new ServiceCollection();
        services.AddWebSocketClient(options =>
        {
            options.Url = "wss://echo.websocket.org";
            options.EnableAutoReconnect = true;
            options.ReconnectIntervalMs = 2000;
            options.MaxReconnectAttempts = 3;
            options.HeartbeatIntervalMs = 5000;
        });

        var serviceProvider = services.BuildServiceProvider();
        var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

        try
        {
            var reconnectCount = 0;

            webSocketClient.Connected += (sender, e) =>
            {
                Console.WriteLine($"✅ 连接成功 (重连次数: {reconnectCount})");
            };

            webSocketClient.Disconnected += (sender, e) =>
            {
                Console.WriteLine($"❌ 连接断开: {e.Reason} (将重连: {e.WillReconnect})");
            };

            webSocketClient.Reconnecting += (sender, e) =>
            {
                reconnectCount = e.AttemptCount;
                Console.WriteLine($"🔄 正在进行第 {e.AttemptCount} 次重连...");
            };

            webSocketClient.Error += (sender, e) =>
            {
                Console.WriteLine($"⚠️  错误: {e.Message}");
            };

            // 连接
            await webSocketClient.ConnectAsync();
            _ = Task.Run(() => webSocketClient.StartReceivingAsync());

            Console.WriteLine("连接建立，等待5秒...");
            await Task.Delay(5000);

            // 模拟连接断开（通过断开网络或关闭连接）
            Console.WriteLine("模拟连接断开...");
            await webSocketClient.DisconnectAsync(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, "模拟断开");

            // 等待自动重连
            Console.WriteLine("等待自动重连...");
            await Task.Delay(10000);

            // 检查连接状态
            if (webSocketClient.IsConnected)
            {
                Console.WriteLine("✅ 重连成功！");
                await webSocketClient.SendTextAsync("重连后的测试消息");
            }
            else
            {
                Console.WriteLine("❌ 重连失败");
            }

            await Task.Delay(3000);
            await webSocketClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"重连示例出错: {ex.Message}");
        }
        finally
        {
            webSocketClient.Dispose();
            serviceProvider.Dispose();
        }
    }

    /// <summary>
    /// 实时聊天示例
    /// </summary>
    public static async Task RealtimeChatExample()
    {
        Console.WriteLine("\n=== 实时聊天示例 ===");

        var services = new ServiceCollection();
        services.AddWebSocketClient(options =>
        {
            options.Url = "wss://echo.websocket.org";
            options.EnableAutoReconnect = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

        try
        {
            // 聊天消息处理器
            var chatHandler = new TypedWebSocketMessageHandler();
            
            chatHandler.RegisterHandler<ChatMessage>("chat", async (chatMsg, cancellationToken) =>
            {
                var timestamp = chatMsg.Timestamp.ToString("HH:mm:ss");
                Console.WriteLine($"[{timestamp}] {chatMsg.Username}: {chatMsg.Message}");
                await Task.CompletedTask;
            });

            chatHandler.RegisterHandler<UserJoinedMessage>("userJoined", async (joinMsg, cancellationToken) =>
            {
                Console.WriteLine($"🟢 {joinMsg.Username} 加入了聊天室");
                await Task.CompletedTask;
            });

            chatHandler.RegisterHandler<UserLeftMessage>("userLeft", async (leftMsg, cancellationToken) =>
            {
                Console.WriteLine($"🔴 {leftMsg.Username} 离开了聊天室");
                await Task.CompletedTask;
            });

            webSocketClient.MessageReceived += async (sender, e) =>
            {
                await chatHandler.HandleMessageAsync(e.Message);
            };

            webSocketClient.Connected += (sender, e) =>
            {
                Console.WriteLine("📱 已连接到聊天服务器");
            };

            // 连接并开始接收
            await webSocketClient.ConnectAsync();
            _ = Task.Run(() => webSocketClient.StartReceivingAsync());

            // 模拟用户加入
            var joinMessage = new { type = "userJoined", username = "Alice", timestamp = DateTime.Now };
            await webSocketClient.SendJsonAsync(joinMessage);
            await Task.Delay(500);

            // 模拟聊天消息
            var chatMessages = new[]
            {
                new { type = "chat", username = "Alice", message = "大家好！", timestamp = DateTime.Now },
                new { type = "chat", username = "Bob", message = "欢迎Alice！", timestamp = DateTime.Now.AddSeconds(1) },
                new { type = "chat", username = "Alice", message = "这个聊天室很棒", timestamp = DateTime.Now.AddSeconds(2) },
                new { type = "chat", username = "Charlie", message = "同意！", timestamp = DateTime.Now.AddSeconds(3) }
            };

            Console.WriteLine("\n--- 模拟聊天对话 ---");
            foreach (var msg in chatMessages)
            {
                await webSocketClient.SendJsonAsync(msg);
                await Task.Delay(1000);
            }

            // 模拟用户离开
            var leftMessage = new { type = "userLeft", username = "Bob", timestamp = DateTime.Now };
            await webSocketClient.SendJsonAsync(leftMessage);

            await Task.Delay(2000);
            await webSocketClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"聊天示例出错: {ex.Message}");
        }
        finally
        {
            webSocketClient.Dispose();
            serviceProvider.Dispose();
        }
    }

    #region 辅助方法

    private static void SetupClientEvents(IWebSocketClient client, string clientName)
    {
        client.Connected += (sender, e) =>
        {
            Console.WriteLine($"✅ {clientName} 连接成功");
        };

        client.Disconnected += (sender, e) =>
        {
            Console.WriteLine($"❌ {clientName} 连接断开: {e.Reason}");
        };

        client.MessageReceived += (sender, e) =>
        {
            Console.WriteLine($"📨 {clientName} 收到: {e.Message.GetText()}");
        };

        client.Error += (sender, e) =>
        {
            Console.WriteLine($"⚠️  {clientName} 错误: {e.Message}");
        };
    }

    private static void ShowClientStats(IWebSocketClient client, string clientName)
    {
        var stats = client.GetStatistics();
        Console.WriteLine($"{clientName} 统计:");
        Console.WriteLine($"  发送消息: {stats.MessagesSent}");
        Console.WriteLine($"  接收消息: {stats.MessagesReceived}");
        Console.WriteLine($"  连接时长: {stats.ConnectedDuration}");
    }

    #endregion
}

#region 消息模型

/// <summary>
/// 用户消息
/// </summary>
public class UserMessage
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 系统通知
/// </summary>
public class SystemNotification
{
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 聊天消息
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 用户加入消息
/// </summary>
public class UserJoinedMessage
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 用户离开消息
/// </summary>
public class UserLeftMessage
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

#endregion
