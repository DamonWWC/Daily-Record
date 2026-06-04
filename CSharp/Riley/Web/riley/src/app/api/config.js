// API配置文件

// 环境配置
const ENV_CONFIG = {
  development: {
    API_BASE_URL: '/api',
    STATIC_BASE_URL: '/static',
    WS_BASE_URL: '/ws',
    TIMEOUT: 10000
  },
  production: {
    API_BASE_URL: import.meta.env.VITE_API_BASE_URL || 'https://api.riley.com',
    STATIC_BASE_URL: import.meta.env.VITE_STATIC_BASE_URL || 'https://static.riley.com',
    WS_BASE_URL: import.meta.env.VITE_WS_BASE_URL || 'wss://ws.riley.com',
    TIMEOUT: 15000
  }
}

// 获取当前环境配置
const getEnvConfig = () => {
  const env = import.meta.env.MODE || 'development'
  return ENV_CONFIG[env] || ENV_CONFIG.development
}

// API端点配置
export const API_ENDPOINTS = {
  // 用户相关
  USER: {
    LOGIN: '/auth/login',
    LOGOUT: '/auth/logout',
    REGISTER: '/auth/register',
    PROFILE: '/user/profile',
    UPDATE_PROFILE: '/user/profile',
    CHANGE_PASSWORD: '/user/change-password'
  },

  // 权限相关
  PERMISSION: {
    ROLES: '/permission/roles',
    PERMISSIONS: '/permission/permissions',
    USER_ROLES: '/permission/user-roles'
  },

  // 字典相关
  DICTIONARY: {
    LIST: '/dictionary/list',
    CREATE: '/dictionary/create',
    UPDATE: '/dictionary/update',
    DELETE: '/dictionary/delete',
    ITEMS: '/dictionary/items'
  },

  // 文件上传
  UPLOAD: {
    FILE: '/upload/file',
    IMAGE: '/upload/image',
    BATCH: '/upload/batch'
  },

  // 系统配置
  SYSTEM: {
    CONFIG: '/system/config',
    LOGS: '/system/logs',
    HEALTH: '/system/health'
  }
}

// 请求配置
export const REQUEST_CONFIG = {
  // 基础配置
  baseURL: getEnvConfig().API_BASE_URL,
  timeout: getEnvConfig().TIMEOUT,

  // 请求头配置
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  },

  // 响应类型
  responseType: 'json',

  // 是否携带凭证
  withCredentials: true,

  // 重试配置
  retry: {
    maxRetries: 3,
    delay: 1000,
    retryCondition: (error) => {
      // 只在网络错误或5xx错误时重试
      return !error.response || (error.response.status >= 500 && error.response.status < 600)
    }
  }
}

// 导出配置
export default {
  endpoints: API_ENDPOINTS,
  request: REQUEST_CONFIG,
  env: getEnvConfig()
}
