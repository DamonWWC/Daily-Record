# Riley.Core WebSocket客户端

一个功能强大、易于使用的.NET 8 WebSocket客户端库，提供完整的WebSocket通信功能。

## 🚀 特性

- ✅ **完整的WebSocket支持**: 支持文本、二进制和JSON消息
- 🔄 **自动重连机制**: 可配置的自动重连策略
- 💓 **心跳检测**: 内置心跳机制保持连接活跃
- 📨 **消息处理器**: 灵活的消息路由和处理系统
- 🏭 **依赖注入**: 原生支持DI容器和工厂模式
- 📊 **连接统计**: 详细的连接和消息统计信息
- 🛡️ **错误处理**: 完善的错误处理和事件通知
- 🔧 **高度可配置**: 支持超时、缓冲区、SSL等配置
- 👥 **多客户端**: 支持同时管理多个WebSocket连接

## 📦 快速开始

### 1. 配置服务

```csharp
using Microsoft.Extensions.DependencyInjection;
using Riley.Core.WebSocket;

var services = new ServiceCollection();

// 添加日志
services.AddLogging();

// 添加WebSocket客户端
services.AddWebSocketClient(options =>
{
    options.Url = "wss://your-websocket-server.com/ws";
    options.EnableAutoReconnect = true;
    options.ReconnectIntervalMs = 5000;
    options.HeartbeatIntervalMs = 30000;
});

var serviceProvider = services.BuildServiceProvider();
```

### 2. 基本使用

```csharp
var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

// 订阅事件
webSocketClient.Connected += (sender, e) => 
{
    Console.WriteLine($"连接成功: {e.Url}");
};

webSocketClient.MessageReceived += (sender, e) => 
{
    Console.WriteLine($"收到消息: {e.Message.GetText()}");
};

webSocketClient.Error += (sender, e) => 
{
    Console.WriteLine($"发生错误: {e.Message}");
};

// 连接
await webSocketClient.ConnectAsync();

// 开始接收消息
_ = Task.Run(() => webSocketClient.StartReceivingAsync());

// 发送消息
await webSocketClient.SendTextAsync("Hello WebSocket!");
await webSocketClient.SendJsonAsync(new { type = "greeting", message = "Hello" });

// 断开连接
await webSocketClient.DisconnectAsync();
```

## 📖 详细用法

### 配置选项

```csharp
services.AddWebSocketClient(options =>
{
    // 基本配置
    options.Url = "wss://example.com/ws";
    options.ConnectTimeoutMs = 30000;
    options.ReceiveBufferSize = 4096;
    options.SendBufferSize = 4096;
    
    // 自动重连
    options.EnableAutoReconnect = true;
    options.ReconnectIntervalMs = 5000;
    options.MaxReconnectAttempts = 10; // -1表示无限重连
    
    // 心跳配置
    options.HeartbeatIntervalMs = 30000;
    options.HeartbeatMessage = "ping";
    options.HeartbeatResponse = "pong";
    
    // 请求头和子协议
    options.Headers.Add("Authorization", "Bearer your-token");
    options.SubProtocols.Add("chat");
    
    // SSL配置
    options.IgnoreSslErrors = false;
    options.UserAgent = "MyApp/1.0";
});
```

### 消息处理器

#### 类型化消息处理

```csharp
var messageHandler = new TypedWebSocketMessageHandler();

// 注册不同类型的消息处理器
messageHandler.RegisterHandler("chat", async (message, cancellationToken) =>
{
    Console.WriteLine($"聊天消息: {message.GetText()}");
});

messageHandler.RegisterHandler<UserMessage>("userMessage", async (userMsg, cancellationToken) =>
{
    Console.WriteLine($"用户消息 - {userMsg.Username}: {userMsg.Content}");
});

// 在消息接收事件中使用处理器
webSocketClient.MessageReceived += async (sender, e) =>
{
    await messageHandler.HandleMessageAsync(e.Message);
};
```

#### 消息路由器

```csharp
var router = new WebSocketMessageRouter();

// 添加路由处理器
router.AddRouteHandler("/chat", new FunctionalWebSocketMessageHandler(message =>
{
    Console.WriteLine($"聊天路由: {message.GetText()}");
}));

router.AddRouteHandler("/notification", new FunctionalWebSocketMessageHandler(message =>
{
    Console.WriteLine($"通知路由: {message.GetText()}");
}));

// 路由消息
await router.RouteMessageAsync("/chat", message);
```

### 多客户端管理

```csharp
// 配置多个命名客户端
services.AddWebSocketClients(new Dictionary<string, Action<WebSocketOptions>>
{
    ["chat"] = options =>
    {
        options.Url = "wss://chat.example.com/ws";
        options.HeartbeatIntervalMs = 15000;
    },
    ["notifications"] = options =>
    {
        options.Url = "wss://notifications.example.com/ws";
        options.HeartbeatIntervalMs = 30000;
    }
});

// 使用工厂创建客户端
var factory = serviceProvider.GetRequiredService<IWebSocketClientFactory>();
var chatClient = factory.CreateClient("chat");
var notificationClient = factory.CreateClient("notifications");

// 并发连接
await Task.WhenAll(
    chatClient.ConnectAsync(),
    notificationClient.ConnectAsync()
);
```

### 事件处理

```csharp
webSocketClient.Connected += (sender, e) =>
{
    Console.WriteLine($"✅ 连接成功: {e.Url}");
    if (!string.IsNullOrEmpty(e.SubProtocol))
    {
        Console.WriteLine($"   子协议: {e.SubProtocol}");
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
    var message = e.Message;
    Console.WriteLine($"📨 收到消息 [{message.MessageType}]");
    
    if (message.MessageType == WebSocketMessageType.Text)
    {
        Console.WriteLine($"   内容: {message.GetText()}");
    }
    else if (message.MessageType == WebSocketMessageType.Binary)
    {
        Console.WriteLine($"   大小: {message.GetBytes().Length} bytes");
    }
};

webSocketClient.Error += (sender, e) =>
{
    Console.WriteLine($"⚠️  错误: {e.Message}");
    if (!string.IsNullOrEmpty(e.Context))
    {
        Console.WriteLine($"   上下文: {e.Context}");
    }
};

webSocketClient.Reconnecting += (sender, e) =>
{
    Console.WriteLine($"🔄 重连中 (第{e.AttemptCount}次)");
    
    // 可以取消重连
    if (e.AttemptCount > 5)
    {
        e.Cancel = true;
    }
};
```

### 发送不同类型的消息

```csharp
// 文本消息
await webSocketClient.SendTextAsync("Hello World!");

// JSON消息
var data = new { type = "message", content = "Hello", timestamp = DateTime.Now };
await webSocketClient.SendJsonAsync(data);

// 二进制消息
var binaryData = Encoding.UTF8.GetBytes("Binary content");
await webSocketClient.SendBinaryAsync(binaryData);

// 自定义消息
var customMessage = WebSocketMessage.CreateText("Custom message");
customMessage.Id = "msg-001";
await webSocketClient.SendMessageAsync(customMessage);
```

### 连接管理

```csharp
// 检查连接状态
if (webSocketClient.IsConnected)
{
    Console.WriteLine("WebSocket已连接");
}

// 等待连接建立
await webSocketClient.WaitForConnectionAsync(TimeSpan.FromSeconds(10));

// 获取连接统计
var stats = webSocketClient.GetStatistics();
Console.WriteLine($"连接时间: {stats.ConnectedAt}");
Console.WriteLine($"发送消息: {stats.MessagesSent}");
Console.WriteLine($"接收消息: {stats.MessagesReceived}");
Console.WriteLine($"重连次数: {stats.ReconnectCount}");

// 手动重连
await webSocketClient.ReconnectAsync();

// 优雅断开
await webSocketClient.DisconnectAsync(WebSocketCloseStatus.NormalClosure, "正常关闭");
```

## 🎯 实际应用场景

### 实时聊天应用

```csharp
services.AddWebSocketClient(options =>
{
    options.Url = "wss://chat.example.com/ws";
    options.EnableAutoReconnect = true;
    options.HeartbeatIntervalMs = 30000;
});

var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

// 聊天消息处理
var chatHandler = new TypedWebSocketMessageHandler();
chatHandler.RegisterHandler<ChatMessage>("chat", async (chatMsg, cancellationToken) =>
{
    Console.WriteLine($"[{chatMsg.Timestamp:HH:mm:ss}] {chatMsg.Username}: {chatMsg.Message}");
});

webSocketClient.MessageReceived += async (sender, e) =>
{
    await chatHandler.HandleMessageAsync(e.Message);
};

await webSocketClient.ConnectAsync();
_ = Task.Run(() => webSocketClient.StartReceivingAsync());

// 发送聊天消息
await webSocketClient.SendJsonAsync(new 
{ 
    type = "chat", 
    username = "Alice", 
    message = "Hello everyone!", 
    timestamp = DateTime.Now 
});
```

### 实时数据推送

```csharp
services.AddWebSocketClient(options =>
{
    options.Url = "wss://data.example.com/realtime";
    options.EnableAutoReconnect = true;
    options.MaxReconnectAttempts = -1; // 无限重连
});

var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

// 数据处理器
var dataHandler = new TypedWebSocketMessageHandler();
dataHandler.RegisterHandler<StockPrice>("stockPrice", async (price, cancellationToken) =>
{
    Console.WriteLine($"股票 {price.Symbol}: {price.Price:C} ({price.Change:+0.00;-0.00})");
});

dataHandler.RegisterHandler<SystemAlert>("alert", async (alert, cancellationToken) =>
{
    Console.WriteLine($"🚨 系统告警 [{alert.Level}]: {alert.Message}");
});

webSocketClient.MessageReceived += async (sender, e) =>
{
    await dataHandler.HandleMessageAsync(e.Message);
};

await webSocketClient.ConnectAsync();
_ = Task.Run(() => webSocketClient.StartReceivingAsync());

// 订阅数据
await webSocketClient.SendJsonAsync(new 
{ 
    type = "subscribe", 
    channels = new[] { "AAPL", "GOOGL", "MSFT" } 
});
```

### 游戏实时通信

```csharp
services.AddWebSocketClient(options =>
{
    options.Url = "wss://game.example.com/ws";
    options.HeartbeatIntervalMs = 10000; // 游戏需要更频繁的心跳
    options.EnableAutoReconnect = true;
});

var webSocketClient = serviceProvider.GetRequiredService<IWebSocketClient>();

// 游戏消息处理
var gameHandler = new TypedWebSocketMessageHandler();
gameHandler.RegisterHandler<PlayerMove>("playerMove", async (move, cancellationToken) =>
{
    // 更新玩家位置
    UpdatePlayerPosition(move.PlayerId, move.X, move.Y);
});

gameHandler.RegisterHandler<GameEvent>("gameEvent", async (evt, cancellationToken) =>
{
    // 处理游戏事件
    HandleGameEvent(evt);
});

webSocketClient.MessageReceived += async (sender, e) =>
{
    await gameHandler.HandleMessageAsync(e.Message);
};

// 发送玩家操作
await webSocketClient.SendJsonAsync(new 
{ 
    type = "playerMove", 
    playerId = "player123", 
    x = 100, 
    y = 200, 
    timestamp = DateTime.Now 
});
```

## ⚙️ 高级配置

### SSL/TLS配置

```csharp
services.AddWebSocketClient(options =>
{
    options.Url = "wss://secure.example.com/ws";
    options.IgnoreSslErrors = false; // 生产环境应为false
    options.Headers.Add("Authorization", "Bearer your-jwt-token");
});
```

### 自定义消息过滤器

```csharp
// 创建过滤器
var textFilter = new MessageTypeFilter(WebSocketMessageType.Text);
var contentFilter = new TextContentFilter(text => text.Contains("important"));
var compositeFilter = new CompositeFilter(true, textFilter, contentFilter);

// 在消息处理中使用过滤器
webSocketClient.MessageReceived += (sender, e) =>
{
    if (compositeFilter.Filter(e.Message))
    {
        // 处理重要的文本消息
        HandleImportantMessage(e.Message);
    }
};
```

### 性能优化

```csharp
services.AddWebSocketClient(options =>
{
    // 调整缓冲区大小
    options.ReceiveBufferSize = 8192;
    options.SendBufferSize = 8192;
    
    // 消息队列限制
    options.MaxMessageQueueLength = 1000;
    
    // 启用压缩（如果服务器支持）
    options.EnableCompression = true;
    
    // 优化心跳间隔
    options.HeartbeatIntervalMs = 60000;
});
```

## 🔧 故障排除

### 常见问题

1. **连接超时**
   ```csharp
   options.ConnectTimeoutMs = 60000; // 增加超时时间
   ```

2. **频繁重连**
   ```csharp
   options.ReconnectIntervalMs = 10000; // 增加重连间隔
   options.MaxReconnectAttempts = 5; // 限制重连次数
   ```

3. **消息丢失**
   ```csharp
   options.MaxMessageQueueLength = 5000; // 增加消息队列长度
   ```

4. **SSL证书问题**
   ```csharp
   options.IgnoreSslErrors = true; // 仅用于开发环境
   ```

### 调试技巧

```csharp
// 启用详细日志
services.AddLogging(builder => 
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// 监控所有事件
webSocketClient.Connected += (s, e) => Console.WriteLine($"Connected: {e.Url}");
webSocketClient.Disconnected += (s, e) => Console.WriteLine($"Disconnected: {e.Reason}");
webSocketClient.Error += (s, e) => Console.WriteLine($"Error: {e.Message}");
webSocketClient.Reconnecting += (s, e) => Console.WriteLine($"Reconnecting: {e.AttemptCount}");

// 定期检查统计信息
var timer = new Timer(_ =>
{
    var stats = webSocketClient.GetStatistics();
    Console.WriteLine($"Stats: Sent={stats.MessagesSent}, Received={stats.MessagesReceived}");
}, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
```

## 📚 完整示例

查看 `Examples/WebSocketUsageExamples.cs` 文件获取完整的使用示例，包括：

- ✅ 基本WebSocket连接和消息收发
- ✅ 消息处理器和路由系统
- ✅ 多客户端管理
- ✅ 自动重连机制演示
- ✅ 实时聊天应用示例
- ✅ 错误处理和统计信息

## 🤝 最佳实践

1. **资源管理**: 始终在using语句中使用WebSocket客户端或手动调用Dispose
2. **异常处理**: 订阅Error事件处理所有WebSocket相关异常
3. **重连策略**: 根据应用场景配置合适的重连参数
4. **消息处理**: 使用消息处理器模式组织复杂的消息处理逻辑
5. **性能监控**: 定期检查连接统计信息，监控应用性能
6. **安全性**: 在生产环境中正确配置SSL和认证

## 📄 许可证

MIT License
