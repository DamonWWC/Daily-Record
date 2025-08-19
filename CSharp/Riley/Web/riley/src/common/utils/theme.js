// 主题工具函数
import { THEME, STORAGE_KEYS } from '@/constants'
import { getLocalStorage, setLocalStorage } from './storage'

/**
 * 获取当前主题
 * @returns {string} 主题名称
 */
export function getCurrentTheme() {
  return getLocalStorage(STORAGE_KEYS.THEME, THEME.LIGHT)
}

/**
 * 设置主题
 * @param {string} theme - 主题名称
 */
export function setTheme(theme) {
  if (!Object.values(THEME).includes(theme)) {
    console.warn('无效的主题名称:', theme)
    return
  }
  
  setLocalStorage(STORAGE_KEYS.THEME, theme)
  applyTheme(theme)
}

/**
 * 应用主题
 * @param {string} theme - 主题名称
 */
export function applyTheme(theme) {
  const html = document.documentElement
  
  if (theme === THEME.DARK) {
    html.setAttribute('data-theme', 'dark')
    html.classList.add('dark')
  } else {
    html.removeAttribute('data-theme')
    html.classList.remove('dark')
  }
}

/**
 * 切换主题
 */
export function toggleTheme() {
  const currentTheme = getCurrentTheme()
  const newTheme = currentTheme === THEME.LIGHT ? THEME.DARK : THEME.LIGHT
  setTheme(newTheme)
}

/**
 * 初始化主题
 */
export function initTheme() {
  const theme = getCurrentTheme()
  applyTheme(theme)
}

/**
 * 监听系统主题变化
 */
export function watchSystemTheme() {
  if (window.matchMedia) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    
    const handleChange = (e) => {
      const systemTheme = e.matches ? THEME.DARK : THEME.LIGHT
      const currentTheme = getCurrentTheme()
      
      // 如果用户没有手动设置主题，则跟随系统
      if (currentTheme === THEME.LIGHT || currentTheme === THEME.DARK) {
        setTheme(systemTheme)
      }
    }
    
    mediaQuery.addEventListener('change', handleChange)
    
    // 初始检查
    handleChange(mediaQuery)
  }
}
