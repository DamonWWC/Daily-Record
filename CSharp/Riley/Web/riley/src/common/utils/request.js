// 请求工具函数

import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getLocalStorage, removeLocalStorage } from './storage'
import { STORAGE_KEYS } from '@/constants'

// 创建axios实例
const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
request.interceptors.request.use(
  (config) => {
    // 添加token
    const token = getLocalStorage(STORAGE_KEYS.TOKEN)
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // 添加loading状态
    config.loading = true

    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
request.interceptors.response.use(
  (response) => {
    const { data, config } = response

    // 关闭loading
    if (config.loading) {
      config.loading = false
    }

    // 处理业务错误
    if (data.code !== 200 && data.code !== 0) {
      ElMessage.error(data.message || '请求失败')
      return Promise.reject(new Error(data.message || '请求失败'))
    }

    return data
  },
  (error) => {
    const { config, response } = error

    // 关闭loading
    if (config?.loading) {
      config.loading = false
    }

    // 处理HTTP错误
    if (response) {
      const { status, data } = response

      switch (status) {
        case 401:
          // 未授权，清除token并跳转到登录页
          removeLocalStorage(STORAGE_KEYS.TOKEN)
          ElMessage.error('登录已过期，请重新登录')
          // 这里可以跳转到登录页
          break
        case 403:
          ElMessage.error('没有权限访问该资源')
          break
        case 404:
          ElMessage.error('请求的资源不存在')
          break
        case 500:
          ElMessage.error('服务器内部错误')
          break
        default:
          ElMessage.error(data?.message || `请求失败 (${status})`)
      }
    } else if (error.code === 'ECONNABORTED') {
      ElMessage.error('请求超时，请检查网络连接')
    } else {
      ElMessage.error('网络错误，请检查网络连接')
    }

    return Promise.reject(error)
  }
)

// 封装请求方法
export const http = {
  get: (url, config = {}) => request.get(url, config),
  post: (url, data = {}, config = {}) => request.post(url, data, config),
  put: (url, data = {}, config = {}) => request.put(url, data, config),
  delete: (url, config = {}) => request.delete(url, config),
  patch: (url, data = {}, config = {}) => request.patch(url, data, config)
}

// 导出request实例
export { request }

// 创建带loading的请求方法
export function createLoadingRequest(requestFn) {
  return async (...args) => {
    const loading = ElLoading.service({
      lock: true,
      text: '加载中...',
      background: 'rgba(0, 0, 0, 0.7)'
    })

    try {
      const result = await requestFn(...args)
      return result
    } finally {
      loading.close()
    }
  }
}

// 创建带确认的请求方法
export function createConfirmRequest(requestFn, message = '确定要执行此操作吗？') {
  return async (...args) => {
    try {
      await ElMessageBox.confirm(message, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })

      const result = await requestFn(...args)
      ElMessage.success('操作成功')
      return result
    } catch (error) {
      if (error !== 'cancel') {
        throw error
      }
    }
  }
}
