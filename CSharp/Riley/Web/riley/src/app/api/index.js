// API 接口入口文件

import { request } from '@/common/utils/request'

// 用户相关接口
export const userApi = {
  // 登录
  login: (data) => request.post('/user/login', data),

  // 注册
  register: (data) => request.post('/user/register', data),

  // 获取用户信息
  getUserInfo: () => request.get('/user/info'),

  // 更新用户信息
  updateUserInfo: (data) => request.put('/user/info', data),

  // 修改密码
  changePassword: (data) => request.put('/user/password', data),

  // 退出登录
  logout: () => request.post('/user/logout')
}

// 文件上传接口
export const uploadApi = {
  // 上传文件
  uploadFile: (file, onProgress) => {
    const formData = new FormData()
    formData.append('file', file)

    return request.post('/upload/file', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: onProgress
    })
  },

  // 批量上传
  uploadFiles: (files, onProgress) => {
    const formData = new FormData()
    files.forEach(file => {
      formData.append('files', file)
    })

    return request.post('/upload/files', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: onProgress
    })
  }
}

// 数据接口
export const dataApi = {
  // 获取列表数据
  getList: (params) => request.get('/data/list', { params }),

  // 获取详情
  getDetail: (id) => request.get(`/data/detail/${id}`),

  // 创建数据
  create: (data) => request.post('/data/create', data),

  // 更新数据
  update: (id, data) => request.put(`/data/update/${id}`, data),

  // 删除数据
  delete: (id) => request.delete(`/data/delete/${id}`),

  // 批量删除
  batchDelete: (ids) => request.delete('/data/batch-delete', { data: { ids } })
}

// 系统接口
export const systemApi = {
  // 获取系统配置
  getConfig: () => request.get('/system/config'),

  // 更新系统配置
  updateConfig: (data) => request.put('/system/config', data),

  // 获取系统状态
  getStatus: () => request.get('/system/status'),

  // 获取系统日志
  getLogs: (params) => request.get('/system/logs', { params })
}
