// API入口文件

// 导出配置
export { API_ENDPOINTS, REQUEST_CONFIG } from './config'
export { default as apiConfig } from './config'

// 导入用户API服务
import {
  login,
  logout,
  register,
  getUserProfile,
  updateUserProfile,
  changePassword,
  getUserProfileWithLoading,
  changePasswordWithConfirm,
  getUserProfileWithRetry,
  UserService
} from './user'

// 导出用户API服务
export {
  login,
  logout,
  register,
  getUserProfile,
  updateUserProfile,
  changePassword,
  getUserProfileWithLoading,
  changePasswordWithConfirm,
  getUserProfileWithRetry,
  UserService
}

// 导出默认用户服务
export { default as userApi } from './user'

// 统一导出所有API服务
export const api = {
  user: {
    login,
    logout,
    register,
    getUserProfile,
    updateUserProfile,
    changePassword,
    getUserProfileWithLoading,
    changePasswordWithConfirm,
    getUserProfileWithRetry,
    UserService
  }
}

export default api
