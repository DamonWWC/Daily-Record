# Riley Vue.js 项目

这是一个基于 Vue 3 的现代化前端项目，严格按照 `rileyrule.mdc` 中的规范构建。

## 🚀 技术栈

- **Vue 3** - 使用 Composition API
- **Vite** - 现代化构建工具
- **Pinia** - 状态管理
- **Vue Router** - 路由管理
- **Element Plus** - UI 组件库
- **ECharts** - 图表库
- **Sass** - CSS 预处理器
- **Axios** - HTTP 客户端

## 📁 项目结构

```
src/
├── app/                    # 应用主体目录
│   ├── api/               # API 接口目录
│   ├── router/            # 路由配置
│   ├── store/             # Pinia 状态管理
│   ├── views/             # 页面组件
│   │   ├── example/       # 示例页面
│   │   ├── exception/     # 异常页面
│   │   └── iframe/        # 内嵌页面
│   └── views/             # 页面组件
├── common/                 # 公共模块
│   ├── components/        # 公共组件
│   ├── directives/        # Vue 指令
│   └── utils/             # 工具函数
├── constants/              # 常量定义
├── portal/                 # 门户页面
├── styles/                 # 样式文件
│   ├── global/            # 全局样式
│   ├── styles/            # 项目样式
│   └── theme/             # 主题样式
└── main.js                # 应用入口
```

## 🛠️ 开发环境

### 安装依赖
```bash
npm install
```

### 启动开发服务器
```bash
npm run dev
```

开发服务器将在 http://127.0.0.1:8080/ 上运行

### 构建生产版本
```bash
npm run build
```

### 预览生产版本
```bash
npm run preview
```

## 📱 功能特性

- ✅ Vue 3 Composition API
- ✅ Element Plus 组件库集成
- ✅ ECharts 图表支持
- ✅ SCSS 样式预处理器
- ✅ 响应式设计
- ✅ 主题切换支持
- ✅ 路由管理
- ✅ 状态管理
- ✅ 工具函数库
- ✅ 异常页面处理

## 🎯 页面说明

- **首页** (`/`) - 展示示例页面内容
- **关于** (`/about`) - 关于页面
- **示例** (`/example`) - Element Plus 组件和 ECharts 图表示例
- **404页面** - 自定义的"即将到来"页面

## 🔧 配置说明

### Vite 配置
- 端口：8080
- 自动打开浏览器
- SCSS 全局样式注入
- 路径别名：`@` 指向 `src/` 目录

### 样式系统
- 使用 SCSS 变量和混入
- 支持明暗主题切换
- Element Plus 样式定制
- BEM 命名规范

## 📝 开发规范

- 组件命名：PascalCase
- 文件结构：严格遵循 template > script > style 顺序
- 样式：使用 scoped 和 BEM 规范
- 函数声明：使用 function 关键字
- 响应式：优先使用 ref，复杂对象使用 reactive

## 🚧 注意事项

当前版本存在一些 SCSS 警告（关于过时的 @import 语法），这些不会影响应用运行，但建议在后续版本中更新为现代的 @use 语法。

## �� 许可证

MIT License
