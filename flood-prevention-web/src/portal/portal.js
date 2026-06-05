import '@ecp/ecp-ui/lib/url-polyfill';
import nprogress from 'nprogress';
import 'nprogress/nprogress.css';

import Vue from 'vue';
import App from './portal.vue';
import EcpUI, { Utils as EcpUtils } from '@ecp/ecp-ui';
// import { MicroUtils, MicroApp } from '../micro-app';
import { LoginUtils } from 'ecp-login-component';
// import { Menu } from './config/menu.config';

import * as InitialUtils from '@/common/utils/initial-utils';

import router from '@/app/router';
import store from '@/app/store';

import '@ecp/ecp-ui/theme/default/index.scss'; // 1.2.17及以后版本的@ecp/ecp-ui使用这种方式引入样式文件

import '@/theme/default/index.scss';

import Components from '@common/components';

Vue.config.productionTip = false;
Vue.use(EcpUI);

Components.forEach(component => {
    Vue.component(component.name, component);
});

const MicroApp = EcpUtils.MicroApp;
const MicroUtils = EcpUtils.MicroUtils;

// 菜单的属性
const menuProps = {
    id: 'Id',
    label: 'Text',
    route: 'Target',
    url: 'Url',
    symbol: 'symbol',
    children: 'ChildNodes'
};

const mountErrHandler = err => {
    if (err.reason === 'cancel' || err.type === 'unhandledrejection') return;
    window.eventBus.emit('PORTAL_APP_MOUNT_ERROR', err);
};

const microApp = new MicroApp({
    // vue: Vue,
    rootEntry: App,
    mainContainer: '#container',
    subContainer: '#sub-wrapper'
});

(async function () {
    // 初始化应用

    try {
        await InitialUtils.systemInitial({});
    } catch (error) {
        console.log(
            '%c [PORTAL] systemInitial Caught Error',
            'font-size:18px;color:red;font-weight:700;',
            error
        );
    }

    const appConfig = {
        startsWith: '/s-',
        menu: store.state.permission['navMenu'],
        systemMenu: store.state.permission['systemMenu'] // 导航菜单和系统管理菜单有需要可以拆开来
    };

    appConfig.menuProps = menuProps;
    appConfig.menu = InitialUtils.formatMenu({
        menu: appConfig.menu,
        menuProps,
        symbol: appConfig.startsWith
    });
    appConfig.systemMenu = InitialUtils.formatMenu({
        menu: appConfig.systemMenu,
        menuProps,
        symbol: appConfig.startsWith
    });

    console.log(
        '%c [PORTAL] Menus',
        'font-size:18px;color:blue;font-weight:700;',
        appConfig.menu,
        appConfig.systemMenu
    );

    const locations = window.location;
    let href = locations.pathname + (locations?.hash || '');
    const hrefMatcher =
        locations.pathname + (locations?.hash?.replace(/\?.*/, '') || '');
    const rootPath = '/';

    const isGetFirstMenu =
        hrefMatcher === `${rootPath}s-` || hrefMatcher === rootPath || hrefMatcher === '/#/';

    // 没有路径时，使用配置的默认路径
    // isGetFirstMenu && (href = MicroUtils.getFirstMenuRoute(appConfig.menu, menuProps));
    // // const menuNode = MicroUtils.findTree(appConfig.menu, (d) => d[menuProps.route] === href, menuProps);
    if (isGetFirstMenu) {
        const firstPath = MicroUtils.getFirstMenuRoute(
            appConfig.menu,
            menuProps
        );
        firstPath && (href = firstPath);
    }
    const symbol =
        (MicroUtils.constants.SYMBOL_REG.exec(href) || [])[0] ||
        appConfig.startsWith;

    appConfig.defaultPath = href;
    appConfig.defaultRoute = href;
    appConfig.startsWith = symbol;

    console.log(
        '%c [PORTAL] AppConfig',
        'font-size:18px;color:blue;font-weight:700;',
        appConfig
    );

    // 初始化 state
    const state = {
        config: appConfig
    };

    // 将配置放到全局状态
    const actions = MicroUtils.initGlobalState(state);
    actions.setGlobalState(state);
    actions.offGlobalStateChange();

    // 动画的类名
    const enterName = 'fade-transform-enter';
    const enterActiveName = 'fade-transform-enter-active';
    const leaveName = 'fade-transform-leave-to';
    const leaveActiveName = 'fade-transform-leave-active';

    const loadingWrapper = '#portal-layout-content'; // 这里要替换为实际的包裹子应用容器的id
    nprogress.configure({ parent: loadingWrapper }); // 子应用加载进度条配置

    // // 如果初始化进入的子应用设置了默认路由, 且需要初始化就跳转至第一条菜单链接, 就放开这里
    // MicroUtils.runAfterFirstMounted(() => {
    //     if (href !== location.href.replace(location.origin, '')) {
    //         window.history.pushState({}, '', href);
    //     }
    // });

    /**
     * 子应用加载
     */
    let initComp = false;
    const subWrapperName = '#sub-wrapper'; // 这里要替换为实际的子应用容器id
    microApp.start([...appConfig.menu, ...appConfig.systemMenu], {
        config: appConfig,
        defaultRoute: appConfig.defaultRoute,
        symbol,
        menuProps,
        beforeLoad: app => {
            /**
             * // 如确定主应用没有使用 loadMicroApp 手动加载子应用，则可保留 externals，并需要嵌入公服或其它未剔除 externals 的应用，请放开这段
             * // 否则主应用与子应用应移除 vue、vuex、vue-router 等 externals 处理
             * // 剔除了 externals 的主应用，可使用代理+匹配上下文标识替换，并使用 iframe 嵌入未剔除 externals 的应用
            const otherUnuseExternalsSystems = [
                'omof-frontend',
                'common-frontend',
                'flood-prevention-web'
            ];
            if (otherUnuseExternalsSystems.includes(app.name)) {
                if (!window.Vue) {
                    window.Vue2 && (window.Vue = window.Vue2);
                }
            } else if (window.Vue) {
                // 单独的实例应用
                window.Vue2 = window.Vue;
                delete window.Vue;
            }
            */
        },
        beforeMount: () => {
            return new Promise(resolve => {
                nprogress.start();
                let subContainer = document.querySelector(subWrapperName);
                if (subContainer) {
                    subContainer.classList.add(enterActiveName);
                    subContainer.classList.add(enterName);
                }
                setTimeout(
                    () => {
                        resolve();
                    },
                    initComp ? 100 : 0
                );
                if (!initComp) {
                    initComp = true;
                }
            });
        },
        afterMount: app => {
            return new Promise(resolve => {
                InitialUtils.reformatSubSystem(app.name);
                let subContainer = document.querySelector(subWrapperName);
                if (subContainer) {
                    nprogress.done();
                    subContainer.classList.remove(enterActiveName);
                    subContainer.classList.remove(enterName);
                }
                resolve();
            });
        },
        beforeUnmount: () => {
            return new Promise(resolve => {
                nprogress.start();
                let subContainer = document.querySelector(subWrapperName);
                if (subContainer) {
                    subContainer.classList.add(leaveActiveName);
                    subContainer.classList.add(leaveName);
                }
                setTimeout(() => {
                    resolve();
                }, 150);
            });
        },
        afterUnmount: () => {
            return new Promise(resolve => {
                let subContainer = document.querySelector(subWrapperName);
                if (subContainer) {
                    subContainer.classList.remove(leaveActiveName);
                    subContainer.classList.remove(leaveName);
                }
                resolve();
            });
        }
    });
})();

// 处理微应用发送的sso登录消息
window.eventBus.on('loginStatus', data => {
    // // 如使用自定义登录页, 用这个↓
    // // eslint-disable-next-line
    // const loginUrl = `${window.location.origin}/${window.__POWERED_BY_QIANKUN____?'flood-prevention-web':__webpack_public_path__.replace(/^\/+|\/+$/g, '')}/login.html`;

    const loginUrl = '';

    const param = {
        isPrimaryApp: true,
        response: data.response,
        loginUrl: loginUrl || window.location.origin + '/sso/login'
        // , isComplexNetwork: true // 如应用部署网络环境复杂(例如: 需要支持非内网访问等), 请放开isComplexNetwork
    };
    LoginUtils.loginInterceptors(param);
});
