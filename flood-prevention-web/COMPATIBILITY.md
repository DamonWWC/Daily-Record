## 兼容性问题汇总

> #### 如果使用了@ecp/ecp-ui@1.2.x在代码中请勿使用“@ecp/ecp-ui/src/xxx”，否则需要添加@ecp/ecp-ui至编译列表(transpileDependencies)

### 1. exports相关错误

#### 1.1 SCRIPT5009: “exports”未定义

报错原因：debug这个npm包不兼容，里面用到exports.xxx，在IE中不支持

#### 1.2 Uncaught TypeError: Cannot assign to read only property 'exports' of object '#<Object>'

解决：

> 出现以上两个问题，都可以使用此解决方法

需要在babel.config.js 添加 **sourceType: 'unambiguous', // 严格区分import 和 require 不可混用**

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

### 2. SCRIPT1010: 缺少标识符

报错原因： 经排查以下这些npm包会有es6语法，需要配置编译后才可以在IE11下运行

解决：

- 如果使用的是@ecp/ecp-ui@1.1.x及以下, 在vue.config.js的transpileDependencies添加以下配置：

```js
transpileDependencies: [
	'vue-echarts',
	'resize-detector',
	'@ecp/ecp-ui',
	'socket.io-client',
	'ecp-login-component'
]
```

- 如果使用的是@ecp/ecp-ui@1.2.x

在vue.config.js的transpileDependencies添加以下配置（@ecp/ecp-ui@1.2.x为编译版，相关模块不用配置编译）:

```js
transpileDependencies: [
	'ecp-login-component'
]
```

> 如果做了以上配置，依然有问题，需要检查下有其他npm包需要编译，并将其添加到配置中。


### 3. 对象不支持此操作

报错原因： 如果使用了@ecp/ecp-ui的MicroApp启动门户应用，可能会遇到以下new URL报错的问题

解决：

- 升级@ecp/ecp-ui至1.1.x（最低1.1.35），并在portal.js文件头部添加url-polyfill

```js
import '@ecp/ecp-ui/src/utils/micro-app/url-polyfill';
```

- 或者升级@ecp/ecp-ui至1.2.x，并在portal.js文件头部添加url-polyfill

```js
import '@ecp/ecp-ui/lib/url-polyfill';
```

### 4. 对象不支持finally

在 babel.config.js 中的 polyfills 添加 es7.promise.finally

> 如果还有其他不支持的方法和属性，可以添加对应的polyfill

```js
module.exports = {
    presets: [
        ['@vue/app',
            {
                polyfills: [
                    'es6.promise',
                    'es6.symbol',
                    // 兼容finally
                    'es7.promise.finally',
                    // 兼容g2
                    'es6.number.is-nan',
                    'es6.number.is-integer',
                    'es6.math.log10',
                    'es6.array.fill',
                ]
            }
        ],
        [
            '@babel/preset-env',
            {
                'modules': false
            }
        ]
    ],
    sourceType: 'unambiguous', // 严格区分import 和 require 不可混用
    'plugins': [
        '@babel/plugin-proposal-class-properties',
        '@babel/plugin-proposal-optional-chaining'
    ]
};
```

### 5. el-table在chrome70仅显示一行

这是flex在低版本浏览器的兼容性问题，添加height:100%即可

```css
flex: 1 1 auto;
height: 100%;
overflow: auto;
```



