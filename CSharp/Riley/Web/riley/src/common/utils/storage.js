// 存储工具函数

/**
 * 设置本地存储
 * @param {string} key - 键名
 * @param {any} value - 值
 */
export function setLocalStorage(key, value) {
  try {
    localStorage.setItem(key, JSON.stringify(value))
  } catch (error) {
    console.error('设置本地存储失败:', error)
  }
}

/**
 * 获取本地存储
 * @param {string} key - 键名
 * @param {any} defaultValue - 默认值
 * @returns {any}
 */
export function getLocalStorage(key, defaultValue = null) {
  try {
    const value = localStorage.getItem(key)
    return value ? JSON.parse(value) : defaultValue
  } catch (error) {
    console.error('获取本地存储失败:', error)
    return defaultValue
  }
}

/**
 * 移除本地存储
 * @param {string} key - 键名
 */
export function removeLocalStorage(key) {
  try {
    localStorage.removeItem(key)
  } catch (error) {
    console.error('移除本地存储失败:', error)
  }
}

/**
 * 清空本地存储
 */
export function clearLocalStorage() {
  try {
    localStorage.clear()
  } catch (error) {
    console.error('清空本地存储失败:', error)
  }
}

/**
 * 设置会话存储
 * @param {string} key - 键名
 * @param {any} value - 值
 */
export function setSessionStorage(key, value) {
  try {
    sessionStorage.setItem(key, JSON.stringify(value))
  } catch (error) {
    console.error('设置会话存储失败:', error)
  }
}

/**
 * 获取会话存储
 * @param {string} key - 键名
 * @param {any} defaultValue - 默认值
 * @returns {any}
 */
export function getSessionStorage(key, defaultValue = null) {
  try {
    const value = sessionStorage.getItem(key)
    return value ? JSON.parse(value) : defaultValue
  } catch (error) {
    console.error('获取会话存储失败:', error)
    return defaultValue
  }
}

/**
 * 移除会话存储
 * @param {string} key - 键名
 */
export function removeSessionStorage(key) {
  try {
    sessionStorage.removeItem(key)
  } catch (error) {
    console.error('移除会话存储失败:', error)
  }
}

/**
 * 清空会话存储
 */
export function clearSessionStorage() {
  try {
    sessionStorage.clear()
  } catch (error) {
    console.error('清空会话存储失败:', error)
  }
}
