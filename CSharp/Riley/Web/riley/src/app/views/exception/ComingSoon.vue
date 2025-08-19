<template>
  <div class="coming-soon">
    <div class="coming-soon__container">
      <div class="coming-soon__content">
        <div class="coming-soon__icon">
          <el-icon size="120" color="var(--el-color-primary)">
            <Construction />
          </el-icon>
        </div>
        
        <h1 class="coming-soon__title">页面建设中</h1>
        <p class="coming-soon__description">
          抱歉，您访问的页面正在建设中，敬请期待！
        </p>
        
        <div class="coming-soon__actions">
          <el-button type="primary" @click="handleGoHome">
            <el-icon><House /></el-icon>
            返回首页
          </el-button>
          <el-button @click="handleGoBack">
            <el-icon><Back /></el-icon>
            返回上页
          </el-button>
        </div>
        
        <div class="coming-soon__progress">
          <el-progress 
            :percentage="progressPercentage" 
            :stroke-width="8"
            :show-text="false"
          />
          <p class="coming-soon__progress-text">
            开发进度: {{ progressPercentage }}%
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Construction, House, Back } from '@element-plus/icons-vue'

const router = useRouter()
const progressPercentage = ref(0)

// 事件处理函数
function handleGoHome() {
  router.push('/')
}

function handleGoBack() {
  router.go(-1)
}

// 模拟进度条动画
onMounted(() => {
  const timer = setInterval(() => {
    if (progressPercentage.value < 85) {
      progressPercentage.value += Math.random() * 10
    } else {
      clearInterval(timer)
    }
  }, 200)
})
</script>

<style scoped lang="scss">
.coming-soon {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--el-color-primary-light-9) 0%, var(--el-color-primary-light-8) 100%);

  &__container {
    max-width: 600px;
    width: 100%;
    padding: 2rem;
  }

  &__content {
    text-align: center;
    background: var(--el-bg-color);
    border-radius: var(--el-border-radius-large);
    padding: 3rem 2rem;
    box-shadow: var(--el-box-shadow-light);
  }

  &__icon {
    margin-bottom: 2rem;
    animation: bounce 2s infinite;
  }

  &__title {
    font-size: 2.5rem;
    color: var(--el-text-color-primary);
    margin-bottom: 1rem;
    font-weight: 600;
  }

  &__description {
    font-size: 1.1rem;
    color: var(--el-text-color-secondary);
    margin-bottom: 2rem;
    line-height: 1.6;
  }

  &__actions {
    display: flex;
    gap: 1rem;
    justify-content: center;
    margin-bottom: 3rem;
    flex-wrap: wrap;
  }

  &__progress {
    max-width: 300px;
    margin: 0 auto;

    &-text {
      margin-top: 0.5rem;
      font-size: 0.9rem;
      color: var(--el-text-color-secondary);
    }
  }
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-10px);
  }
  60% {
    transform: translateY(-5px);
  }
}

@media (max-width: 768px) {
  .coming-soon {
    &__container {
      padding: 1rem;
    }

    &__content {
      padding: 2rem 1rem;
    }

    &__title {
      font-size: 2rem;
    }

    &__actions {
      flex-direction: column;
      align-items: center;
    }
  }
}
</style>
