// 服务器启动文件

const app = require('./main')
const config = require('../config/config.ini')

const PORT = process.env.PORT || config.server.port || 8080
const HOST = process.env.HOST || config.server.host || 'localhost'

app.listen(PORT, HOST, () => {
  console.log(`🚀 服务器启动成功！`)
  console.log(`📍 地址: http://${HOST}:${PORT}`)
  console.log(`🌍 环境: ${process.env.NODE_ENV || 'development'}`)
  console.log(`⏰ 时间: ${new Date().toLocaleString()}`)
})

// 优雅关闭
process.on('SIGTERM', () => {
  console.log('收到 SIGTERM 信号，正在关闭服务器...')
  process.exit(0)
})

process.on('SIGINT', () => {
  console.log('收到 SIGINT 信号，正在关闭服务器...')
  process.exit(0)
})
