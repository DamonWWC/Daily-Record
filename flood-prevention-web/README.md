# flood-prevention-web

> -   flood-prevention-web 模板工程已支持微前端，[详情 ◁](./MICRO_APP.md)
> -   flood-prevention-web 已支持 IE11，兼容过程中的问题已汇总到 [兼容性问题汇总 ◁](./COMPATIBILITY.md)

## 注意事项

> -   全局样式请 `务必`按照 src/styles.scss、src/theme/mixin.scss 与 src/theme/variable.scss 提示写！
> -   非字典操作类接口，`禁止`直接调用字典接口获取字典！请通过 VueX 获取！
> -   公服管理的字典，`禁止`直接传 Kind 数字获取！请在 src/constants/dictionary-map.js 中按提示列枚举，通过枚举定义的 Key 获取字典！
> -   页面引入路由`务必`使用`import(xxx)`方式异步加载，以实现路由懒加载！
> -   项目版本 Release 时，除了打 git tag，`务必`记得把 package.json 的 version 也改了！
> -   $api不再挂载到Vue.prototype,`务必`查看plugin.js内的注释内容

## 技术栈

-   vue 全家桶: vue + vuex + vue-router + element-ui
-   @ecp/ecp-ui：基于 element-ui 封装的 [公共组件库](http://frontend.pcitech.online/@ecp/ecp-ui)
-   lodash: 部分常用方法（如类型判断等）推荐使用 [lodash](https://www.lodashjs.com/docs/latest)

## Clone 后的修改

-   修改文件夹名称为项目名，并将关键词 `flood-prevention-web` 全局替换为 `项目名`
-   将端口 `8080` 全局 `全字匹配` 替换为 `分配的应用端口号`
-   当前应用需要用作主应用时，建议将 全局组件 `app-` 前缀及文件前缀 全部替换为 `项目名-`

## 使用命令

-   安装依赖

```bash
yarn
```

-   git 子模块 (如有使用, 需要执行)

```bash
# 子模块初始化（install之后手动执行）
git submodule init && git submodule update --remote --merge

# 项目添加 git 配置
git config status.submoduleSummary true
git config submodule.recurse true
git config push.recurseSubmodules check
git config alias.sdiff "! git diff && git submodule foreach 'git diff'"

# 子模块拉更新
git pull && git pull --recurse-submodules && git submodule update --remote --merge

# 子模块提交
# cd 到具体目录之后，跟普通git项目一样操作即可
```

-   本地开发（普通模式）

```bash
yarn serve
```

-   本地开发（微前端模式）

```bash
yarn serve:uni
```

-   本地开发，代理使用 mock.proxy.config.js

> easyMock: <http://172.25.20.65:7300/> 开发时推荐使用

```bash
yarn serve:mock
```

-   构建

```bash
yarn build:uni
```

-   修复 lint 的问题

```bash
npm run lint
```

## 工程目录

```md
. 工程目录
├── README.md
├── build-image
│ └── Dockerfile 构建项目镜像的配置文件
├── config uni-server 部署插件的配置文件
│ ├── config.ini 前端相关的配置模板，提供给 uni-server 生成前端配置
│ ├── proxy.ini 前端代理配置模板，提供给 uni-server 生成前端代理
│ ├── server-config.js uni-server 部署插件的服务配置
│ └── var.ini config.ini、proxy.ini 用到的变量，如果成功订阅 nacos 配置服务，则会被 nacos 配置覆盖
├── package.json
├── public 公共资源目录
│ ├── config.json 公共配置，如果使用 uni-server 部署应用时，则会自动创建或覆盖此配置文件
│ ├── index.html 普通模式或微前端模式下子应用的页面入口
│ ├── portal-iframe.html 主应用的 iframe 页面入口
│ └── portal.html 微前端模式下主应用的页面入口
├── server
│ ├── main.js 基于 uni-server 的 node 服务端的主入口
│ └── server.js 用于 PM2 启动的入口，内部也是调用 main.js
├── src
│ ├── App.vue 普通模式的入口组件
│ ├── app
│ │ ├── api 接口请求模块
│ │ │ ├── system-service 权限相关的接口请求
│ │ │ ├── index.js 所有接口请求的入口文件
│ │ │ └── interceptors.js axios 请求库的拦截器等配置
│ │ ├── router vue-router 路由入口
│ │ ├── store vuex 的 store 和 modules 相关文件
│ │ └── views 页面组件
│ ├── assets 图片字体等资源文件
│ ├── common
│ │ ├── components 全局组件相关
│ │ │ ├── index.js 全局组件的入口文件
│ │ │ └── layout 页面布局组件
│ │ ├── directives 公共指令
│ │ ├── filters 公共过滤器
│ │ ├── index.js 通用模块入口
│ │ └── utils 通用的工具方法
│ ├── constants 公共常量
│ ├── main.js 普通模式的主入口
│ ├── plugins.js 一些插件的引用入口
│ ├── polyfills.js 一些 polifill 兼容代码的引用入口
│ ├── portal
│ │ ├── config 微前端模式下的主应用配置
│ │ ├── portal-layout.vue 微前端模式下的主应用布局组件
│ │ ├── portal.js 微前端模式下的主应用入口
│ │ └── portal.vue 微前端模式下的主应用入口组件
│ ├── styles.scss 全局样式入口
│ └── theme 主题样式目录
├── vue.config.js vue-cli 的配置文件，支持 Vue CLI 3.x 的相关配置
├── vue.proxy.config.js 设置开发服务器代理的配置文件
└── yarn.lock
```
