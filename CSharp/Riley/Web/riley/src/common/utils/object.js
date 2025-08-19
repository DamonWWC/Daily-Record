// 对象工具函数

/**
 * 深拷贝对象
 * @param {*} obj - 要拷贝的对象
 * @returns {*}
 */
export function deepClone(obj) {
  if (obj === null || typeof obj !== 'object') {
    return obj
  }
  
  if (obj instanceof Date) {
    return new Date(obj.getTime())
  }
  
  if (obj instanceof Array) {
    return obj.map(item => deepClone(item))
  }
  
  if (typeof obj === 'object') {
    const cloned = {}
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        cloned[key] = deepClone(obj[key])
      }
    }
    return cloned
  }
  
  return obj
}

/**
 * 合并对象
 * @param {...Object} objects - 对象列表
 * @returns {Object}
 */
export function merge(...objects) {
  return objects.reduce((result, obj) => {
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        if (typeof obj[key] === 'object' && obj[key] !== null && !Array.isArray(obj[key])) {
          result[key] = merge(result[key] || {}, obj[key])
        } else {
          result[key] = obj[key]
        }
      }
    }
    return result
  }, {})
}

/**
 * 获取对象嵌套属性值
 * @param {Object} obj - 对象
 * @param {string} path - 属性路径
 * @param {*} defaultValue - 默认值
 * @returns {*}
 */
export function get(obj, path, defaultValue = undefined) {
  const keys = path.split('.')
  let result = obj
  
  for (const key of keys) {
    if (result === null || result === undefined) {
      return defaultValue
    }
    result = result[key]
  }
  
  return result === undefined ? defaultValue : result
}

/**
 * 设置对象嵌套属性值
 * @param {Object} obj - 对象
 * @param {string} path - 属性路径
 * @param {*} value - 值
 * @returns {Object}
 */
export function set(obj, path, value) {
  const keys = path.split('.')
  const result = { ...obj }
  let current = result
  
  for (let i = 0; i < keys.length - 1; i++) {
    const key = keys[i]
    if (!(key in current) || typeof current[key] !== 'object') {
      current[key] = {}
    }
    current = current[key]
  }
  
  current[keys[keys.length - 1]] = value
  return result
}

/**
 * 移除对象属性
 * @param {Object} obj - 对象
 * @param {string|Array} keys - 要移除的属性
 * @returns {Object}
 */
export function omit(obj, keys) {
  const keyArray = Array.isArray(keys) ? keys : [keys]
  const result = {}
  
  for (const key in obj) {
    if (obj.hasOwnProperty(key) && !keyArray.includes(key)) {
      result[key] = obj[key]
    }
  }
  
  return result
}

/**
 * 选择对象属性
 * @param {Object} obj - 对象
 * @param {string|Array} keys - 要选择的属性
 * @returns {Object}
 */
export function pick(obj, keys) {
  const keyArray = Array.isArray(keys) ? keys : [keys]
  const result = {}
  
  for (const key of keyArray) {
    if (key in obj) {
      result[key] = obj[key]
    }
  }
  
  return result
}

/**
 * 对象转查询字符串
 * @param {Object} obj - 对象
 * @returns {string}
 */
export function toQueryString(obj) {
  return Object.keys(obj)
    .filter(key => obj[key] !== null && obj[key] !== undefined)
    .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(obj[key])}`)
    .join('&')
}

/**
 * 查询字符串转对象
 * @param {string} queryString - 查询字符串
 * @returns {Object}
 */
export function fromQueryString(queryString) {
  const result = {}
  const params = new URLSearchParams(queryString)
  
  for (const [key, value] of params) {
    result[key] = value
  }
  
  return result
}

/**
 * 判断对象是否为空
 * @param {Object} obj - 对象
 * @returns {boolean}
 */
export function isEmpty(obj) {
  if (obj === null || obj === undefined) {
    return true
  }
  
  if (typeof obj === 'string' || Array.isArray(obj)) {
    return obj.length === 0
  }
  
  if (typeof obj === 'object') {
    return Object.keys(obj).length === 0
  }
  
  return false
}
