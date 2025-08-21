# WebSocket 客户端使用指南

## 概述

本项目提供了一个完整的WebSocket客户端解决方案，包括：

- **WebSocketClient**: 核心WebSocket客户端类
- **WebSocketService**: 高级服务类，提供消息处理和管理
- **useWebSocket**: Vue组合式函数，用于在组件中使用WebSocket
- **WebSocketStatus**: 状态指示器组件

## 功能特性

- ✅ 自动重连机制
- ✅ 心跳保活
- ✅ 消息队列
- ✅ 事件驱动架构
- ✅ 类型化消息处理
- ✅ Vue 3 Composition API支持
- ✅ Element Plus集成
- ✅ 错误处理和日志

## 快速开始

### 1. 基本使用

```javascript
import { wsService } from '@/common/utils/websocket-service'

// 初始化WebSocket服务
wsService.init()

// 发送消息
wsService.send('notification', {
  title: '系统通知',
  message: '这是一条测试消息',
  type: 'info'
})
```

### 2. 在Vue组件中使用

```vue
<template>
  <div>
    <p>连接状态: {{ isConnected ? '已连接' : '未连接' }}</p>
    <el-button @click="sendMessage">发送消息</el-button>
  </div>
</template>

<script setup>
import { useWebSocket } from '@/common/composables/useWebSocket'

const { isConnected, sendMessage } = useWebSocket()

function sendMessage() {
  sendMessage('chat', {
    sender: '用户',
    message: 'Hello World!'
  })
}
</script>
```

### 3. 使用状态指示器组件

```vue
<template>
  <div>
    <WebSocketStatus :show-text="true" @click="handleStatusClick" />
  </div>
</template>

<script setup>
import WebSocketStatus from '@/common/components/WebSocketStatus.vue'

function handleStatusClick(status) {
  console.log('WebSocket状态:', status)
}
</script>
```

## API 参考

### WebSocketClient

核心WebSocket客户端类。

#### 构造函数选项

```javascript
const client = new WebSocketClient({
  url: '/ws',                    // WebSocket URL
  protocols: [],                 // WebSocket协议
  reconnectInterval: 3000,       // 重连间隔(ms)
  maxReconnectAttempts: 5,       // 最大重连次数
  heartbeatInterval: 30000,      // 心跳间隔(ms)
  heartbeatMessage: 'ping',      // 心跳消息
  autoReconnect: true,           // 自动重连
  debug: false                   // 调试模式
})
```

#### 主要方法

- `connect()`: 连接WebSocket
- `disconnect()`: 断开连接
- `send(data)`: 发送消息
- `on(eventType, handler)`: 注册事件处理器
- `off(eventType, handler)`: 移除事件处理器
- `isConnected()`: 检查连接状态
- `getStatus()`: 获取连接状态

### WebSocketService

高级服务类，提供消息处理和管理功能。

#### 主要方法

- `init()`: 初始化服务
- `destroy()`: 销毁服务
- `send(type, data)`: 发送消息
- `sendNotification(title, message, type)`: 发送通知
- `sendChatMessage(sender, message)`: 发送聊天消息
- `sendSystemMessage(action, message, level)`: 发送系统消息
- `registerHandler(handler)`: 注册消息处理器
- `removeHandler(type)`: 移除消息处理器

### useWebSocket

Vue组合式函数，提供响应式的WebSocket状态和方法。

#### 返回值

```javascript
const {
  // 状态
  isConnected,        // 是否已连接
  status,            // 连接状态
  statusText,        // 状态文本
  reconnectAttempts, // 重连次数
  messages,          // 消息列表
  error,             // 错误信息
  canSend,           // 是否可以发送消息

  // 方法
  connect,           // 连接
  disconnect,        // 断开
  reconnect,         // 重连
  sendMessage,       // 发送消息
  sendNotification,  // 发送通知
  sendChatMessage,   // 发送聊天消息
  sendSystemMessage, // 发送系统消息
  clearMessages,     // 清空消息
  updateStatus       // 更新状态
} = useWebSocket(options)
```

## 消息类型

### 1. 通知消息 (notification)

```javascript
{
  type: 'notification',
  data: {
    title: '通知标题',
    message: '通知内容',
    type: 'info' // success, warning, info, error
  }
}
```

### 2. 聊天消息 (chat)

```javascript
{
  type: 'chat',
  data: {
    sender: '发送者',
    message: '消息内容',
    timestamp: 1640995200000
  }
}
```

### 3. 系统消息 (system)

```javascript
{
  type: 'system',
  data: {
    action: 'maintenance', // maintenance, update, other
    message: '系统消息',
    level: 'info' // success, warning, info, error
  }
}
```

### 4. 心跳消息 (heartbeat)

```javascript
{
  type: 'heartbeat',
  data: {
    timestamp: 1640995200000
  }
}
```

## 自定义消息处理器

```javascript
import { MessageHandler } from '@/common/utils/websocket-service'

class CustomHandler extends MessageHandler {
  constructor() {
    super('custom')
  }

  handle(data) {
    console.log('处理自定义消息:', data)
    // 自定义处理逻辑
  }
}

// 注册处理器
wsService.registerHandler(new CustomHandler())
```

## 配置说明

### 开发环境配置

在 `vite.proxy.config.js` 中配置WebSocket代理：

```javascript
'/ws': {
  target: 'ws://localhost:3000',
  changeOrigin: true,
  ws: true,
  rewrite: (path) => path.replace(/^\/ws/, '')
}
```

### 生产环境配置

在生产环境中，WebSocket URL会使用环境变量：

```javascript
// 开发环境: ws://localhost:8080/ws
// 生产环境: wss://api.riley.com/ws
```

## 最佳实践

### 1. 错误处理

```javascript
const { error, reconnect } = useWebSocket()

// 监听错误
watch(error, (newError) => {
  if (newError) {
    console.error('WebSocket错误:', newError)
    // 可以显示错误提示或自动重连
  }
})
```

### 2. 消息队列

当WebSocket未连接时，消息会自动加入队列，连接恢复后自动发送：

```javascript
// 即使未连接，消息也会被缓存
wsService.send('notification', {
  title: '离线消息',
  message: '这条消息会在连接恢复后发送'
})
```

### 3. 生命周期管理

```javascript
// 在组件中使用
onMounted(() => {
  wsService.init()
})

onUnmounted(() => {
  wsService.destroy()
})
```

### 4. 状态监控

```javascript
const { isConnected, status } = useWebSocket()

// 监控连接状态变化
watch(isConnected, (connected) => {
  if (connected) {
    console.log('WebSocket已连接')
  } else {
    console.log('WebSocket已断开')
  }
})
```

## 测试页面

访问 `/example/websocket-test` 页面可以测试WebSocket功能：

- 连接/断开控制
- 发送不同类型的消息
- 查看消息历史
- 监控连接状态

## 注意事项

1. **浏览器兼容性**: 确保浏览器支持WebSocket
2. **HTTPS要求**: 在生产环境中，如果使用HTTPS，WebSocket也必须使用WSS
3. **连接限制**: 注意浏览器的并发连接数限制
4. **内存管理**: 及时清理事件监听器，避免内存泄漏
5. **错误处理**: 实现适当的错误处理和用户提示

## 故障排除

### 常见问题

1. **连接失败**: 检查WebSocket服务器是否运行，URL是否正确
2. **消息丢失**: 检查网络连接，确保心跳机制正常工作
3. **重连失败**: 检查重连配置，可能需要调整重连间隔和次数
4. **性能问题**: 检查消息频率，避免发送过多消息

### 调试模式

启用调试模式查看详细日志：

```javascript
const client = new WebSocketClient({
  debug: true
})
```

这将输出详细的连接、消息和错误日志到控制台。
