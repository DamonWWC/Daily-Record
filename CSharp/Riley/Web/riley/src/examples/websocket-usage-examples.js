// WebSocket 外部监听使用示例

import { ws, wsService } from '@/common/utils/websocket-service'

// ========== 基础示例 ==========

// 示例1: 基本消息监听
export function basicMessageListening() {
  // 初始化连接
  ws.init()

  // 监听所有消息
  ws.onMessage((data, eventType) => {
    console.log(`收到 ${eventType} 类型消息:`, data)
  })

  // 监听特定类型消息
  ws.onNotification((data) => {
    console.log('通知消息:', data.data.title, data.data.message)
  })

  ws.onChat((data) => {
    console.log('聊天消息:', data.data.sender, '说:', data.data.message)
  })

  ws.onSystem((data) => {
    console.log('系统消息:', data.data.action, '-', data.data.message)
  })
}

// 示例2: 消息历史查询
export function messageHistoryExamples() {
  // 获取所有历史消息
  const allMessages = ws.getHistory()
  console.log('所有历史消息:', allMessages)

  // 获取最近10条消息
  const recent = ws.getLatest(10)
  console.log('最近10条消息:', recent)

  // 获取通知类型的最近5条消息
  const recentNotifications = ws.getLatestByType('notification', 5)
  console.log('最近5条通知:', recentNotifications)

  // 按条件过滤消息
  const filtered = ws.getHistory({
    type: 'chat',
    startTime: Date.now() - 3600000, // 最近1小时
    limit: 20
  })
  console.log('过滤后的消息:', filtered)
}

// ========== Vue组件示例 ==========

// 示例3: Vue组件中的实时消息显示
export const RealtimeMessageComponent = {
  template: `
    <div>
      <h3>实时消息 ({{ messages.length }})</h3>
      <div v-for="msg in messages" :key="msg.id" class="message">
        <strong>{{ msg.type }}:</strong>
        {{ JSON.stringify(msg.data) }}
        <small>{{ formatTime(msg.receivedAt) }}</small>
      </div>
    </div>
  `,

  setup() {
    const messages = ref([])

    // 消息监听器
    const messageListener = (data) => {
      messages.value.unshift({
        ...data,
        id: Date.now() + Math.random()
      })

      // 限制显示数量
      if (messages.value.length > 50) {
        messages.value = messages.value.slice(0, 50)
      }
    }

    // 格式化时间
    const formatTime = (date) => {
      return new Date(date).toLocaleTimeString()
    }

    onMounted(() => {
      ws.init()
      ws.onMessage(messageListener)
    })

    onUnmounted(() => {
      ws.off('message', messageListener)
    })

    return {
      messages,
      formatTime
    }
  }
}

// ========== 实际应用示例 ==========

// 示例4: 聊天应用
export class ChatApplication {
  constructor() {
    this.messages = []
    this.currentUser = '用户'
    this.setupListeners()
  }

  setupListeners() {
    ws.init()

    // 监听聊天消息
    ws.onChat((data) => {
      this.messages.push({
        id: Date.now(),
        sender: data.data.sender,
        message: data.data.message,
        timestamp: data.receivedAt || new Date()
      })

      // 通知UI更新
      this.onMessageReceived?.(this.messages)
    })
  }

  sendMessage(message) {
    if (!message.trim()) return

    ws.chat(this.currentUser, message)
  }

  getRecentMessages(count = 20) {
    return ws.getLatestByType('chat', count)
  }

  // 设置消息接收回调
  onMessageReceived(callback) {
    this.onMessageReceived = callback
  }
}

// 示例5: 通知中心
export class NotificationCenter {
  constructor() {
    this.notifications = []
    this.unreadCount = 0
    this.setupListeners()
  }

  setupListeners() {
    ws.init()

    // 监听通知消息
    ws.onNotification((data) => {
      const notification = {
        id: Date.now(),
        title: data.data.title,
        message: data.data.message,
        type: data.data.type,
        isRead: false,
        timestamp: data.receivedAt || new Date()
      }

      this.notifications.unshift(notification)
      this.unreadCount++

      // 通知UI更新
      this.onNotificationReceived?.(notification)
    })
  }

  markAsRead(notificationId) {
    const notification = this.notifications.find(n => n.id === notificationId)
    if (notification && !notification.isRead) {
      notification.isRead = true
      this.unreadCount--
    }
  }

  markAllAsRead() {
    this.notifications.forEach(n => {
      if (!n.isRead) {
        n.isRead = true
      }
    })
    this.unreadCount = 0
  }

  getUnreadNotifications() {
    return this.notifications.filter(n => !n.isRead)
  }

  // 设置通知接收回调
  onNotificationReceived(callback) {
    this.onNotificationReceived = callback
  }
}

// 示例6: 系统监控
export class SystemMonitor {
  constructor() {
    this.systemEvents = []
    this.alerts = []
    this.connectionStatus = 'disconnected'
    this.setupListeners()
  }

  setupListeners() {
    ws.init()

    // 监听系统消息
    ws.onSystem((data) => {
      const event = {
        id: Date.now(),
        action: data.data.action,
        message: data.data.message,
        level: data.data.level,
        timestamp: data.receivedAt || new Date()
      }

      this.systemEvents.push(event)

      // 根据级别创建告警
      if (data.data.level === 'error' || data.data.action === 'maintenance') {
        this.createAlert(event)
      }

      this.onSystemEvent?.(event)
    })

    // 监听连接状态
    ws.onConnect(() => {
      this.connectionStatus = 'connected'
      this.logEvent('WebSocket连接成功')
    })

    ws.onDisconnect(() => {
      this.connectionStatus = 'disconnected'
      this.logEvent('WebSocket连接断开')
      this.createAlert({
        message: 'WebSocket连接断开',
        level: 'warning',
        timestamp: new Date()
      })
    })

    ws.onError((error) => {
      this.connectionStatus = 'error'
      this.logEvent('WebSocket错误: ' + error.message)
      this.createAlert({
        message: 'WebSocket连接错误: ' + error.message,
        level: 'error',
        timestamp: new Date()
      })
    })
  }

  createAlert(event) {
    this.alerts.push({
      id: Date.now(),
      message: event.message,
      level: event.level,
      timestamp: event.timestamp,
      isResolved: false
    })

    this.onAlert?.(this.alerts[this.alerts.length - 1])
  }

  resolveAlert(alertId) {
    const alert = this.alerts.find(a => a.id === alertId)
    if (alert) {
      alert.isResolved = true
    }
  }

  getSystemStatus() {
    return {
      connectionStatus: this.connectionStatus,
      totalEvents: this.systemEvents.length,
      activeAlerts: this.alerts.filter(a => !a.isResolved).length,
      recentEvents: this.systemEvents.slice(-10)
    }
  }

  logEvent(message) {
    console.log(`[${new Date().toLocaleTimeString()}] ${message}`)
  }

  // 设置回调函数
  onSystemEvent(callback) {
    this.onSystemEvent = callback
  }

  onAlert(callback) {
    this.onAlert = callback
  }
}

// ========== 高级示例 ==========

// 示例7: 消息统计分析
export class MessageAnalyzer {
  static analyze(timeRange = 24 * 60 * 60 * 1000) { // 默认24小时
    const startTime = Date.now() - timeRange
    const messages = ws.getHistory({
      startTime: startTime
    })

    const analysis = {
      totalCount: messages.length,
      timeRange: {
        start: startTime,
        end: Date.now()
      },
      typeDistribution: {},
      hourlyDistribution: {},
      averageInterval: 0
    }

    // 统计类型分布
    messages.forEach(msg => {
      analysis.typeDistribution[msg.type] =
        (analysis.typeDistribution[msg.type] || 0) + 1

      // 统计小时分布
      const hour = new Date(msg.timestamp).getHours()
      analysis.hourlyDistribution[hour] =
        (analysis.hourlyDistribution[hour] || 0) + 1
    })

    // 计算平均间隔
    if (messages.length > 1) {
      const intervals = []
      for (let i = 1; i < messages.length; i++) {
        intervals.push(messages[i].timestamp - messages[i-1].timestamp)
      }
      analysis.averageInterval = intervals.reduce((a, b) => a + b, 0) / intervals.length
    }

    return analysis
  }

  static generateReport(analysis) {
    const report = {
      summary: `在${Math.round((analysis.timeRange.end - analysis.timeRange.start) / 3600000)}小时内收到${analysis.totalCount}条消息`,
      typeDistribution: analysis.typeDistribution,
      mostActiveHour: Object.entries(analysis.hourlyDistribution)
        .sort(([,a], [,b]) => b - a)[0]?.[0] || '无数据',
      averageInterval: Math.round(analysis.averageInterval / 1000) + '秒'
    }

    return report
  }
}

// 示例8: 自定义消息处理器
export class CustomMessageHandler {
  constructor() {
    this.customMessages = []
    this.setupCustomListener()
  }

  setupCustomListener() {
    // 使用通用监听器处理自定义逻辑
    ws.on('custom', (data) => {
      this.handleCustomMessage(data)
    })

    // 也可以监听所有消息然后过滤
    ws.onMessage((data) => {
      if (data.type === 'custom') {
        this.handleCustomMessage(data)
      }
    })
  }

  handleCustomMessage(data) {
    this.customMessages.push({
      id: Date.now(),
      ...data,
      processedAt: new Date()
    })

    // 自定义处理逻辑
    console.log('处理自定义消息:', data)

    // 可以触发其他业务逻辑
    this.onCustomMessage?.(data)
  }

  sendCustomMessage(customData) {
    ws.send('custom', customData)
  }

  onCustomMessage(callback) {
    this.onCustomMessage = callback
  }
}

// ========== 使用示例 ==========

// 完整使用示例
export function completeExample() {
  console.log('=== WebSocket 外部监听完整示例 ===')

  // 1. 初始化并设置基础监听
  ws.init()

  // 2. 设置消息监听
  ws.onMessage((data, eventType) => {
    console.log(`[${eventType}] 收到消息:`, data)
  })

  // 3. 创建应用实例
  const chatApp = new ChatApplication()
  const notificationCenter = new NotificationCenter()
  const systemMonitor = new SystemMonitor()

  // 4. 设置回调
  chatApp.onMessageReceived((messages) => {
    console.log('聊天消息更新:', messages.length, '条消息')
  })

  notificationCenter.onNotificationReceived((notification) => {
    console.log('新通知:', notification.title)
  })

  systemMonitor.onSystemEvent((event) => {
    console.log('系统事件:', event.action, event.message)
  })

  // 5. 发送测试消息
  setTimeout(() => {
    ws.notify('测试通知', '这是一条测试通知消息', 'info')
    ws.chat('测试用户', '这是一条测试聊天消息')
    ws.system('update', '这是一条测试系统消息', 'info')
  }, 2000)

  // 6. 定期分析消息
  setInterval(() => {
    const analysis = MessageAnalyzer.analyze()
    const report = MessageAnalyzer.generateReport(analysis)
    console.log('消息分析报告:', report)
  }, 60000) // 每分钟分析一次

  console.log('完整示例设置完成')
}

// 导出所有示例
export default {
  basicMessageListening,
  messageHistoryExamples,
  RealtimeMessageComponent,
  ChatApplication,
  NotificationCenter,
  SystemMonitor,
  MessageAnalyzer,
  CustomMessageHandler,
  completeExample
}

