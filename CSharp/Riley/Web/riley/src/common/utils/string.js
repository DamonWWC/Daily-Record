// 字符串工具函数

/**
 * 首字母大写
 * @param {string} str - 字符串
 * @returns {string}
 */
export function capitalize(str) {
  if (!str) return ''
  return str.charAt(0).toUpperCase() + str.slice(1)
}

/**
 * 驼峰命名转短横线命名
 * @param {string} str - 字符串
 * @returns {string}
 */
export function camelToKebab(str) {
  return str.replace(/([a-z0-9]|(?=[A-Z]))([A-Z])/g, '$1-$2').toLowerCase()
}

/**
 * 短横线命名转驼峰命名
 * @param {string} str - 字符串
 * @returns {string}
 */
export function kebabToCamel(str) {
  return str.replace(/-([a-z])/g, (match, letter) => letter.toUpperCase())
}

/**
 * 短横线命名转帕斯卡命名
 * @param {string} str - 字符串
 * @returns {string}
 */
export function kebabToPascal(str) {
  return capitalize(kebabToCamel(str))
}

/**
 * 截断字符串
 * @param {string} str - 字符串
 * @param {number} length - 最大长度
 * @param {string} suffix - 后缀
 * @returns {string}
 */
export function truncate(str, length = 30, suffix = '...') {
  if (!str || str.length <= length) return str
  return str.substring(0, length) + suffix
}

/**
 * 移除HTML标签
 * @param {string} str - 字符串
 * @returns {string}
 */
export function stripHtml(str) {
  if (!str) return ''
  return str.replace(/<[^>]*>/g, '')
}

/**
 * 转义HTML字符
 * @param {string} str - 字符串
 * @returns {string}
 */
export function escapeHtml(str) {
  if (!str) return ''
  const map = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  }
  return str.replace(/[&<>"']/g, char => map[char])
}

/**
 * 生成随机字符串
 * @param {number} length - 长度
 * @returns {string}
 */
export function randomString(length = 8) {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
  let result = ''
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return result
}

/**
 * 格式化文件大小
 * @param {number} bytes - 字节数
 * @returns {string}
 */
export function formatFileSize(bytes) {
  if (bytes === 0) return '0 B'
  
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
