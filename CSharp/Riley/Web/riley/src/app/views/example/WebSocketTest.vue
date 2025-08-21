<template>
  <div class="websocket-test">
    <el-card class="websocket-card">
      <template #header>
        <div class="card-header">
          <span>WebSocket 客户端测试</span>
          <div class="status-indicator">
            <el-tag
              :type="isConnected ? 'success' : 'danger'"
              :icon="isConnected ? 'CircleCheck' : 'CircleClose'"
            >
              {{ statusText }}
            </el-tag>
            <span v-if="reconnectAttempts > 0" class="reconnect-info">
              重连次数: {{ reconnectAttempts }}
            </span>
          </div>
        </div>
      </template>

      <!-- 连接控制 -->
      <div class="connection-controls">
        <el-button type="primary" :disabled="isConnected" @click="connect"> 连接 </el-button>
        <el-button type="danger" :disabled="!isConnected" @click="disconnect"> 断开 </el-button>
        <el-button type="warning" :disabled="isConnected" @click="reconnect"> 重连 </el-button>
      </div>

      <!-- 消息发送区域 -->
      <div class="message-send-area">
        <el-divider>发送消息</el-divider>

        <!-- 通知消息 -->
        <el-form :model="notificationForm" label-width="80px">
          <el-form-item label="通知类型">
            <el-select v-model="notificationForm.type" placeholder="选择通知类型">
              <el-option label="信息" value="info" />
              <el-option label="成功" value="success" />
              <el-option label="警告" value="warning" />
              <el-option label="错误" value="error" />
            </el-select>
          </el-form-item>
          <el-form-item label="标题">
            <el-input v-model="notificationForm.title" placeholder="通知标题" />
          </el-form-item>
          <el-form-item label="内容">
            <el-input
              v-model="notificationForm.message"
              type="textarea"
              placeholder="通知内容"
              :rows="2"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" :disabled="!canSend" @click="sendNotification">
              发送通知
            </el-button>
          </el-form-item>
        </el-form>

        <!-- 聊天消息 -->
        <el-form :model="chatForm" label-width="80px">
          <el-form-item label="发送者">
            <el-input v-model="chatForm.sender" placeholder="发送者名称" />
          </el-form-item>
          <el-form-item label="消息">
            <el-input v-model="chatForm.message" type="textarea" placeholder="聊天消息" :rows="2" />
          </el-form-item>
          <el-form-item>
            <el-button type="success" :disabled="!canSend" @click="sendChatMessage">
              发送聊天
            </el-button>
          </el-form-item>
        </el-form>

        <!-- 系统消息 -->
        <el-form :model="systemForm" label-width="80px">
          <el-form-item label="操作类型">
            <el-select v-model="systemForm.action" placeholder="选择操作类型">
              <el-option label="维护" value="maintenance" />
              <el-option label="更新" value="update" />
              <el-option label="其他" value="other" />
            </el-select>
          </el-form-item>
          <el-form-item label="消息">
            <el-input
              v-model="systemForm.message"
              type="textarea"
              placeholder="系统消息"
              :rows="2"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="warning" :disabled="!canSend" @click="sendSystemMessage">
              发送系统消息
            </el-button>
          </el-form-item>
        </el-form>
      </div>

      <!-- 消息列表 -->
      <div class="message-list">
        <el-divider>
          消息记录
          <el-button type="text" size="small" @click="clearMessages"> 清空 </el-button>
        </el-divider>

        <div class="messages-container">
          <div
            v-for="message in messages"
            :key="message.id"
            class="message-item"
            :class="message.direction"
          >
            <div class="message-header">
              <span class="message-type">{{ message.type }}</span>
              <span class="message-time">{{ formatTime(message.timestamp) }}</span>
              <span class="message-direction">{{
                message.direction === 'in' ? '接收' : '发送'
              }}</span>
            </div>
            <div class="message-content">
              <pre>{{ JSON.stringify(message.data, null, 2) }}</pre>
            </div>
          </div>
        </div>
      </div>

      <!-- 错误信息 -->
      <div v-if="error" class="error-info">
        <el-alert :title="'连接错误: ' + error.message" type="error" show-icon :closable="false" />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useWebSocket } from '@/common/composables/useWebSocket'

// 使用WebSocket组合式函数
const {
  isConnected,
  status,
  statusText,
  reconnectAttempts,
  messages,
  error,
  canSend,
  connect,
  disconnect,
  reconnect,
  sendNotification: sendNotificationMessage,
  sendChatMessage: sendChatMessageFunc,
  sendSystemMessage: sendSystemMessageFunc,
  clearMessages,
} = useWebSocket()

// 表单数据
const notificationForm = reactive({
  type: 'info',
  title: '',
  message: '',
})

const chatForm = reactive({
  sender: '测试用户',
  message: '',
})

const systemForm = reactive({
  action: 'update',
  message: '',
})

// 发送通知
function sendNotification() {
  if (notificationForm.title && notificationForm.message) {
    sendNotificationMessage(notificationForm.title, notificationForm.message, notificationForm.type)
    // 清空表单
    notificationForm.title = ''
    notificationForm.message = ''
  }
}

// 发送聊天消息
function sendChatMessage() {
  if (chatForm.sender && chatForm.message) {
    sendChatMessageFunc(chatForm.sender, chatForm.message)
    // 清空消息
    chatForm.message = ''
  }
}

// 发送系统消息
function sendSystemMessage() {
  if (systemForm.message) {
    sendSystemMessageFunc(systemForm.action, systemForm.message)
    // 清空消息
    systemForm.message = ''
  }
}

// 格式化时间
function formatTime(timestamp) {
  return new Date(timestamp).toLocaleTimeString()
}
</script>

<style scoped lang="scss">
.websocket-test {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.websocket-card {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;

    .status-indicator {
      display: flex;
      align-items: center;
      gap: 10px;

      .reconnect-info {
        font-size: 12px;
        color: #909399;
      }
    }
  }
}

.connection-controls {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
}

.message-send-area {
  margin-bottom: 20px;

  .el-form {
    margin-bottom: 20px;
    padding: 15px;
    border: 1px solid #ebeef5;
    border-radius: 4px;
    background-color: #fafafa;
  }
}

.message-list {
  .messages-container {
    max-height: 400px;
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
    border-left: 4px solid #409eff;

    &.in {
      border-left-color: #67c23a;
      background-color: #f0f9ff;
    }

    &.out {
      border-left-color: #409eff;
      background-color: #f0f9ff;
    }

    .message-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 5px;
      font-size: 12px;
      color: #909399;

      .message-type {
        font-weight: bold;
        color: #409eff;
      }

      .message-direction {
        color: #67c23a;
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

.error-info {
  margin-top: 20px;
}
</style>
