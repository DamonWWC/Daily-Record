// WebSocket客户端工具类

import { ElMessage } from 'element-plus'

// WebSocket状态枚举
export const WS_STATUS = {
  CONNECTING: 0, // 连接中
  OPEN: 1,       // 已连接
  CLOSING: 2,    // 关闭中
  CLOSED: 3      // 已关闭
}

// WebSocket消息类型枚举
export const WS_MESSAGE_TYPE = {
  CONNECT: 'connect',
  DISCONNECT: 'disconnect',
  MESSAGE: 'message',
  ERROR: 'error',
  HEARTBEAT: 'heartbeat',
  NOTIFICATION: 'notification',
  CHAT: 'chat',
  SYSTEM: 'system'
}

// WebSocket客户端类
export class WebSocketClient {
  constructor(options = {}) {
    this.options = {
      url: '/ws',
      protocols: [],
      reconnectInterval: 3000,
      maxReconnectAttempts: 5,
      heartbeatInterval: 30000,
      heartbeatMessage: 'ping',
      autoReconnect: true,
      debug: false,
      ...options
    }

    this.ws = null
    this.status = WS_STATUS.CLOSED
    this.reconnectAttempts = 0
    this.heartbeatTimer = null
    this.reconnectTimer = null
    this.messageHandlers = new Map()
    this.connectionHandlers = new Map()
    this.isManualClose = false

    // 绑定方法
    this.connect = this.connect.bind(this)
    this.disconnect = this.disconnect.bind(this)
    this.send = this.send.bind(this)
    this.on = this.on.bind(this)
    this.off = this.off.bind(this)
  }

  // 获取WebSocket URL
  getWebSocketURL() {
    const { url } = this.options

    // 如果是相对路径，转换为完整URL
    if (url.startsWith('/')) {
      const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
      const host = window.location.host
      return `${protocol}//${host}${url}`
    }

    return url
  }

  // 连接WebSocket
  connect() {
    if (this.status === WS_STATUS.CONNECTING || this.status === WS_STATUS.OPEN) {
      this.log('WebSocket already connected or connecting')
      return
    }

    try {
      const wsURL = this.getWebSocketURL()
      this.log(`Connecting to WebSocket: ${wsURL}`)

      this.ws = new WebSocket(wsURL, this.options.protocols)
      this.status = WS_STATUS.CONNECTING
      this.isManualClose = false

      this.ws.onopen = this.handleOpen.bind(this)
      this.ws.onmessage = this.handleMessage.bind(this)
      this.ws.onerror = this.handleError.bind(this)
      this.ws.onclose = this.handleClose.bind(this)

    } catch (error) {
      this.log('WebSocket connection error:', error)
      this.handleError(error)
    }
  }

  // 断开连接
  disconnect() {
    this.isManualClose = true
    this.clearTimers()

    if (this.ws && this.status !== WS_STATUS.CLOSED) {
      this.log('Disconnecting WebSocket')
      this.ws.close(1000, 'Manual disconnect')
    }
  }

  // 发送消息
  send(data) {
    if (this.status !== WS_STATUS.OPEN) {
      this.log('WebSocket is not connected')
      return false
    }

    try {
      const message = typeof data === 'string' ? data : JSON.stringify(data)
      this.ws.send(message)
      this.log('Sent message:', message)
      return true
    } catch (error) {
      this.log('Send message error:', error)
      return false
    }
  }

  // 发送心跳
  sendHeartbeat() {
    if (this.status === WS_STATUS.OPEN) {
      this.send(this.options.heartbeatMessage)
    }
  }

  // 处理连接打开
  handleOpen(event) {
    this.log('WebSocket connected')
    this.status = WS_STATUS.OPEN
    this.reconnectAttempts = 0

    // 启动心跳
    this.startHeartbeat()

    // 触发连接事件
    this.triggerEvent(WS_MESSAGE_TYPE.CONNECT, event)

    // 显示连接成功消息
    ElMessage.success('WebSocket连接成功')
  }

  // 处理消息接收
  handleMessage(event) {
    try {
      const data = JSON.parse(event.data)
      this.log('Received message:', data)

      // 处理心跳响应
      if (data.type === 'pong') {
        this.log('Received heartbeat response')
        return
      }

      // 触发消息事件
      this.triggerEvent(WS_MESSAGE_TYPE.MESSAGE, data)

      // 根据消息类型触发特定事件
      if (data.type) {
        this.triggerEvent(data.type, data)
      }

    } catch (error) {
      // 如果不是JSON格式，作为普通文本处理
      this.log('Received text message:', event.data)
      this.triggerEvent(WS_MESSAGE_TYPE.MESSAGE, {
        type: 'text',
        data: event.data,
        timestamp: Date.now()
      })
    }
  }

  // 处理错误
  handleError(error) {
    this.log('WebSocket error:', error)
    this.status = WS_STATUS.CLOSED

    // 触发错误事件
    this.triggerEvent(WS_MESSAGE_TYPE.ERROR, error)

    // 显示错误消息
    ElMessage.error('WebSocket连接错误')
  }

  // 处理连接关闭
  handleClose(event) {
    this.log('WebSocket closed:', event.code, event.reason)
    this.status = WS_STATUS.CLOSED
    this.clearTimers()

    // 触发断开连接事件
    this.triggerEvent(WS_MESSAGE_TYPE.DISCONNECT, event)

    // 如果不是手动关闭且启用了自动重连，则尝试重连
    if (!this.isManualClose && this.options.autoReconnect) {
      this.scheduleReconnect()
    }

    // 显示断开连接消息
    if (!this.isManualClose) {
      ElMessage.warning('WebSocket连接已断开，正在尝试重连...')
    }
  }

  // 安排重连
  scheduleReconnect() {
    if (this.reconnectAttempts >= this.options.maxReconnectAttempts) {
      this.log('Max reconnection attempts reached')
      ElMessage.error('WebSocket重连失败，请刷新页面重试')
      return
    }

    this.reconnectAttempts++
    const delay = this.options.reconnectInterval * this.reconnectAttempts

    this.log(`Scheduling reconnect attempt ${this.reconnectAttempts} in ${delay}ms`)

    this.reconnectTimer = setTimeout(() => {
      this.log(`Attempting to reconnect (${this.reconnectAttempts}/${this.options.maxReconnectAttempts})`)
      this.connect()
    }, delay)
  }

  // 启动心跳
  startHeartbeat() {
    if (this.options.heartbeatInterval > 0) {
      this.heartbeatTimer = setInterval(() => {
        this.sendHeartbeat()
      }, this.options.heartbeatInterval)
    }
  }

  // 清除定时器
  clearTimers() {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer)
      this.heartbeatTimer = null
    }

    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer)
      this.reconnectTimer = null
    }
  }

  // 注册事件处理器
  on(eventType, handler) {
    if (!this.messageHandlers.has(eventType)) {
      this.messageHandlers.set(eventType, [])
    }
    this.messageHandlers.get(eventType).push(handler)
  }

  // 移除事件处理器
  off(eventType, handler) {
    if (this.messageHandlers.has(eventType)) {
      const handlers = this.messageHandlers.get(eventType)
      const index = handlers.indexOf(handler)
      if (index > -1) {
        handlers.splice(index, 1)
      }
    }
  }

  // 触发事件
  triggerEvent(eventType, data) {
    if (this.messageHandlers.has(eventType)) {
      const handlers = this.messageHandlers.get(eventType)
      handlers.forEach(handler => {
        try {
          handler(data)
        } catch (error) {
          this.log('Event handler error:', error)
        }
      })
    }
  }

  // 日志输出
  log(...args) {
    if (this.options.debug) {
      console.log('[WebSocket]', ...args)
    }
  }

  // 获取连接状态
  getStatus() {
    return this.status
  }

  // 检查是否已连接
  isConnected() {
    return this.status === WS_STATUS.OPEN
  }

  // 获取重连次数
  getReconnectAttempts() {
    return this.reconnectAttempts
  }
}

// 创建WebSocket客户端实例
export function createWebSocketClient(options = {}) {
  return new WebSocketClient(options)
}

// 默认WebSocket客户端实例
export const wsClient = createWebSocketClient({
  url: '/ws',
  debug: import.meta.env.DEV,
  autoReconnect: true,
  heartbeatInterval: 30000
})

export default WebSocketClient
