// 数组工具函数

/**
 * 数组去重
 * @param {Array} arr - 数组
 * @returns {Array}
 */
export function unique(arr) {
  return [...new Set(arr)]
}

/**
 * 数组分组
 * @param {Array} arr - 数组
 * @param {string|Function} key - 分组键或函数
 * @returns {Object}
 */
export function groupBy(arr, key) {
  return arr.reduce((groups, item) => {
    const groupKey = typeof key === 'function' ? key(item) : item[key]
    if (!groups[groupKey]) {
      groups[groupKey] = []
    }
    groups[groupKey].push(item)
    return groups
  }, {})
}

/**
 * 数组排序
 * @param {Array} arr - 数组
 * @param {string} key - 排序键
 * @param {string} order - 排序方向 ('asc' | 'desc')
 * @returns {Array}
 */
export function sortBy(arr, key, order = 'asc') {
  return [...arr].sort((a, b) => {
    const aVal = a[key]
    const bVal = b[key]
    
    if (order === 'desc') {
      return bVal > aVal ? 1 : bVal < aVal ? -1 : 0
    }
    return aVal > bVal ? 1 : aVal < bVal ? -1 : 0
  })
}

/**
 * 数组分块
 * @param {Array} arr - 数组
 * @param {number} size - 块大小
 * @returns {Array}
 */
export function chunk(arr, size) {
  const chunks = []
  for (let i = 0; i < arr.length; i += size) {
    chunks.push(arr.slice(i, i + size))
  }
  return chunks
}

/**
 * 数组扁平化
 * @param {Array} arr - 数组
 * @param {number} depth - 扁平化深度
 * @returns {Array}
 */
export function flatten(arr, depth = Infinity) {
  return arr.flat(depth)
}

/**
 * 数组交集
 * @param {...Array} arrays - 数组列表
 * @returns {Array}
 */
export function intersection(...arrays) {
  return arrays.reduce((acc, arr) => 
    acc.filter(item => arr.includes(item))
  )
}

/**
 * 数组并集
 * @param {...Array} arrays - 数组列表
 * @returns {Array}
 */
export function union(...arrays) {
  return unique(flatten(arrays))
}

/**
 * 数组差集
 * @param {Array} arr1 - 数组1
 * @param {Array} arr2 - 数组2
 * @returns {Array}
 */
export function difference(arr1, arr2) {
  return arr1.filter(item => !arr2.includes(item))
}

/**
 * 随机打乱数组
 * @param {Array} arr - 数组
 * @returns {Array}
 */
export function shuffle(arr) {
  const shuffled = [...arr]
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]]
  }
  return shuffled
}

/**
 * 获取数组随机元素
 * @param {Array} arr - 数组
 * @returns {*}
 */
export function randomItem(arr) {
  return arr[Math.floor(Math.random() * arr.length)]
}

/**
 * 数组求和
 * @param {Array} arr - 数组
 * @param {string|Function} key - 求和键或函数
 * @returns {number}
 */
export function sum(arr, key) {
  return arr.reduce((total, item) => {
    const value = typeof key === 'function' ? key(item) : item[key]
    return total + (Number(value) || 0)
  }, 0)
}
