<template>
  <div class="websocket-external-example">
    <el-card class="example-card">
      <template #header>
        <div class="card-header">
          <span>WebSocket 外部监听示例</span>
          <el-tag :type="isConnected ? 'success' : 'danger'">
            {{ isConnected ? '已连接' : '未连接' }}
          </el-tag>
        </div>
      </template>

      <!-- 连接控制 -->
      <div class="control-section">
        <el-button type="primary" @click="initWebSocket">初始化连接</el-button>
        <el-button type="warning" @click="setupListeners">设置监听器</el-button>
        <el-button type="info" @click="sendTestMessages">发送测试消息</el-button>
        <el-button type="danger" @click="clearAll">清空所有</el-button>
      </div>

      <!-- 实时消息显示 -->
      <div class="message-display">
        <el-divider>实时接收的消息</el-divider>
        <div class="real-time-messages">
          <div
            v-for="message in realtimeMessages"
            :key="message.id"
            class="message-item"
            :class="`message-${message.type}`"
          >
            <div class="message-header">
              <el-tag size="small" :type="getTagType(message.type)">{{ message.type }}</el-tag>
              <span class="message-time">{{ formatTime(message.receivedAt) }}</span>
            </div>
            <div class="message-content">
              <pre>{{ JSON.stringify(message.data, null, 2) }}</pre>
            </div>
          </div>
        </div>
      </div>

      <!-- 消息历史查询 -->
      <div class="history-section">
        <el-divider>消息历史查询</el-divider>

        <div class="query-controls">
          <el-form :model="queryForm" inline>
            <el-form-item label="消息类型">
              <el-select v-model="queryForm.type" placeholder="选择类型" clearable>
                <el-option label="全部" value="" />
                <el-option label="通知" value="notification" />
                <el-option label="聊天" value="chat" />
                <el-option label="系统" value="system" />
              </el-select>
            </el-form-item>
            <el-form-item label="数量限制">
              <el-input-number v-model="queryForm.limit" :min="1" :max="100" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="queryHistory">查询历史</el-button>
              <el-button type="success" @click="getLatestMessages">获取最新</el-button>
            </el-form-item>
          </el-form>
        </div>

        <div class="history-results">
          <div v-for="message in historyMessages" :key="message.id" class="history-item">
            <div class="history-header">
              <el-tag size="small" :type="getTagType(message.type)">{{ message.type }}</el-tag>
              <span class="history-time">{{ formatTime(message.receivedAt) }}</span>
            </div>
            <div class="history-content">
              <pre>{{ JSON.stringify(message.data, null, 2) }}</pre>
            </div>
          </div>
        </div>
      </div>

      <!-- 统计信息 -->
      <div class="stats-section">
        <el-divider>统计信息</el-divider>
        <el-row :gutter="20">
          <el-col :span="6">
            <el-statistic title="总消息数" :value="totalMessages" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="通知消息" :value="notificationCount" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="聊天消息" :value="chatCount" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="系统消息" :value="systemCount" />
          </el-col>
        </el-row>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { ws } from '@/common/utils/websocket-service'
import { ElMessage } from 'element-plus'

// 响应式数据
const isConnected = ref(false)
const realtimeMessages = ref([])
const historyMessages = ref([])
const messageStats = ref({
  total: 0,
  notification: 0,
  chat: 0,
  system: 0,
})

// 查询表单
const queryForm = reactive({
  type: '',
  limit: 10,
})

// 监听器函数引用（用于清理）
const listeners = {
  message: null,
  notification: null,
  chat: null,
  system: null,
  connect: null,
  disconnect: null,
}

// 计算属性
const totalMessages = computed(() => messageStats.value.total)
const notificationCount = computed(() => messageStats.value.notification)
const chatCount = computed(() => messageStats.value.chat)
const systemCount = computed(() => messageStats.value.system)

// 初始化WebSocket连接
function initWebSocket() {
  ws.init()
  updateConnectionStatus()
  ElMessage.success('WebSocket 初始化完成')
}

// 设置事件监听器
function setupListeners() {
  // 监听所有消息
  listeners.message = (data, eventType) => {
    console.log('收到消息:', eventType, data)
    addRealtimeMessage(data)
    updateStats(data.type)
  }
  ws.onMessage(listeners.message)

  // 监听特定类型的消息
  listeners.notification = (data) => {
    console.log('收到通知消息:', data)
    ElMessage.info(`外部监听到通知: ${data.data.title}`)
  }
  ws.onNotification(listeners.notification)

  listeners.chat = (data) => {
    console.log('收到聊天消息:', data)
    ElMessage.info(`外部监听到聊天: ${data.data.sender} 说了话`)
  }
  ws.onChat(listeners.chat)

  listeners.system = (data) => {
    console.log('收到系统消息:', data)
    ElMessage.warning(`外部监听到系统消息: ${data.data.action}`)
  }
  ws.onSystem(listeners.system)

  // 监听连接状态变化
  listeners.connect = () => {
    console.log('WebSocket 连接成功')
    updateConnectionStatus()
    ElMessage.success('WebSocket 连接成功')
  }
  ws.onConnect(listeners.connect)

  listeners.disconnect = () => {
    console.log('WebSocket 连接断开')
    updateConnectionStatus()
    ElMessage.warning('WebSocket 连接断开')
  }
  ws.onDisconnect(listeners.disconnect)

  ElMessage.success('事件监听器设置完成')
}

// 发送测试消息
function sendTestMessages() {
  if (!ws.isConnected()) {
    ElMessage.error('WebSocket 未连接')
    return
  }

  // 发送通知消息
  ws.notify('测试通知', '这是一条外部监听测试通知', 'info')

  // 发送聊天消息
  setTimeout(() => {
    ws.chat('测试用户', '这是一条外部监听测试聊天消息')
  }, 1000)

  // 发送系统消息
  setTimeout(() => {
    ws.system('update', '这是一条外部监听测试系统消息', 'info')
  }, 2000)

  ElMessage.success('测试消息已发送')
}

// 查询消息历史
function queryHistory() {
  const filter = {}

  if (queryForm.type) {
    filter.type = queryForm.type
  }

  if (queryForm.limit) {
    filter.limit = queryForm.limit
  }

  historyMessages.value = ws.getHistory(filter)
  ElMessage.success(`查询到 ${historyMessages.value.length} 条历史消息`)
}

// 获取最新消息
function getLatestMessages() {
  const count = queryForm.limit || 10

  if (queryForm.type) {
    historyMessages.value = ws.getLatestByType(queryForm.type, count)
  } else {
    historyMessages.value = ws.getLatest(count)
  }

  ElMessage.success(`获取到 ${historyMessages.value.length} 条最新消息`)
}

// 清空所有数据和监听器
function clearAll() {
  // 清空实时消息
  realtimeMessages.value = []

  // 清空历史消息
  historyMessages.value = []
  ws.clearHistory()

  // 重置统计
  messageStats.value = {
    total: 0,
    notification: 0,
    chat: 0,
    system: 0,
  }

  // 移除所有监听器
  Object.keys(listeners).forEach((key) => {
    if (listeners[key]) {
      ws.off(key, listeners[key])
      listeners[key] = null
    }
  })

  ElMessage.success('已清空所有数据和监听器')
}

// 添加实时消息到显示列表
function addRealtimeMessage(data) {
  realtimeMessages.value.unshift({
    ...data,
    id: Date.now() + Math.random(),
  })

  // 限制显示数量
  if (realtimeMessages.value.length > 20) {
    realtimeMessages.value = realtimeMessages.value.slice(0, 20)
  }
}

// 更新统计信息
function updateStats(type) {
  messageStats.value.total++

  if (messageStats.value[type] !== undefined) {
    messageStats.value[type]++
  }
}

// 更新连接状态
function updateConnectionStatus() {
  isConnected.value = ws.isConnected()
}

// 格式化时间
function formatTime(date) {
  if (!date) return ''
  return new Date(date).toLocaleTimeString()
}

// 获取标签类型
function getTagType(messageType) {
  const typeMap = {
    notification: 'primary',
    chat: 'success',
    system: 'warning',
    error: 'danger',
  }
  return typeMap[messageType] || 'info'
}

// 组件挂载时的初始化
onMounted(() => {
  updateConnectionStatus()

  // 定期更新连接状态
  const statusInterval = setInterval(() => {
    updateConnectionStatus()
  }, 1000)

  onUnmounted(() => {
    clearInterval(statusInterval)
    clearAll()
  })
})
</script>

<style scoped lang="scss">
.websocket-external-example {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.example-card {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}

.control-section {
  margin-bottom: 20px;

  .el-button {
    margin-right: 10px;
  }
}

.message-display {
  margin-bottom: 30px;

  .real-time-messages {
    max-height: 300px;
    overflow-y: auto;
    border: 1px solid #ebeef5;
    border-radius: 4px;
    padding: 10px;
    background-color: #fafafa;
  }

  .message-item {
    margin-bottom: 10px;
    padding: 10px;
    border-radius: 4px;
    background-color: white;
    border-left: 4px solid #409eff;

    &.message-notification {
      border-left-color: #409eff;
    }

    &.message-chat {
      border-left-color: #67c23a;
    }

    &.message-system {
      border-left-color: #e6a23c;
    }

    .message-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 5px;

      .message-time {
        font-size: 12px;
        color: #909399;
      }
    }

    .message-content {
      pre {
        margin: 0;
        font-size: 12px;
        color: #606266;
        white-space: pre-wrap;
        word-break: break-all;
      }
    }
  }
}

.history-section {
  margin-bottom: 30px;

  .query-controls {
    margin-bottom: 15px;
  }

  .history-results {
    max-height: 400px;
    overflow-y: auto;
    border: 1px solid #ebeef5;
    border-radius: 4px;
    padding: 10px;
    background-color: #fafafa;
  }

  .history-item {
    margin-bottom: 10px;
    padding: 8px;
    border-radius: 4px;
    background-color: white;
    border-left: 3px solid #909399;

    .history-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 5px;

      .history-time {
        font-size: 12px;
        color: #909399;
      }
    }

    .history-content {
      pre {
        margin: 0;
        font-size: 11px;
        color: #606266;
        white-space: pre-wrap;
        word-break: break-all;
      }
    }
  }
}

.stats-section {
  .el-statistic {
    text-align: center;
  }
}
</style>

