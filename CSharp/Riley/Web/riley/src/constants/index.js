// 常量定义入口文件

// API 相关常量
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'
export const API_TIMEOUT = 10000

// 路由相关常量
export const ROUTE_NAMES = {
  HOME: 'home',
  LOGIN: 'login',
  DASHBOARD: 'dashboard',
  USER_PROFILE: 'user-profile',
  SETTINGS: 'settings'
}

// 状态相关常量
export const STATUS = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info'
}

// 权限相关常量
export const PERMISSIONS = {
  READ: 'read',
  WRITE: 'write',
  DELETE: 'delete',
  ADMIN: 'admin'
}

// 主题相关常量
export const THEME = {
  LIGHT: 'light',
  DARK: 'dark'
}

// 本地存储键名
export const STORAGE_KEYS = {
  TOKEN: 'token',
  USER_INFO: 'userInfo',
  THEME: 'theme',
  LANGUAGE: 'language'
}

// 分页相关常量
export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 10,
  PAGE_SIZE_OPTIONS: [10, 20, 50, 100]
}
