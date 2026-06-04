// Vite代理配置文件

// 环境变量配置
const env = {
  development: {
    VITE_API_BASE_URL: 'http://localhost:3000',
    VITE_STATIC_BASE_URL: 'http://localhost:3000',
    VITE_WS_BASE_URL: 'ws://localhost:3000'
  },
  production: {
    VITE_API_BASE_URL: 'https://api.riley.com',
    VITE_STATIC_BASE_URL: 'https://static.riley.com',
    VITE_WS_BASE_URL: 'wss://ws.riley.com'
  }
}

// 获取当前环境
const getCurrentEnv = () => {
  return process.env.NODE_ENV || 'development'
}

// 获取环境变量
const getEnvVar = (key) => {
  const currentEnv = getCurrentEnv()
  return env[currentEnv]?.[key] || env.development[key]
}

// 代理配置
export const proxyConfig = {
  // API代理
  '/api': {
    target: getEnvVar('VITE_API_BASE_URL'),
    changeOrigin: true,
    rewrite: (path) => path.replace(/^\/api/, ''),
    configure: (proxy, options) => {
      proxy.on('error', (err, req, res) => {
        console.log('proxy error', err)
      })
      proxy.on('proxyReq', (proxyReq, req, res) => {
        console.log('Sending Request to the Target:', req.method, req.url)
      })
      proxy.on('proxyRes', (proxyRes, req, res) => {
        console.log('Received Response from the Target:', proxyRes.statusCode, req.url)
      })
    }
  },
  // 静态资源代理
  '/static': {
    target: getEnvVar('VITE_STATIC_BASE_URL'),
    changeOrigin: true,
    rewrite: (path) => path.replace(/^\/static/, '')
  },
  // WebSocket代理
  '/ws': {
    target: getEnvVar('VITE_WS_BASE_URL'),
    changeOrigin: true,
    ws: true,
    rewrite: (path) => path.replace(/^\/ws/, '')
  }
}

export default proxyConfig
