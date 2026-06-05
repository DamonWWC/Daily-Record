# 目录结构说明

```
|-- frontend
    |-- .browserslistrc                     配置兼容浏览器
    |-- .editorconfig                       配置编辑器
    |-- .eslintrc.js                        配置eslint
    |-- .gitignore                          配置git忽略文件
    |-- .npmrc                              配置公司私有npm源
    |-- .yarnrc                             配置公司私有yarn源
    |-- COMPATIBILITY.md                    兼容性说明
    |-- MICRO_APP.md                        微应用说明
    |-- README.md                           项目说明
    |-- babel.config.js                     配置babel
    |-- jsconfig.json                       dev 环境编译器选项配置
    |-- package-lock.json                   配置依赖包锁定文件
    |-- package.json                        配置项目包信息
    |-- postcss.config.js                   配置postcss
    |-- vue.config.js                       vue-cli配置文件
    |-- vue.proxy.config.js                 vue-cli代理配置文件
    |-- yarn.lock                           配置yarn锁定文件
    |-- build-image
    |   |-- Dockerfile                      docker容器镜像生成文件
    |-- config
    |   |-- config.ini                      生产环境配置变量
    |   |-- file-api.ini                    可视化部署配置相关文件
    |   |-- proxy.ini                       生产环境代理配置
    |   |-- server-config.js                生产环境前端服务相关配置文件
    |   |-- var.ini                         生产环境变量配置文件
    |-- public                              前端项目公共资源目录
    |   |-- config.json
    |   |-- favicon.ico
    |   |-- index.html
    |   |-- portal-iframe.html
    |   |-- portal.html
    |-- server                              前端服务启动脚本
    |   |-- main.js
    |   |-- server.js
    |-- src                                 前端项目源码目录
        |-- App.vue
        |-- gloable.d.ts
        |-- main.js
        |-- plugins.js
        |-- polyfills.js
        |-- public-path.js
        |-- styles.scss                     全局样式文件
        |-- app
        |   |-- api                         前端接口目录
        |   |   |-- index.js                前端接口入口文件，各个模块统一放到this.$api中
        |   |   |-- interceptors.js         前端请求拦截器
        |   |-- router                      前端vue-router路由目录
        |   |   |-- index.js
        |   |-- store                       前端vuex存储目录
        |   |   |-- index.js
        |   |-- views                       前端视图页面目录
        |       |-- view-route.js           前端视图路由配置文件
        |       |-- industry-analysis       前端具体页面目录
        |       |   |-- index.js            页面入口
        |       |   |-- index.vue
        |       |   |-- components          页面组件
        |       |       |-- analysis.vue
        |       |       |-- download.vue
        |       |       |-- status.vue
        |-- assets                          前端静态资源目录
        |-- common                          前端通用模块目录
        |   |-- index.js
        |   |-- components                  前端通用组件目录
        |   |   |-- index.js
        |   |   |-- app-layout              布局组件
        |   |       |-- app-layout.vue
        |   |   |-- app-micro-component     根据路由手动加载子应用页面
        |   |   |-- app-route-component     路由匹配渲染组件
        |   |-- directives                  前端通用vue指令目录
        |   |   |-- index.js
        |   |-- filters                     vue filters目录
        |   |   |-- index.js
        |   |-- utils                       前端通用方法目录
        |       |-- browser                 浏览器兼容判断相关方法
        |       |-- file-utils              文件处理相关方法
        |       |-- helpers                 数据辅助处理相关方法
        |       |-- initial-utils           应用加载初始化相关方法
        |       |-- permission              鉴权相关方法
        |       |-- index.js
        |-- constants                       前端常量目录
        |   |-- mock                        本地模拟数据文件目录
        |   |-- dictionary-map.js           字典key-value 枚举
        |   |-- index.js
        |-- portal                          前端微前端 当作主应用时门户目录
        |   |-- portal-layout.vue
        |   |-- portal.js
        |   |-- portal.vue
        |   |-- config
        |       |-- apply.js
        |       |-- free.config.js
        |       |-- index.js
        |       |-- menu.config.js
        |-- theme                           前端换肤主题目录
            |-- mixin.scss                  scss 混入处理相关
            |-- variable.scss               scss 变量相关
            |-- darken
            |   |-- import.scss
            |   |-- index.scss
            |-- default
                |-- _import.scss
                |-- index.scss
```
