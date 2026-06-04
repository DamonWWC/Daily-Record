// WebSocket服务类

import { wsClient, WS_MESSAGE_TYPE } from './websocket'
import { ElMessage, ElNotification } from 'element-plus'

// 消息处理器基类
class MessageHandler {
  constructor(type) {
    this.type = type
  }

  handle(data) {
    // 子类需要重写此方法
    console.log(`Handling ${this.type} message:`, data)
  }
}

// 通知消息处理器
class NotificationHandler extends MessageHandler {
  constructor() {
    super('notification')
  }

  handle(data) {
    const { title, message, type = 'info', duration = 4500 } = data

    ElNotification({
      title: title || '系统通知',
      message: message || '',
      type: type, // success, warning, info, error
      duration: duration,
      position: 'top-right'
    })
  }
}

// 聊天消息处理器
class ChatHandler extends MessageHandler {
  constructor() {
    super('chat')
  }

  handle(data) {
    const { sender, message } = data

    ElMessage({
      message: `${sender}: ${message}`,
      type: 'info',
      duration: 3000
    })
  }
}

// 系统消息处理器
class SystemHandler extends MessageHandler {
  constructor() {
    super('system')
  }

  handle(data) {
    const { action, message, level = 'info' } = data

    switch (action) {
      case 'maintenance':
        ElMessage({
          message: `系统维护: ${message}`,
          type: 'warning',
          duration: 0,
          showClose: true
        })
        break
      case 'update':
        ElMessage({
          message: `系统更新: ${message}`,
          type: 'success',
          duration: 5000
        })
        break
      default:
        ElMessage({
          message: message || '系统消息',
          type: level,
          duration: 3000
        })
    }
  }
}

// WebSocket服务类
export class WebSocketService {
  constructor() {
    this.client = wsClient
    this.handlers = new Map()
    this.messageQueue = []
    this.isInitialized = false

    // 外部事件监听器
    this.eventListeners = new Map()

    // 消息历史记录
    this.messageHistory = []
    this.maxHistorySize = 100

    // 注册默认消息处理器
    this.registerDefaultHandlers()
  }

  // 初始化服务
  init() {
    if (this.isInitialized) {
      return
    }

    // 注册事件监听器
    this.client.on(WS_MESSAGE_TYPE.CONNECT, this.handleConnect.bind(this))
    this.client.on(WS_MESSAGE_TYPE.DISCONNECT, this.handleDisconnect.bind(this))
    this.client.on(WS_MESSAGE_TYPE.MESSAGE, this.handleMessage.bind(this))
    this.client.on(WS_MESSAGE_TYPE.ERROR, this.handleError.bind(this))

    // 连接WebSocket
    this.client.connect()

    this.isInitialized = true
  }

  // 销毁服务
  destroy() {
    if (!this.isInitialized) {
      return
    }

    // 移除事件监听器
    this.client.off(WS_MESSAGE_TYPE.CONNECT, this.handleConnect)
    this.client.off(WS_MESSAGE_TYPE.DISCONNECT, this.handleDisconnect)
    this.client.off(WS_MESSAGE_TYPE.MESSAGE, this.handleMessage)
    this.client.off(WS_MESSAGE_TYPE.ERROR, this.handleError)

    // 断开连接
    this.client.disconnect()

    this.isInitialized = false
  }

  // 注册默认消息处理器
  registerDefaultHandlers() {
    this.registerHandler(new NotificationHandler())
    this.registerHandler(new ChatHandler())
    this.registerHandler(new SystemHandler())
  }

  // 注册消息处理器
  registerHandler(handler) {
    this.handlers.set(handler.type, handler)
  }

  // 移除消息处理器
  removeHandler(type) {
    this.handlers.delete(type)
  }

  // 处理连接事件
  handleConnect() {
    console.log('WebSocket service connected')

    // 发送队列中的消息
    this.flushMessageQueue()
  }

  // 处理断开连接事件
  handleDisconnect() {
    console.log('WebSocket service disconnected')
  }

    // 处理消息事件
  handleMessage(data) {
    const { type } = data

    // 添加到消息历史记录
    this.addToHistory(data)

    // 触发外部事件监听器
    this.triggerEventListeners('message', data)
    this.triggerEventListeners(type, data)

    // 查找对应的处理器
    const handler = this.handlers.get(type)
    if (handler) {
      handler.handle(data)
    } else {
      console.log(`No handler found for message type: ${type}`)
    }
  }

  // 处理错误事件
  handleError(error) {
    console.error('WebSocket service error:', error)
  }

  // 发送消息
  send(type, data = {}) {
    const message = {
      type,
      data,
      timestamp: Date.now()
    }

    if (this.client.isConnected()) {
      return this.client.send(message)
    } else {
      // 如果未连接，将消息加入队列
      this.messageQueue.push(message)
      return false
    }
  }

  // 发送通知
  sendNotification(title, message, type = 'info') {
    return this.send('notification', {
      title,
      message,
      type
    })
  }

  // 发送聊天消息
  sendChatMessage(sender, message) {
    return this.send('chat', {
      sender,
      message,
      timestamp: Date.now()
    })
  }

  // 发送系统消息
  sendSystemMessage(action, message, level = 'info') {
    return this.send('system', {
      action,
      message,
      level
    })
  }

  // 发送心跳
  sendHeartbeat() {
    return this.send('heartbeat', {
      timestamp: Date.now()
    })
  }

  // 刷新消息队列
  flushMessageQueue() {
    while (this.messageQueue.length > 0) {
      const message = this.messageQueue.shift()
      this.client.send(message)
    }
  }

  // 获取连接状态
  getStatus() {
    return this.client.getStatus()
  }

  // 检查是否已连接
  isConnected() {
    return this.client.isConnected()
  }

  // 获取重连次数
  getReconnectAttempts() {
    return this.client.getReconnectAttempts()
  }

  // 手动重连
  reconnect() {
    this.client.disconnect()
    setTimeout(() => {
      this.client.connect()
    }, 1000)
  }

  // ========== 外部事件监听方法 ==========

  // 添加事件监听器
  addEventListener(eventType, callback) {
    if (!this.eventListeners.has(eventType)) {
      this.eventListeners.set(eventType, [])
    }
    this.eventListeners.get(eventType).push(callback)
  }

  // 移除事件监听器
  removeEventListener(eventType, callback) {
    if (this.eventListeners.has(eventType)) {
      const listeners = this.eventListeners.get(eventType)
      const index = listeners.indexOf(callback)
      if (index > -1) {
        listeners.splice(index, 1)
      }
    }
  }

  // 移除所有指定类型的事件监听器
  removeAllEventListeners(eventType) {
    this.eventListeners.delete(eventType)
  }

  // 触发事件监听器
  triggerEventListeners(eventType, data) {
    if (this.eventListeners.has(eventType)) {
      const listeners = this.eventListeners.get(eventType)
      listeners.forEach(callback => {
        try {
          callback(data, eventType)
        } catch (error) {
          console.error(`Error in event listener for ${eventType}:`, error)
        }
      })
    }
  }

  // ========== 消息历史管理方法 ==========

  // 添加消息到历史记录
  addToHistory(data) {
    const messageWithMeta = {
      ...data,
      id: Date.now() + Math.random(),
      receivedAt: new Date(),
      timestamp: data.timestamp || Date.now()
    }

    this.messageHistory.push(messageWithMeta)

    // 限制历史记录大小
    if (this.messageHistory.length > this.maxHistorySize) {
      this.messageHistory = this.messageHistory.slice(-this.maxHistorySize)
    }
  }

  // 获取消息历史记录
  getMessageHistory(filter = {}) {
    let history = [...this.messageHistory]

    // 按类型过滤
    if (filter.type) {
      history = history.filter(msg => msg.type === filter.type)
    }

    // 按时间范围过滤
    if (filter.startTime) {
      history = history.filter(msg => msg.timestamp >= filter.startTime)
    }

    if (filter.endTime) {
      history = history.filter(msg => msg.timestamp <= filter.endTime)
    }

    // 按数量限制
    if (filter.limit) {
      history = history.slice(-filter.limit)
    }

    return history
  }

  // 获取最新消息
  getLatestMessages(count = 10) {
    return this.messageHistory.slice(-count)
  }

  // 获取指定类型的最新消息
  getLatestMessagesByType(type, count = 10) {
    return this.messageHistory
      .filter(msg => msg.type === type)
      .slice(-count)
  }

  // 清空消息历史
  clearMessageHistory() {
    this.messageHistory = []
  }

  // 设置历史记录最大大小
  setMaxHistorySize(size) {
    this.maxHistorySize = size
    if (this.messageHistory.length > size) {
      this.messageHistory = this.messageHistory.slice(-size)
    }
  }

  // ========== 便捷的监听方法 ==========

  // 监听所有消息
  onMessage(callback) {
    this.addEventListener('message', callback)
  }

  // 监听通知消息
  onNotification(callback) {
    this.addEventListener('notification', callback)
  }

  // 监听聊天消息
  onChat(callback) {
    this.addEventListener('chat', callback)
  }

  // 监听系统消息
  onSystem(callback) {
    this.addEventListener('system', callback)
  }

  // 监听连接事件
  onConnect(callback) {
    this.client.on('connect', callback)
  }

  // 监听断开事件
  onDisconnect(callback) {
    this.client.on('disconnect', callback)
  }

  // 监听错误事件
  onError(callback) {
    this.client.on('error', callback)
  }
}

// 创建WebSocket服务实例
export const wsService = new WebSocketService()

// 导出便捷方法
export const ws = {
  // ========== 连接管理 ==========
  init: () => wsService.init(),
  destroy: () => wsService.destroy(),
  reconnect: () => wsService.reconnect(),

  // ========== 状态查询 ==========
  getStatus: () => wsService.getStatus(),
  isConnected: () => wsService.isConnected(),

  // ========== 消息发送 ==========
  send: (type, data) => wsService.send(type, data),
  notify: (title, message, type) => wsService.sendNotification(title, message, type),
  chat: (sender, message) => wsService.sendChatMessage(sender, message),
  system: (action, message, level) => wsService.sendSystemMessage(action, message, level),

  // ========== 事件监听 ==========
  on: (eventType, callback) => wsService.addEventListener(eventType, callback),
  off: (eventType, callback) => wsService.removeEventListener(eventType, callback),
  onMessage: (callback) => wsService.onMessage(callback),
  onNotification: (callback) => wsService.onNotification(callback),
  onChat: (callback) => wsService.onChat(callback),
  onSystem: (callback) => wsService.onSystem(callback),
  onConnect: (callback) => wsService.onConnect(callback),
  onDisconnect: (callback) => wsService.onDisconnect(callback),
  onError: (callback) => wsService.onError(callback),

  // ========== 消息历史 ==========
  getHistory: (filter) => wsService.getMessageHistory(filter),
  getLatest: (count) => wsService.getLatestMessages(count),
  getLatestByType: (type, count) => wsService.getLatestMessagesByType(type, count),
  clearHistory: () => wsService.clearMessageHistory(),

  // ========== 高级功能 ==========
  setMaxHistorySize: (size) => wsService.setMaxHistorySize(size),
  removeAllListeners: (eventType) => wsService.removeAllEventListeners(eventType)
}

export default WebSocketService
