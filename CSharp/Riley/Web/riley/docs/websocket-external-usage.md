# WebSocket 外部监听使用指南

## 概述

WebSocket服务现在支持两种消息处理方式：

1. **内置处理器**：自动处理消息并显示UI（通知、聊天、系统消息）
2. **外部监听**：允许外部代码监听和处理接收到的消息

这种设计既保持了原有的自动化处理功能，又提供了灵活的外部访问接口。

## 核心功能

### 1. 事件监听系统
- 支持监听所有消息或特定类型消息
- 提供便捷的监听方法
- 支持多个监听器同时存在
- 自动错误处理

### 2. 消息历史管理
- 自动记录所有接收的消息
- 支持按类型、时间、数量过滤
- 可配置历史记录大小
- 提供多种查询方法

### 3. 双重处理机制
- 内置处理器继续工作（显示通知等）
- 外部监听器同时接收消息
- 互不干扰，各司其职

## 使用示例

### 基础用法

```javascript
import { ws } from '@/common/utils/websocket-service'

// 1. 初始化连接
ws.init()

// 2. 监听所有消息
ws.onMessage((data, eventType) => {
  console.log('收到消息:', eventType, data)
  // 自定义处理逻辑
})

// 3. 监听特定类型消息
ws.onNotification((data) => {
  console.log('收到通知:', data.data.title)
  // 处理通知消息
})

ws.onChat((data) => {
  console.log('收到聊天:', data.data.sender, data.data.message)
  // 处理聊天消息
})

ws.onSystem((data) => {
  console.log('收到系统消息:', data.data.action)
  // 处理系统消息
})
```

### Vue 组件中使用

```vue
<template>
  <div>
    <div v-for="message in realtimeMessages" :key="message.id">
      {{ message.type }}: {{ JSON.stringify(message.data) }}
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { ws } from '@/common/utils/websocket-service'

const realtimeMessages = ref([])

// 消息监听器
const messageListener = (data) => {
  realtimeMessages.value.unshift({
    ...data,
    id: Date.now()
  })
  
  // 限制显示数量
  if (realtimeMessages.value.length > 50) {
    realtimeMessages.value = realtimeMessages.value.slice(0, 50)
  }
}

onMounted(() => {
  // 初始化连接
  ws.init()
  
  // 设置监听器
  ws.onMessage(messageListener)
})

onUnmounted(() => {
  // 清理监听器
  ws.off('message', messageListener)
})
</script>
```

### 消息历史查询

```javascript
// 获取所有历史消息
const allMessages = ws.getHistory()

// 获取最近10条消息
const recentMessages = ws.getLatest(10)

// 获取通知类型的最近5条消息
const recentNotifications = ws.getLatestByType('notification', 5)

// 按条件过滤消息
const filteredMessages = ws.getHistory({
  type: 'chat',           // 只要聊天消息
  startTime: Date.now() - 3600000,  // 最近1小时
  limit: 20               // 最多20条
})

// 获取今天的系统消息
const todayStart = new Date()
todayStart.setHours(0, 0, 0, 0)

const todaySystemMessages = ws.getHistory({
  type: 'system',
  startTime: todayStart.getTime()
})
```

### 实时数据统计

```javascript
// 统计不同类型消息数量
function getMessageStats() {
  const allMessages = ws.getHistory()
  
  const stats = {
    total: allMessages.length,
    notification: 0,
    chat: 0,
    system: 0
  }
  
  allMessages.forEach(msg => {
    if (stats[msg.type] !== undefined) {
      stats[msg.type]++
    }
  })
  
  return stats
}

// 实时更新统计
ws.onMessage(() => {
  const stats = getMessageStats()
  console.log('消息统计:', stats)
})
```

### 聊天应用示例

```vue
<template>
  <div class="chat-app">
    <!-- 聊天消息列表 -->
    <div class="chat-messages">
      <div 
        v-for="message in chatMessages" 
        :key="message.id"
        class="chat-message"
      >
        <strong>{{ message.data.sender }}:</strong>
        {{ message.data.message }}
        <span class="time">{{ formatTime(message.receivedAt) }}</span>
      </div>
    </div>
    
    <!-- 发送消息 -->
    <div class="chat-input">
      <el-input 
        v-model="newMessage" 
        @keyup.enter="sendMessage"
        placeholder="输入消息..."
      />
      <el-button @click="sendMessage">发送</el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { ws } from '@/common/utils/websocket-service'

const chatMessages = ref([])
const newMessage = ref('')
const currentUser = ref('当前用户')

// 聊天消息监听器
const chatListener = (data) => {
  chatMessages.value.push(data)
  
  // 滚动到底部
  nextTick(() => {
    scrollToBottom()
  })
}

// 发送消息
function sendMessage() {
  if (!newMessage.value.trim()) return
  
  ws.chat(currentUser.value, newMessage.value)
  newMessage.value = ''
}

// 格式化时间
function formatTime(date) {
  return new Date(date).toLocaleTimeString()
}

// 滚动到底部
function scrollToBottom() {
  const container = document.querySelector('.chat-messages')
  if (container) {
    container.scrollTop = container.scrollHeight
  }
}

onMounted(() => {
  ws.init()
  
  // 加载历史聊天记录
  chatMessages.value = ws.getLatestByType('chat', 50)
  
  // 监听新的聊天消息
  ws.onChat(chatListener)
})

onUnmounted(() => {
  ws.off('chat', chatListener)
})
</script>
```

### 通知中心示例

```vue
<template>
  <div class="notification-center">
    <el-badge :value="unreadCount" class="notification-badge">
      <el-button @click="showNotifications = !showNotifications">
        <el-icon><Bell /></el-icon>
      </el-button>
    </el-badge>
    
    <el-drawer v-model="showNotifications" title="通知中心">
      <div v-for="notification in notifications" :key="notification.id">
        <el-card class="notification-item">
          <h4>{{ notification.data.title }}</h4>
          <p>{{ notification.data.message }}</p>
          <small>{{ formatTime(notification.receivedAt) }}</small>
        </el-card>
      </div>
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { ws } from '@/common/utils/websocket-service'
import { Bell } from '@element-plus/icons-vue'

const notifications = ref([])
const showNotifications = ref(false)
const readNotifications = ref(new Set())

// 未读通知数量
const unreadCount = computed(() => {
  return notifications.value.filter(n => !readNotifications.value.has(n.id)).length
})

// 通知监听器
const notificationListener = (data) => {
  notifications.value.unshift(data)
  
  // 限制通知数量
  if (notifications.value.length > 100) {
    notifications.value = notifications.value.slice(0, 100)
  }
}

// 标记为已读
function markAsRead(notificationId) {
  readNotifications.value.add(notificationId)
}

// 格式化时间
function formatTime(date) {
  return new Date(date).toLocaleString()
}

onMounted(() => {
  ws.init()
  
  // 加载历史通知
  notifications.value = ws.getLatestByType('notification', 50)
  
  // 监听新通知
  ws.onNotification(notificationListener)
})

onUnmounted(() => {
  ws.off('notification', notificationListener)
})
</script>
```

### 系统监控示例

```javascript
// 系统监控类
class SystemMonitor {
  constructor() {
    this.systemMessages = []
    this.alerts = []
    this.setupListeners()
  }
  
  setupListeners() {
    // 监听系统消息
    ws.onSystem((data) => {
      this.handleSystemMessage(data)
    })
    
    // 监听连接状态
    ws.onConnect(() => {
      this.logEvent('WebSocket连接成功')
    })
    
    ws.onDisconnect(() => {
      this.logEvent('WebSocket连接断开')
      this.createAlert('连接断开', 'warning')
    })
    
    ws.onError((error) => {
      this.logEvent('WebSocket错误: ' + error.message)
      this.createAlert('连接错误', 'error')
    })
  }
  
  handleSystemMessage(data) {
    this.systemMessages.push(data)
    
    // 根据系统消息类型创建告警
    const { action, message, level } = data.data
    
    if (action === 'maintenance') {
      this.createAlert(`系统维护: ${message}`, 'warning')
    } else if (level === 'error') {
      this.createAlert(`系统错误: ${message}`, 'error')
    }
  }
  
  createAlert(message, level) {
    this.alerts.push({
      id: Date.now(),
      message,
      level,
      timestamp: new Date()
    })
  }
  
  logEvent(message) {
    console.log(`[${new Date().toLocaleTimeString()}] ${message}`)
  }
  
  // 获取系统状态报告
  getSystemReport() {
    const recentMessages = ws.getLatestByType('system', 20)
    
    return {
      connectionStatus: ws.isConnected() ? '正常' : '异常',
      recentSystemMessages: recentMessages.length,
      activeAlerts: this.alerts.filter(alert => 
        Date.now() - alert.timestamp.getTime() < 300000 // 5分钟内的告警
      ).length,
      totalMessages: ws.getHistory().length
    }
  }
}

// 使用系统监控
const monitor = new SystemMonitor()

// 定期生成报告
setInterval(() => {
  const report = monitor.getSystemReport()
  console.log('系统状态报告:', report)
}, 60000) // 每分钟一次
```

## 高级功能

### 1. 自定义过滤器

```javascript
// 自定义消息过滤器
function createMessageFilter(conditions) {
  return (message) => {
    // 按发送者过滤
    if (conditions.sender && message.data.sender !== conditions.sender) {
      return false
    }
    
    // 按关键词过滤
    if (conditions.keyword) {
      const content = JSON.stringify(message.data).toLowerCase()
      if (!content.includes(conditions.keyword.toLowerCase())) {
        return false
      }
    }
    
    // 按时间范围过滤
    if (conditions.timeRange) {
      const messageTime = message.timestamp
      if (messageTime < conditions.timeRange.start || 
          messageTime > conditions.timeRange.end) {
        return false
      }
    }
    
    return true
  }
}

// 使用自定义过滤器
const filter = createMessageFilter({
  sender: '管理员',
  keyword: '重要',
  timeRange: {
    start: Date.now() - 86400000, // 24小时前
    end: Date.now()
  }
})

const filteredMessages = ws.getHistory().filter(filter)
```

### 2. 消息统计分析

```javascript
// 消息统计分析器
class MessageAnalyzer {
  static analyze(messages) {
    const analysis = {
      totalCount: messages.length,
      typeDistribution: {},
      hourlyDistribution: {},
      senderDistribution: {},
      averageMessageLength: 0,
      timeRange: {
        earliest: null,
        latest: null
      }
    }
    
    let totalLength = 0
    
    messages.forEach(msg => {
      // 类型分布
      analysis.typeDistribution[msg.type] = 
        (analysis.typeDistribution[msg.type] || 0) + 1
      
      // 小时分布
      const hour = new Date(msg.timestamp).getHours()
      analysis.hourlyDistribution[hour] = 
        (analysis.hourlyDistribution[hour] || 0) + 1
      
      // 发送者分布
      if (msg.data.sender) {
        analysis.senderDistribution[msg.data.sender] = 
          (analysis.senderDistribution[msg.data.sender] || 0) + 1
      }
      
      // 消息长度
      const msgLength = JSON.stringify(msg.data).length
      totalLength += msgLength
      
      // 时间范围
      if (!analysis.timeRange.earliest || msg.timestamp < analysis.timeRange.earliest) {
        analysis.timeRange.earliest = msg.timestamp
      }
      if (!analysis.timeRange.latest || msg.timestamp > analysis.timeRange.latest) {
        analysis.timeRange.latest = msg.timestamp
      }
    })
    
    analysis.averageMessageLength = messages.length > 0 ? 
      Math.round(totalLength / messages.length) : 0
    
    return analysis
  }
  
  static generateReport(analysis) {
    return `
消息统计报告
==============
总消息数: ${analysis.totalCount}
平均消息长度: ${analysis.averageMessageLength} 字符
时间范围: ${new Date(analysis.timeRange.earliest).toLocaleString()} - ${new Date(analysis.timeRange.latest).toLocaleString()}

类型分布:
${Object.entries(analysis.typeDistribution).map(([type, count]) => `  ${type}: ${count}`).join('\n')}

最活跃时段:
${Object.entries(analysis.hourlyDistribution)
  .sort(([,a], [,b]) => b - a)
  .slice(0, 5)
  .map(([hour, count]) => `  ${hour}:00 - ${count} 条消息`)
  .join('\n')}
    `.trim()
  }
}

// 使用统计分析
const messages = ws.getHistory()
const analysis = MessageAnalyzer.analyze(messages)
const report = MessageAnalyzer.generateReport(analysis)
console.log(report)
```

## 最佳实践

### 1. 内存管理
```javascript
// 合理设置历史记录大小
ws.setMaxHistorySize(500) // 根据应用需求调整

// 定期清理旧消息
setInterval(() => {
  const oldMessages = ws.getHistory({
    endTime: Date.now() - 86400000 // 1天前
  })
  
  if (oldMessages.length > 100) {
    ws.clearHistory()
    console.log('清理了旧的消息记录')
  }
}, 3600000) // 每小时检查一次
```

### 2. 错误处理
```javascript
// 设置错误监听
ws.onError((error) => {
  console.error('WebSocket错误:', error)
  
  // 发送错误报告到监控系统
  if (typeof reportError === 'function') {
    reportError('WebSocket Error', error)
  }
})

// 监听器中的错误处理
ws.onMessage((data) => {
  try {
    // 处理消息的业务逻辑
    processMessage(data)
  } catch (error) {
    console.error('处理消息时出错:', error)
    // 不要让错误影响其他监听器
  }
})
```

### 3. 性能优化
```javascript
// 使用防抖减少频繁更新
import { debounce } from 'lodash'

const updateUI = debounce((messages) => {
  // 更新UI的逻辑
}, 100)

ws.onMessage((data) => {
  const latestMessages = ws.getLatest(20)
  updateUI(latestMessages)
})
```

## 注意事项

1. **监听器清理**: 组件卸载时记得移除监听器，避免内存泄漏
2. **消息频率**: 高频消息可能影响性能，考虑使用防抖或节流
3. **历史记录大小**: 根据应用需求合理设置，避免占用过多内存
4. **错误处理**: 监听器中的错误不应影响其他功能
5. **双重处理**: 外部监听和内置处理器会同时工作，避免重复处理

这种设计既保持了原有的便捷性，又提供了强大的扩展能力，适合各种复杂的实时应用场景。

