<template>
  <div class="websocket-status">
    <el-tooltip :content="tooltipContent" placement="bottom" :show-after="500">
      <div class="status-indicator" @click="handleClick">
        <el-icon :class="statusClass" :size="16">
          <component :is="statusIcon" />
        </el-icon>
        <span v-if="showText" class="status-text">{{ statusText }}</span>
      </div>
    </el-tooltip>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { CircleCheck, CircleClose, Loading, Refresh } from '@element-plus/icons-vue'
import { useSimpleWebSocket } from '@/common/composables/useWebSocket'

// 定义props
const props = defineProps({
  showText: {
    type: Boolean,
    default: false,
  },
  clickable: {
    type: Boolean,
    default: true,
  },
})

// 定义emits
const emit = defineEmits(['click'])

// 使用WebSocket状态
const { isConnected, status, connect, disconnect } = useSimpleWebSocket()

// 计算属性
const statusText = computed(() => {
  if (isConnected.value) return '已连接'
  if (status.value === 0) return '连接中'
  return '未连接'
})

const statusClass = computed(() => {
  if (isConnected.value) return 'status-connected'
  if (status.value === 0) return 'status-connecting'
  return 'status-disconnected'
})

const statusIcon = computed(() => {
  if (isConnected.value) return CircleCheck
  if (status.value === 0) return Loading
  return CircleClose
})

const tooltipContent = computed(() => {
  if (isConnected.value) {
    return 'WebSocket已连接'
  }
  if (status.value === 0) {
    return 'WebSocket连接中...'
  }
  return 'WebSocket未连接，点击重连'
})

// 点击处理
function handleClick() {
  if (!props.clickable) return

  emit('click', { isConnected: isConnected.value, status: status.value })

  if (!isConnected.value) {
    connect()
  }
}
</script>

<style scoped lang="scss">
.websocket-status {
  display: inline-flex;
  align-items: center;

  .status-indicator {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 4px 8px;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.3s ease;

    &:hover {
      background-color: rgba(0, 0, 0, 0.05);
    }

    .status-text {
      font-size: 12px;
      color: #606266;
    }

    .status-connected {
      color: #67c23a;
      animation: pulse 2s infinite;
    }

    .status-connecting {
      color: #e6a23c;
      animation: spin 1s linear infinite;
    }

    .status-disconnected {
      color: #f56c6c;
    }
  }
}

@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
