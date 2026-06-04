// 验证工具函数

/**
 * 验证邮箱
 * @param {string} email - 邮箱地址
 * @returns {boolean}
 */
export function isValidEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

/**
 * 验证手机号
 * @param {string} phone - 手机号
 * @returns {boolean}
 */
export function isValidPhone(phone) {
  const phoneRegex = /^1[3-9]\d{9}$/
  return phoneRegex.test(phone)
}

/**
 * 验证身份证号
 * @param {string} idCard - 身份证号
 * @returns {boolean}
 */
export function isValidIdCard(idCard) {
  const idCardRegex = /(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/
  return idCardRegex.test(idCard)
}

/**
 * 验证URL
 * @param {string} url - URL地址
 * @returns {boolean}
 */
export function isValidUrl(url) {
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}

/**
 * 验证密码强度
 * @param {string} password - 密码
 * @returns {Object} 包含强度等级和建议
 */
export function validatePassword(password) {
  const result = {
    score: 0,
    level: 'weak',
    suggestions: []
  }
  
  if (!password) {
    result.suggestions.push('密码不能为空')
    return result
  }
  
  // 长度检查
  if (password.length >= 8) {
    result.score += 1
  } else {
    result.suggestions.push('密码长度至少8位')
  }
  
  // 包含数字
  if (/\d/.test(password)) {
    result.score += 1
  } else {
    result.suggestions.push('密码应包含数字')
  }
  
  // 包含小写字母
  if (/[a-z]/.test(password)) {
    result.score += 1
  } else {
    result.suggestions.push('密码应包含小写字母')
  }
  
  // 包含大写字母
  if (/[A-Z]/.test(password)) {
    result.score += 1
  } else {
    result.suggestions.push('密码应包含大写字母')
  }
  
  // 包含特殊字符
  if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
    result.score += 1
  } else {
    result.suggestions.push('密码应包含特殊字符')
  }
  
  // 设置强度等级
  if (result.score >= 4) {
    result.level = 'strong'
  } else if (result.score >= 3) {
    result.level = 'medium'
  } else {
    result.level = 'weak'
  }
  
  return result
}

/**
 * 验证是否为数字
 * @param {*} value - 值
 * @returns {boolean}
 */
export function isNumber(value) {
  return typeof value === 'number' && !isNaN(value)
}

/**
 * 验证是否为正整数
 * @param {*} value - 值
 * @returns {boolean}
 */
export function isPositiveInteger(value) {
  return Number.isInteger(value) && value > 0
}

/**
 * 验证是否为非负整数
 * @param {*} value - 值
 * @returns {boolean}
 */
export function isNonNegativeInteger(value) {
  return Number.isInteger(value) && value >= 0
}

/**
 * 验证字符串长度
 * @param {string} str - 字符串
 * @param {number} min - 最小长度
 * @param {number} max - 最大长度
 * @returns {boolean}
 */
export function validateStringLength(str, min = 0, max = Infinity) {
  if (typeof str !== 'string') return false
  const length = str.length
  return length >= min && length <= max
}

/**
 * 验证文件大小
 * @param {File} file - 文件对象
 * @param {number} maxSize - 最大大小（字节）
 * @returns {boolean}
 */
export function validateFileSize(file, maxSize) {
  return file.size <= maxSize
}

/**
 * 验证文件类型
 * @param {File} file - 文件对象
 * @param {Array} allowedTypes - 允许的文件类型
 * @returns {boolean}
 */
export function validateFileType(file, allowedTypes) {
  return allowedTypes.includes(file.type)
}
