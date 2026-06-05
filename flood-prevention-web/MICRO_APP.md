# 微前端

## 文档

-   [qiankun 文档](https://qiankun.umijs.org/zh/guide)
-   [@ecp/uni-plugin](http://172.25.20.65:4873/-/web/detail/@ecp/uni-plugin)
-   [MicroApp、MicroUtils](http://172.25.20.65:8090/#/utils/micro-app)

## 目录

-   [实践详解](#实践详解)
    -   [1-构建](#1-构建)
    -   [2-运行](#2-运行)
    -   [3-部署](#3-部署)
-   [接入指南](#接入指南)
    -   [1-子应用接入](#1-子应用接入)
    -   [2-主应用接入](#2-主应用接入)
    -   [3-手动加载子应用](#3-手动加载子应用)
-   [问题汇总](#问题汇总)

> 在 flood-prevention-web 模板中，**yarn serve:uni** 或 **yarn build:uni** 进行微前端的本地开发和构建

### 实践详解

微前端主要相关的目录如下：

```md
|-- config （部署服务的配置）
|-- server-config.js （部署服务的配置文件）
...
|-- public
|-- index.html （普通应用或子应用入口）
|-- portal.html （主应用入口）
|-- portal-iframe.html （主应用下的 iframe 入口）
|-- src
|-- app （普通应用或子应用）
|-- portal （主应用）
|-- portal.js （主应用入口）
|-- portal.vue （主应用入口组件）
|-- App.vue （普通应用或子应用的入口组件）
|-- main.js （普通应用或子应用的入口）
...
|-- vue.config.js
|-- package.json
...
```

> **注意：如果需要启用主应用，需要设置 vue.config.js 的 usePortal 为 true**

---

步骤：

#### 1-构建

> 结合@ecp/uni-plugin

**_需要确保 package.json 的 name 是唯一的，不会与其他项目冲突_**

构建主要在 vue.config.js 做以下改造：

-   <span id="pages">**入口（pages）**</span>

主应用和子应用可以共存于同一个项目中，通过 vue.config.js 中 pages 配置多个入口实现；

构建时，会将 protal.html 重命名为 index.html，将原来的 index.html 重命名为 sub.html。

-   <span id="publicPath">**资源路径前缀（publicPath）**</span>

所有资源都以 _项目名_（package.json 的 name，下同）作为统一前缀

> 项目名不能以 /^(\/?[a-z]\d\*-)/ 开头

```js
if (useSubModule) {
    ...
    if (process.env.NODE_ENV === 'production' || !usePortal) {
        // 本地调试时，不修改publicPath，通过代理的方式重写当前应用的sub入口
        publicPath = `/${packageName}/`;
    }
}
```

> 注意：在启用主应用 portal 的开发模式下，统一前缀会使调试不方便，所以没有使用，并且需要在 devServer 添加配置：

```js
// 由于没有统一前缀，所以需要添加重定向才能获取到自己的sub.html
before (app) {
    if (usePortal) {
        app.get(`/${packageName}/sub.html`, function (req, res) {
            res.redirect('/sub.html');
        });
    }
}
```

-   <span id="output">**输出（output）**</span>

qiankun 规定，需要以 umd 的方式构建输出

```js
output = {
    library: packageName,
    libraryTarget: "umd",
    jsonpFunction: `webpackJsonp_${packageName}`
};
```

> 如果项目中有库使用了 amd 或者 cmd，可能会导致 umd 无法正常使用

-   <span id="externals">**externals**</span>

~~目前将 vue、vue-router、vuex、lodash 做了 externals 处理，主要是为了解决 vue-router 的重复加载问题，以及减少子应用加载的体积~~

> ~~externals 解决了一些问题，但是也引入了不少问题：~~
>
> > ~~1. \$api 等 vue 原型下的变量覆盖问题，虽然对常见的变量（\$api/\$utils/\$constant/\$constants）做了缓存切换处理，但是后续添加的变量都需要手动处理（详解 @ecp/ecp-ui 的 MicroApp）~~ >> ~~2. 由于 externals 所以开发了@ecp/uni-plugin，并且引入了 sub.html 作为子应用的入口，如果不使用 externals，就可以不用@ecp/uni-plugin 和 sub.html 了~~

目前仅 lodash 使用了 externals

-   vue 因为实例覆盖剔除出 externals
-   vue-router 因为重定义问题剔除出 externals

#### 2-运行

> 结合 @ecp/ecp-ui 的 MicroApp、MicroUtils，根据目前的微前端实践封装了 qiankun 的使用方式

-   **菜单**

对菜单的 url 格式是有要求的，必须以 _应用名+页面路由_，如：/flood-prevention-web#/Page1

由于我们用的是代理的形式加载子应用，如果我们直接以上面的格式访问，就相当于直接通过代理访问子应用的页面了，根本访问不了主应用。

为了避免这个问题，同时为了解决有菜单组的情况（如可以在 应用中心 和 控制台 切换），在主应用接收到菜单列表时，统一对菜单 url 做格式化处理，在 url 前面加上标识（如 /s-）。

标识统一格式为单字母或单字母加数字，由于统一了格式，所以我们依然可以从 url 解析出应用名，从而加载对应的子应用。

> 第一个有效菜单，作为*默认路由*。

-   **主应用加载**

MicroApp 从菜单里面提取和组装子应用列表，qiankun 会根据应用列表匹配和加载对应的子应用

qiankun 启动进行初始化，然后 HeadCache 在子应用加载前会将\<head>里面的元素都加上 _data-symbol="portal"_ 的标记，后面子应用加载时，所有插入\<head>里面的元素都会做缓存处理。

-   **子应用加载**

主应用加载完后，如果当前页面 url 不能从注册列表里面匹配到子应用，会加载默认路由，否则就加载匹配到的子应用。

子应用加载前会做两个缓存处理：

(1) Vue 的原型属性：目前会将 \$api/\$utils/\$constant/\$constants 做缓存处理。

子应用加载时，会先尝试从缓存中取值，如果没值会将当前值缓存，如果有值则返回缓存值，即缓存在子应用首次加载时才会写入值。

> 缓存原因：使用了 external，导致不管主应用还是子应用都共用同一个全局 Vue，所以手动实现一个缓存解决子应用间切换的问题

(2) \<head> 里面追加的元素：子应用加载时尝试加载已经缓存的元素，在子应用卸载时缓存没有标记 _data-symbol="portal"_ 的元素，然后清除他们。

> 缓存原因：子应用加载时会有一些元素（如懒加载的路由样式、js）追加到\<head>，如果不处理在加载其他子应用时会有样式冲突等问题存在。

-   **子应用二次加载**

子应用再次加载时，子应用入口的初始化代码不会再执行了，而是直接触发 mount 生命周期了。

除了上面提到的缓存问题外，还有需要注意 @ecp/ecp-ui、common 全局公共组件、工具等，在 use 时需要更新引用。

这个也是 external 引起的问题，通过更新引用重新 use，处理 @ecp/ecp-ui 和 common 被覆盖的问题。

```js
export async function mount (props) {
    Vue.use({ ...EcpUI });
    Vue.use({ ...CommonPart });
    ...
}
```

> 即子应用只 load 一次，后面都是触发 mount、unmount 等生命周期

#### 3-部署

> 结合 uni-server

-   **配置**

uni-server 适配了通过代理的微前端模式，所以在配置上（config/server-config.js）有几点需要注意的：

(1) _SERVER_CONFIG.APP_NAME_ 必须和 _package.json_ 的 _name_ 保持一致，因为会使用 APP_NAME 作为虚拟路径指向 dist 目录，从而处理微前端的静态资源代理。

> SERVER_CONFIG.APP_ALIAS 与 SERVER_CONFIG.APP_NAME 有同样效果

(2) APPS_ENTRY 为主应用需要的子应用列表，uni-server 会根据列表的配置，添加到 proxy 代理列表里面，主应用就可以根据代理访问不同子应用的静态资源，而不需要配置 ip 端口了。

> [qiankun - 如何部署](https://qiankun.umijs.org/zh/cookbook#场景2：主应用和微应用部署在不同的服务器，使用-nginx-代理访问)

(3) NACOS_CONFIG.registerService 是否注册子应用到 nacos，建议设置为 true，这样主应用就可以根据 nacos 注册列表自动生成代理了。

-   **打包**

uni-server 和 nodejs 安装包等都是放到 ftp 下，如果项目没有特殊改造，直接用下面的模板配置在 jenkins 上即可

```bash
yarn
yarn build:uni

name=`node -pe "require('./package.json').name"`

rm -rf ./${name} && mkdir ${name}

# ftp获取相关资源文件
rm -rf ./bin
mkdir ./bin
lftp jenkins:jks123@172.25.21.47 <<EOF
set ssl:verify-certificate no
get -c /前端共享/前端部署资源/centos/uni-server/node_modules.tar
get -c /前端共享/前端部署资源/centos/nodejs-plugin.tar.gz
get -c /前端共享/前端部署资源/centos/uni-server/bin/run.sh -o ./bin/run.sh
get -c /前端共享/前端部署资源/centos/uni-server/bin/healthcheck.sh -o ./bin/healthcheck.sh
bye
EOF

mv dist config server bin node_modules.tar nodejs-plugin.tar.gz ./${name}

tar -zvcf ${name}.tar.gz ${name}
```

-   **部署**

部署就很简单了，解压后执行**sh bin/run.sh start**就可以了，停止的话执行**sh bin/run.sh stop**。

### 接入指南

> 如果是现有项目的微前端改造，由于改造的基础不一致，以下文档只能列出改造的重点，如果改造中出现问题，请对比 flood-prevention-web 和上面对为微前端的讲解分析问题。

#### 1-子应用接入

-   **package.json**

添加 uni 开发和构建指令：

```
...
"scripts": {
    ...
    "serve:uni": "vue-cli-service serve --uni",
    "build:uni": "vue-cli-service build --uni",
},
```

-   **vue.config.js**

(1) packageName 作为子应用的唯一标识

```js
const packageConfig = require("./package.json");
const packageName = packageConfig.name; // 使用package.json的name
```

(2) useMicroApp 启用微前端模式

```js
const useMicroApp = !!process.argv.find(d => d === "--uni"); // 启用微前端模式
```

(3) pages 入口改造为 sub.html

```js
let pages = {
    index: {
        entry: "src/main.js",
        template: "public/index.html",
        filename: useMicroApp ? "sub.html" : "index.html"
    }
};
```

(4) output、[externals](#externals)、publicPath

```js
if (useMicroApp) {
    output = {
        library: packageName,
        libraryTarget: "umd",
        jsonpFunction: `webpackJsonp_${packageName}`
    };
    externals = {
        loadsh: {
            commonjs: "lodash",
            amd: "lodash",
            root: "_" // 指向全局变量
        }
    };
    if (usePortal) {
        pages.sub = {
            entry: "src/main.js",
            template: "public/index.html",
            filename: "sub.html"
        };
        pages.index = {
            entry: "src/portal/portal.js",
            template: "public/portal.html",
            filename: "index.html"
        };
        /**
         * // 如确定主应用没有使用 loadMicroApp 手动加载子应用，则可保留 externals，并需要嵌入公服或其它未剔除 externals 的应用，请放开这段
         * // 否则主应用与子应用应移除 vue、vuex、vue-router 等 externals 处理
         * // 剔除了 externals 的主应用，可使用代理+匹配上下文标识替换，并使用 iframe 嵌入未剔除 externals 的应用
        externals = {
            ...externals,
            vue: 'Vue',
            'vue-router': 'VueRouter',
            vuex: 'Vuex',
            axios: 'axios'
        };
        */
    }
    if (process.env.NODE_ENV === "production" || !usePortal) {
        // 本地调试时，不修改publicPath，通过代理的方式重写当前应用的sub入口
        publicPath = `/${packageName}/`;
    }
}
```

> externals：由于前端的设计使用了 externals，虽然由此引入了一些问题，但目前也通过一些手段规避了，目前暂时保留 externals 的使用

(5) 导出配置（仅列出需要改造的要点）

```js
module.exports = {
    pages,
    publicPath: publicPath,
    ...
    chainWebpack: config => {
        ...
        // 图片和文字文件处理时，设置esModule=false
        ['images', 'fonts'].forEach(t => {
            config.module
                .rule(t)
                .use('url-loader')
                .loader('url-loader')
                .tap(options => ({
                    ...options,
                    esModule: false // file-loader打包图片文件时路径错误输出为[object-module]的解决方法：https://www.jb51.net/article/177740.htm
                }));
        });

        if (process.env.NODE_ENV === 'production') {
            config.plugin('compression').use(CompressionWebpackPlugin, [
                {
                    test: /\.js$|\.css$/,
                    algorithm: 'gzip',
                    threshold: 1024 * 512
                }
            ]);

            // FIX: 微应用下，图片等资源的路径使用 publicPath 无效的问题
            const configFileLoader = type => {
                if (type === 'svg') {
                    config.module
                        .rule('svg')
                        .use('file-loader')
                        .loader('file-loader')
                        .tap(options => ({
                            ...options,
                            publicPath,
                            esModule: false
                        }));
                    return;
                }

                config.module
                    .rule(type)
                    .use('url-loader')
                    .loader('url-loader')
                    .tap(options => ({
                        ...options,
                        fallback: {
                            ...options.fallback,
                            options: {
                                ...options.fallback.options,
                                publicPath,
                                esModule: false
                            }
                        }
                    }));
            };
            useMicroApp &&
                ['media', 'fonts', 'images', 'svg'].forEach(d =>
                    configFileLoader(d)
                );
        }

        // 修复路由懒加载不生效的bug
        config.plugins.delete('prefetch');
        Object.keys(pages).forEach(pageKey =>
            config.plugins.delete(`prefetch-${pageKey}`)
        );
        config.plugins.delete('preload');
        Object.keys(pages).forEach(pageKey =>
            config.plugins.delete(`preload-${pageKey}`)
        );
    },
    devServer: {
        host: '0.0.0.0',
        ...
        publicPath: publicPath,
    },
    configureWebpack: config => {
        return {
            output: output,
            externals: externals,
            ...
            plugins: [
                ...
                new UniPlugin({
                    enabled: !!useMicroApp,
                    replaceIndex: !usePortal,
                    externals: Object.keys(externals).map(item =>
                        item === 'loadsh' ? 'lodash' : item
                    )
                }),
            ]
        };
    }
}
```

-   **mian.js**

导出 qiankun 的生命周期

```js
...
if (!window.__POWERED_BY_QIANKUN__) {
    Vue.use(EcpUI);
    Vue.use(CommonPart);

    new Vue({
        router,
        store,
        render: h => h(App)
    }).$mount('#app');
}

let instance = null;
export async function bootstrap () {
    // console.log('flood-prevention-web bootstraped');
}

export async function mount (props) {
    // console.log('flood-prevention-web props from main framework', props);
    Vue.use({ ...EcpUI });
    Vue.use({ ...CommonPart });

    instance = new Vue({
        router,
        store,
        el: props.container ? props.container.querySelector('#app') : '#app',
        render: h => h(App)
    });
}

export async function unmount () {
    // 必须确保 有实例 且 有 $destroy 才能调用销毁, 否则会把主子应用整个qiankun拉宕掉
    if (instance) {
        instance?.$destroy && instance.$destroy();
        instance = null;
    }
}
```

-   **router.js**

作为子模块不能添加默认路由，否则在主应用点击菜单切换*子应用 B*时，路由会首先被重写为*子应用 A*的默认路由。

```js
...
const ViewRoutes = [
    ExampleRoute('/'),
    {
        path: '*', redirect: '/Page1'
    }
];

// 作为子模块不能添加默认路由
if (window.__POWERED_BY_QIANKUN__) {
    ViewRoutes.pop();
}

export default new Router({
    routes: ViewRoutes
});
```

-   **App.vue**

入口改造，作为子应用时，不显示导航栏（也可以在 layout 加里判断）

```html
<template>
  <div id="app">
    <app-layout name="flood-prevention-web" :menu="menu" v-if="showNav">
        <template #content>
        <router-view></router-view>
        </template>
    </app-layout>
    <template v-else>
        <router-view></router-view>
    </template>
  </div>
</template>

<script>
export default {
    name: 'app',
    computed: {
        showNav () {
            const isSubMode = window.__POWERED_BY_QIANKUN__; // 子模块
            return !isSubMode;
        },
    },
    data () {
        return {

        };
    },
};
</script>
```

#### 2-主应用接入

-   **入口**

增加了 public/protal.html 和 portal-iframe.html，并增加了 src/portal 目录

如果是普通的主应用，直接使用 flood-prevention-web 提供的 protal 即可，否则需要看情况改造 portal.js 和 portal.vue 了。

-   **vue.config.js**

在子应用基础上添加主应用判断，需要调整的内容如下：

```js
const usePortal = true; // 如果不需要作为主应用，请设为false

if (useMicroApp) {
    if (usePortal) {
        pages.sub = {
            entry: 'src/main.js',
            template: 'public/index.html',
            filename: 'sub.html'
        };
        pages.index = {
            entry: 'src/portal/portal.js',
            template: 'public/portal.html',
            filename: 'index.html'
        };
    }
    ...
    if (process.env.NODE_ENV === 'production' || !usePortal) {
        // 本地调试时，不修改publicPath，通过代理的方式重写当前应用的sub入口
        publicPath = `/${packageName}/`;
    }
}

module.exports = {
    ...
    devServer: {
        ...
        before (app) {
            if (usePortal) {
                app.get(`/${packageName}/sub.html`, function (req, res) {
                    res.redirect('/sub.html');
                });
            }
        }
    },
    configureWebpack: config => {
        return {
            plugins: [
                ...
                new UniPlugin({ enabled: !!useMicroApp, replaceIndex: !usePortal }),
            ]
        }
    }
}
```

> ##### 注意：如果在本地需要作为子应用被嵌套，需要设置*usePortal*为 false，否则改造主应用后，在本地不能作为子应用被其他应用使用

#### 3-手动加载子应用

qiankun 提供了[loadMicroApp](https://qiankun.umijs.org/zh/api#loadmicroappapp-configuration)的 api 手动加载子应用，这里就提供一种使用这个 api 的应用方式：

> [注意事项](https://qiankun.umijs.org/zh/cookbook#%E5%90%8C%E6%97%B6%E5%AD%98%E5%9C%A8%E5%A4%9A%E4%B8%AA%E5%BE%AE%E5%BA%94%E7%94%A8%E6%97%B6)

##### 加载（容器）

(1) 子应用加载
方式一: 加载组件，通过 props 传入组件名和参数

```js
this.microApp = MicroUtils.loadMicroApp({
    name: "flood-prevention-web",
    entry: "/flood-prevention-web/sub.html",
    container: "#sub-load-wrapper",
    props: {
        componentName: "Menu3", // 需要加载的组件名
        componentProps: {
            // 传入的参数
            detailInfo: {
                name: this.name
            },
            callback: this.handleConfirm // 传回当前子应用的回调
        }
    }
});
```

方式二: 加载注册到子应用 router 上的页面(或组件)，通过 path 传入路径 query, 通过 props 传入参数

> 使用 app-micro-component 组件

```html
<app-micro-component :target-path="page.path" :target-props="page.props" @render-error="handleRenderError" @render-complate="handleRenderComplate" ref="microAppComponent" :key="page.path" />
<script>
    export default {
        data() {
            return {
                page: {
                    path: this.$utils.MicroUtils.formatRoute(
                    "/flood-prevention-web#/menu3?loadAs=Route-Component",
                    prefix // /s- 之类的匹配前缀
                );, // 可以在路径上加 query 参数, 子应用页面通过 $attrs.query 获取
                    // 页面组件props
                    props: {
                        detailInfo: {
                            name: this.name
                        },
                        callback: this.handleConfirm
                    }
                }
            };
        }
    };
</script>
```

(2) 获取加载状态

可以根据 this.microApp.loadPromise 获取当前的加载状态，加载完会执行 resolve。

```js
this.microAppLoading = true;
this.microApp.loadPromise.finally(d => {
    this.microAppLoading = false;
});
```

(3) 卸载子应用

如果不需要加载的子应用了，可以卸载掉

```js
this.microApp.unmount();
```

##### 被加载（子应用的子应用）

(1) 接收 props 和挂载点优化

在子应用的入口 main.js 改造 mount 钩子函数

```js
export async function mount(props) {
    // console.log('flood-prevention-web props from main framework', props);
    Vue.use({ ...EcpUI });
    Vue.use({ ...CommonPart });

    initApp(props.componentName && props.container).then(() => {
        instance = new Vue({
            router,
            store,
            // 为了避免根 id #app 与其他的 DOM 冲突，需要限制查找范围
            el: props.container
                ? props.container.querySelector("#app")
                : "#app",
            render: h =>
                h(App, {
                    props: {
                        // 接收参数并传入入口组件
                        componentName: props.componentName,
                        componentProps: props.componentProps
                    }
                })
        });
    });
}
```

(2) 入口组件接收参数并渲染组件

App.vue 示例：

```html
<template>
  <div id="app">
    <template v-if="!componentName">
        <router-view></router-view>
        ...
    </template>

    <template v-else>
        <!-- (对应主应用使用的app-micro-component组件) 如果是路由页面作为组件加载，则使用router的matcher匹配路由对应component渲染 (以斜杠为标识, 规范的组件名是没有斜杠的) -->
        <app-route-component :target-path="componentName" v-bind="{targetProps: componentProps}" v-if="isRoute" />

        <!-- 其它组件组要在本文件内引入，或使用其它全局组件 -->
        <component :is="componentName" v-bind="componentProps"  v-else/>
    </template>
  </div>
</template>

<script>
import Menu3 from './app/views/example/menu3/menu3.vue';

export default {
    name: 'app',
    components: {
        Menu3
    },
    props: {
        componentName: {
            type: String,
            default: ''
        },
        componentProps: {
            type: Object,
            default: () => ({})
        }
    },
    ...
}
```

(3) 触发回调

menu3.vue 的示例

```js
export default {
    name: 'menu3',
    props: {
        detailInfo: {
            type: Object,
            default: () => ({})
        },
        callback: {
            type: Function,
            default: () => {}
        }
    },
    ...
    computed: {
        loadAs () {
            if (this.$route.path.match(/menu3/i)) {
                return 'Route';
            }
            return this?.$attrs?.query?.loadAs
                ? this.$attrs.query.loadAs
                : 'Component';
        }
    },
    methods: {
        handleConfirm () {
            if (window.__POWERED_BY_QIANKUN__) {
                typeof this.callback === 'function' && this.callback('confirm', this.form);
            }
        }
    }
};
```

### 问题汇总

#### 1. Uncaught TypeError: Cannot assign to read only property 'exports' of object '#\<Object>'

A: 需要在 babel.config.js 添加 **sourceType: 'unambiguous', //  严格区分 import  和  require  不可混用**

```js
module.exports = {
    presets: [
        ...
    ],
    sourceType: 'unambiguous', // 严格区分import 和 require 不可混用
    'plugins': [
        ...
    ]
};
```

#### 2. 子应用显示的 svg 图标与独立访问的样式不一致？

A: iconfonts 图标重名后被覆盖了，目前暂时没有好的方式处理 iconfonts 的覆盖问题，目前只能通过改图标名处理了。

#### 3. 主应用正常加载，切换子应用时子应用显示空白，但是控制台没有报错？

A: 基本上有以下两种情况：

(1) 菜单 url 有误，路由部分写错了，或者路由更新了但是菜单 url 没有更新

(2) 菜单 url 没错，但是点击菜单后，浏览器显示的 url 路由部分错了

这个问题基本上是有子应用没有去掉默认路由，导致应用切换路由被覆盖

如果能从路由看出归属于哪个应用，那直接改对应的子应用即可；

如果分辨不出，就需要看出现此问题前是访问了哪个 url，再找到对应的子应用修改了。

#### 4. Application died in status LOADING_SOURCE_CODE: You need to export the functional lifecycles in xxx entry

A: 没有获取到子应用的钩子函数，先检查子应用入口 sub.html，看是否返回正确（如：sub.html 里面使用的资源路径是否为*项目名*）、检查服务器的部署包是否有 sub.html

如果没有问题，可以按 [qiankun 文档](https://qiankun.umijs.org/zh/faq#application-died-in-status-loading_source_code-you-need-to-export-the-functional-lifecycles-in-xxx-entry) 排查

#### 5. Uncaught Error: application 'xxx' died in status LOADING_SOURCE_CODE: [qiankun] Target container with \#sub-wrapper not existed while xxx loading

A: 一般情况下是主应用出错导致的，看下控制台的异常，修复一下

如果没有问题，可以按 [qiankun 文档](https://qiankun.umijs.org/zh/faq#application-died-in-status-not_mounted-target-container-with-container-not-existed-while-xxx-loading) 排查

#### 6. 其他问题可以尝试在 qiankun 文档里找找

> [qiankun-常见问题](https://qiankun.umijs.org/zh/faq)
