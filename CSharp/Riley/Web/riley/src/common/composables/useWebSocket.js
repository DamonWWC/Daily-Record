// WebSocket组合式函数

import { ref, onMounted, onUnmounted, computed } from 'vue'
import { wsService, ws, WS_STATUS } from '@/common/utils/websocket-service'

// WebSocket状态映射
const STATUS_MAP = {
  [WS_STATUS.CONNECTING]: 'connecting',
  [WS_STATUS.OPEN]: 'connected',
  [WS_STATUS.CLOSING]: 'closing',
  [WS_STATUS.CLOSED]: 'disconnected'
}

export function useWebSocket(options = {}) {
  // 响应式状态
  const isConnected = ref(false)
  const status = ref('disconnected')
  const reconnectAttempts = ref(0)
  const messages = ref([])
  const error = ref(null)

  // 计算属性
  const statusText = computed(() => {
    return STATUS_MAP[status.value] || 'unknown'
  })

  const canSend = computed(() => {
    return isConnected.value && !error.value
  })

  // 更新状态
  function updateStatus() {
    const wsStatus = wsService.getStatus()
    status.value = wsStatus
    isConnected.value = wsService.isConnected()
    reconnectAttempts.value = wsService.getReconnectAttempts()
  }

  // 添加消息到列表
  function addMessage(message) {
    messages.value.push({
      ...message,
      id: Date.now() + Math.random(),
      timestamp: message.timestamp || Date.now()
    })

    // 限制消息数量
    if (messages.value.length > 100) {
      messages.value = messages.value.slice(-100)
    }
  }

  // 清空消息
  function clearMessages() {
    messages.value = []
  }

  // 发送消息
  function sendMessage(type, data) {
    if (!canSend.value) {
      console.warn('WebSocket is not connected')
      return false
    }

    const success = wsService.send(type, data)
    if (success) {
      addMessage({
        type,
        data,
        direction: 'out',
        timestamp: Date.now()
      })
    }
    return success
  }

  // 发送通知
  function sendNotification(title, message, type = 'info') {
    return sendMessage('notification', { title, message, type })
  }

  // 发送聊天消息
  function sendChatMessage(sender, message) {
    return sendMessage('chat', { sender, message })
  }

  // 发送系统消息
  function sendSystemMessage(action, message, level = 'info') {
    return sendMessage('system', { action, message, level })
  }

  // 手动连接
  function connect() {
    wsService.init()
    updateStatus()
  }

  // 手动断开
  function disconnect() {
    wsService.destroy()
    updateStatus()
  }

  // 手动重连
  function reconnect() {
    wsService.reconnect()
    updateStatus()
  }

  // 事件处理器
  function handleConnect(event) {
    updateStatus()
    error.value = null
    addMessage({
      type: 'system',
      data: { message: 'WebSocket连接成功' },
      direction: 'in',
      timestamp: Date.now()
    })
  }

  function handleDisconnect(event) {
    updateStatus()
    addMessage({
      type: 'system',
      data: { message: 'WebSocket连接断开' },
      direction: 'in',
      timestamp: Date.now()
    })
  }

  function handleMessage(data) {
    addMessage({
      ...data,
      direction: 'in',
      timestamp: data.timestamp || Date.now()
    })
  }

  function handleError(err) {
    updateStatus()
    error.value = err
    addMessage({
      type: 'error',
      data: { message: 'WebSocket连接错误', error: err },
      direction: 'in',
      timestamp: Date.now()
    })
  }

  // 监听WebSocket事件
  function setupEventListeners() {
    wsService.client.on('connect', handleConnect)
    wsService.client.on('disconnect', handleDisconnect)
    wsService.client.on('message', handleMessage)
    wsService.client.on('error', handleError)
  }

  // 移除事件监听
  function removeEventListeners() {
    wsService.client.off('connect', handleConnect)
    wsService.client.off('disconnect', handleDisconnect)
    wsService.client.off('message', handleMessage)
    wsService.client.off('error', handleError)
  }

  // 组件挂载时初始化
  onMounted(() => {
    setupEventListeners()

    // 如果WebSocket服务未初始化，则初始化
    if (!wsService.isInitialized) {
      connect()
    } else {
      updateStatus()
    }
  })

  // 组件卸载时清理
  onUnmounted(() => {
    removeEventListeners()

    // 如果设置了自动清理，则销毁服务
    if (options.autoDestroy) {
      disconnect()
    }
  })

  // 返回响应式数据和方法
  return {
    // 状态
    isConnected,
    status,
    statusText,
    reconnectAttempts,
    messages,
    error,
    canSend,

    // 方法
    connect,
    disconnect,
    reconnect,
    sendMessage,
    sendNotification,
    sendChatMessage,
    sendSystemMessage,
    clearMessages,
    updateStatus
  }
}

// 简化的WebSocket Hook
export function useSimpleWebSocket() {
  const isConnected = ref(false)
  const status = ref('disconnected')

  function updateStatus() {
    isConnected.value = wsService.isConnected()
    status.value = wsService.getStatus()
  }

  onMounted(() => {
    if (!wsService.isInitialized) {
      wsService.init()
    }
    updateStatus()

    // 定期更新状态
    const interval = setInterval(updateStatus, 1000)

    onUnmounted(() => {
      clearInterval(interval)
    })
  })

  return {
    isConnected,
    status,
    connect: () => wsService.init(),
    disconnect: () => wsService.destroy(),
    send: (type, data) => wsService.send(type, data),
    notify: (title, message, type) => wsService.sendNotification(title, message, type)
  }
}

export default useWebSocket
