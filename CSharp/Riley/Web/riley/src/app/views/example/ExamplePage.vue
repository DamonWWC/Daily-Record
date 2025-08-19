<template>
  <div class="example-page">
    <div class="example-page__header">
      <h1 class="example-page__title">示例页面</h1>
      <p class="example-page__description">展示Element Plus组件和ECharts图表的使用</p>
    </div>

    <div class="example-page__content">
      <!-- Element Plus 组件示例 -->
      <div class="example-page__section">
        <h2 class="example-page__section-title">Element Plus 组件</h2>
        
        <div class="example-page__demo">
          <el-card class="example-page__card">
            <template #header>
              <div class="example-page__card-header">
                <span>按钮组件</span>
              </div>
            </template>
            
            <div class="example-page__button-group">
              <el-button type="primary">主要按钮</el-button>
              <el-button type="success">成功按钮</el-button>
              <el-button type="warning">警告按钮</el-button>
              <el-button type="danger">危险按钮</el-button>
              <el-button type="info">信息按钮</el-button>
            </div>
          </el-card>
        </div>

        <div class="example-page__demo">
          <el-card class="example-page__card">
            <template #header>
              <div class="example-page__card-header">
                <span>表单组件</span>
              </div>
            </template>
            
            <el-form :model="formData" label-width="120px">
              <el-form-item label="用户名">
                <el-input v-model="formData.username" placeholder="请输入用户名" />
              </el-form-item>
              <el-form-item label="邮箱">
                <el-input v-model="formData.email" placeholder="请输入邮箱" />
              </el-form-item>
              <el-form-item label="选择器">
                <el-select v-model="formData.select" placeholder="请选择">
                  <el-option label="选项1" value="1" />
                  <el-option label="选项2" value="2" />
                  <el-option label="选项3" value="3" />
                </el-select>
              </el-form-item>
              <el-form-item>
                <el-button type="primary" @click="handleSubmit">提交</el-button>
                <el-button @click="handleReset">重置</el-button>
              </el-form-item>
            </el-form>
          </el-card>
        </div>
      </div>

      <!-- ECharts 图表示例 -->
      <div class="example-page__section">
        <h2 class="example-page__section-title">ECharts 图表</h2>
        
        <div class="example-page__charts">
          <el-card class="example-page__card">
            <template #header>
              <div class="example-page__card-header">
                <span>折线图</span>
              </div>
            </template>
            
            <div ref="lineChartRef" class="example-page__chart"></div>
          </el-card>

          <el-card class="example-page__card">
            <template #header>
              <div class="example-page__card-header">
                <span>饼图</span>
              </div>
            </template>
            
            <div ref="pieChartRef" class="example-page__chart"></div>
          </el-card>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import * as echarts from 'echarts'
import { ElMessage } from 'element-plus'

// 响应式数据
const formData = reactive({
  username: '',
  email: '',
  select: ''
})

// 图表引用
const lineChartRef = ref(null)
const pieChartRef = ref(null)
let lineChart = null
let pieChart = null

// 事件处理函数
function handleSubmit() {
  ElMessage.success('表单提交成功！')
  console.log('表单数据:', formData)
}

function handleReset() {
  Object.assign(formData, {
    username: '',
    email: '',
    select: ''
  })
  ElMessage.info('表单已重置')
}

// 初始化折线图
function initLineChart() {
  if (lineChartRef.value) {
    lineChart = echarts.init(lineChartRef.value)
    
    const option = {
      title: {
        text: '销售数据趋势'
      },
      tooltip: {
        trigger: 'axis'
      },
      xAxis: {
        type: 'category',
        data: ['1月', '2月', '3月', '4月', '5月', '6月']
      },
      yAxis: {
        type: 'value'
      },
      series: [
        {
          name: '销售额',
          type: 'line',
          data: [120, 200, 150, 80, 70, 110],
          smooth: true
        }
      ]
    }
    
    lineChart.setOption(option)
  }
}

// 初始化饼图
function initPieChart() {
  if (pieChartRef.value) {
    pieChart = echarts.init(pieChartRef.value)
    
    const option = {
      title: {
        text: '产品分布',
        left: 'center'
      },
      tooltip: {
        trigger: 'item'
      },
      legend: {
        orient: 'vertical',
        left: 'left'
      },
      series: [
        {
          name: '产品类型',
          type: 'pie',
          radius: '50%',
          data: [
            { value: 1048, name: '产品A' },
            { value: 735, name: '产品B' },
            { value: 580, name: '产品C' },
            { value: 484, name: '产品D' }
          ],
          emphasis: {
            itemStyle: {
              shadowBlur: 10,
              shadowOffsetX: 0,
              shadowColor: 'rgba(0, 0, 0, 0.5)'
            }
          }
        }
      ]
    }
    
    pieChart.setOption(option)
  }
}

// 生命周期钩子
onMounted(() => {
  initLineChart()
  initPieChart()
  
  // 监听窗口大小变化
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  if (lineChart) {
    lineChart.dispose()
  }
  if (pieChart) {
    pieChart.dispose()
  }
  window.removeEventListener('resize', handleResize)
})

// 处理窗口大小变化
function handleResize() {
  if (lineChart) {
    lineChart.resize()
  }
  if (pieChart) {
    pieChart.resize()
  }
}
</script>

<style scoped lang="scss">
.example-page {
  padding: 2rem;

  &__header {
    text-align: center;
    margin-bottom: 3rem;
  }

  &__title {
    font-size: 2rem;
    color: var(--el-text-color-primary);
    margin-bottom: 0.5rem;
  }

  &__description {
    font-size: 1rem;
    color: var(--el-text-color-secondary);
  }

  &__content {
    max-width: 1200px;
    margin: 0 auto;
  }

  &__section {
    margin-bottom: 3rem;

    &-title {
      font-size: 1.5rem;
      color: var(--el-text-color-primary);
      margin-bottom: 1.5rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid var(--el-color-primary);
    }
  }

  &__demo {
    margin-bottom: 2rem;
  }

  &__card {
    margin-bottom: 1rem;

    &-header {
      font-weight: 600;
      color: var(--el-text-color-primary);
    }
  }

  &__button-group {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
  }

  &__charts {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
    gap: 2rem;
  }

  &__chart {
    height: 300px;
    width: 100%;
  }
}

@media (max-width: 768px) {
  .example-page {
    padding: 1rem;

    &__charts {
      grid-template-columns: 1fr;
    }

    &__button-group {
      flex-direction: column;
    }
  }
}
</style>
