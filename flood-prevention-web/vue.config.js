const webpack = require('webpack');
const CompressionWebpackPlugin = require('compression-webpack-plugin');
const EcpVersionWebpackPlugin = require('@ecp/version-webpack-plugin');
// const EcpThemeWebpackPlugin = require('@ecp/theme-webpack-plugin');
const path = require('path');
const proxyConfig = require('./vue.proxy.config');
const UniPlugin = require('@ecp/uni-plugin');

const DEV_SERVER_PORT = 8992;

const packageConfig = require('./package.json');
const packageName = packageConfig.name; // 使用package.json的name, 如果本应用在dev环境既要做主应用又要做子应用, 需要在启动主应用/子应用时临时把这个名字改掉 (即dev环境的主应用与子应用不能同名)
let output = {};
let externals = {};
let publicPath = process.env.NODE_ENV === 'production' ? './' : '/';

const useMicroApp = !!process.argv.find(d => d === '--uni'); // 启用微前端模式
const usePortal = false; // 如果不需要作为主应用，请设为false

let pages = {
    index: {
        entry: 'src/main.js',
        template: 'public/index.html',
        filename: useMicroApp ? 'sub.html' : 'index.html'
    }
    // // 如使用自定义登录页，请放开这段
    // , login: {
    //     entry: path.resolve(__dirname, 'src/login/login.js'),
    //     template: path.resolve(__dirname, 'src/login/login.html')
    // }
};

if (useMicroApp) {
    output = {
        library: packageName,
        libraryTarget: 'umd',
        jsonpFunction: `webpackJsonp_${packageName}`
    };
    externals = {
        loadsh: {
            commonjs: 'lodash',
            amd: 'lodash',
            root: '_' // 指向全局变量
        }
    };
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

    if (process.env.NODE_ENV === 'production' || !usePortal) {
        // 本地调试时，不修改publicPath，通过代理的方式重写当前应用的sub入口
        publicPath = `/${packageName}/`;
    }
}

module.exports = {
    pages,
    publicPath: publicPath,
    outputDir: 'dist',
    productionSourceMap: false,
    runtimeCompiler: true,

    chainWebpack: config => {
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

            // // 主题插件：处理vue组件中的scss
            // console.log('EcpThemeWebpackPlugin.loader:', EcpThemeWebpackPlugin);
            // const scssVue = config.module.rule('scss').oneOf('vue');
            // scssVue.uses.clear();
            // scssVue
            //     .use(EcpThemeWebpackPlugin.scssloader)
            // .loader(EcpThemeWebpackPlugin.scssloader);
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
        port: DEV_SERVER_PORT,
        host: '0.0.0.0',
        https: false,
        publicPath: publicPath,
        proxy: proxyConfig,
        before (app) {
            if (usePortal) {
                app.get(`/${packageName}/sub.html`, function (req, res) {
                    res.redirect('/sub.html');
                });
            }
        }
    },
    css: {
        loaderOptions: {
            sass: {
                data: '@import "./src/theme/default/import.scss";'
            }
        }
    },
    configureWebpack: config => {
        // if (process.env.NODE_ENV === 'production') {
        //     plugins.push(
        //         new EcpThemeWebpackPlugin({
        //             entry: {
        //                 default: path.resolve(__dirname, './src/theme/default/index.scss')
        //                 // darken: path.resolve(__dirname, './src/theme/darken/index.scss')
        //             }
        //         })
        //     );
        // }
        return {
            output: output,
            resolve: {
                alias: {
                    '@': path.resolve(__dirname, 'src'),
                    '@views': path.resolve(__dirname, 'src/app/views'),
                    '@common': path.resolve(__dirname, 'src/common'),
                    '@constants': path.resolve(__dirname, 'src/constants'),
                    '@utils': path.resolve(__dirname, 'src/common/utils'),
                    '@api': path.resolve(__dirname, 'src/app/api'),
                    '@assets': path.resolve(__dirname, 'src/assets'),
                    '@components': path.resolve(__dirname, 'src/app/components'),
                }
            },
            externals: externals,
            plugins: [
                new webpack.ProvidePlugin({
                    _: 'lodash',
                    Axios: 'axios',
                    Utils: path.resolve(__dirname, 'src/common/utils')
                }),
                new webpack.DefinePlugin({
                    PACKAGE_NAME: JSON.stringify(packageName)
                }),
                new UniPlugin({
                    enabled: !!useMicroApp,
                    replaceIndex: !usePortal,
                    externals: Object.keys(externals).map(item =>
                        item === 'loadsh' ? 'lodash' : item
                    )
                }),
                new EcpVersionWebpackPlugin()
            ]
        };
    },

    transpileDependencies: ['ecp-login-component']
};
