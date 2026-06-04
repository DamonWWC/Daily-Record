// 用户API服务

import { http, createLoadingRequest, createConfirmRequest, createRetryRequest } from '@/common/utils/request'
import { API_ENDPOINTS } from './config'

// 用户登录
export function login(data) {
  return http.post(API_ENDPOINTS.USER.LOGIN, data)
}

// 用户登出
export function logout() {
  return http.post(API_ENDPOINTS.USER.LOGOUT)
}

// 用户注册
export function register(data) {
  return http.post(API_ENDPOINTS.USER.REGISTER, data)
}

// 获取用户信息
export function getUserProfile() {
  return http.get(API_ENDPOINTS.USER.PROFILE)
}

// 更新用户信息
export function updateUserProfile(data) {
  return http.put(API_ENDPOINTS.USER.UPDATE_PROFILE, data)
}

// 修改密码
export function changePassword(data) {
  return http.post(API_ENDPOINTS.USER.CHANGE_PASSWORD, data)
}

// 带loading的用户信息获取
export const getUserProfileWithLoading = createLoadingRequest(getUserProfile)

// 带确认的密码修改
export const changePasswordWithConfirm = createConfirmRequest(
  changePassword,
  '确定要修改密码吗？'
)

// 带重试的用户信息获取
export const getUserProfileWithRetry = createRetryRequest(getUserProfile, 3, 1000)

// 用户API服务类
export class UserService {
  // 登录
  static async login(credentials) {
    try {
      const response = await login(credentials)
      return {
        success: true,
        data: response.data,
        message: '登录成功'
      }
    } catch (error) {
      return {
        success: false,
        data: null,
        message: error.message || '登录失败'
      }
    }
  }

  // 登出
  static async logout() {
    try {
      await logout()
      return {
        success: true,
        message: '登出成功'
      }
    } catch (error) {
      return {
        success: false,
        message: error.message || '登出失败'
      }
    }
  }

  // 获取用户信息
  static async getProfile() {
    try {
      const response = await getUserProfileWithLoading()
      return {
        success: true,
        data: response.data,
        message: '获取用户信息成功'
      }
    } catch (error) {
      return {
        success: false,
        data: null,
        message: error.message || '获取用户信息失败'
      }
    }
  }

  // 更新用户信息
  static async updateProfile(profileData) {
    try {
      const response = await updateUserProfile(profileData)
      return {
        success: true,
        data: response.data,
        message: '更新用户信息成功'
      }
    } catch (error) {
      return {
        success: false,
        data: null,
        message: error.message || '更新用户信息失败'
      }
    }
  }

  // 修改密码
  static async changePassword(passwordData) {
    try {
      await changePasswordWithConfirm(passwordData)
      return {
        success: true,
        message: '密码修改成功'
      }
    } catch (error) {
      return {
        success: false,
        message: error.message || '密码修改失败'
      }
    }
  }
}

export default UserService
