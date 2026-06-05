import './polyfills';
import './plugins';

import Vue from 'vue';
import App from './App.vue';
import router from './app/router';
import store from './app/store';
import EcpPlayer from '@ecp/ecp-player/lib/ecpPlayer';

import * as InitialUtils from '@/common/utils/initial-utils';

import CommonPart from './common';

import EcpUI from '@ecp/ecp-ui';
import '@assets/iconfont/iconfont.js';
import '@assets/iconfont/iconfont.css';
import 'tailwindcss/tailwind.css';
import '@ecp/ecp-ui/lib/ecp-ui.common.css'; // 直接引用css，可以跳过@ecp/ecp-ui的scss编译
import '@/theme/blue_src/index.scss'; // 1.2.17及以后版本的@ecp/ecp-ui使用这种方式引入样式文件
import '@/theme/blue/index.scss';
import '@utils/rem';
import Cookies from 'js-cookie';
import wpf from '@/common/utils/wpf';
import _ from 'lodash';
import { sassNull } from 'sass';

Vue.config.productionTip = false;

const initApp = async container => {
    try {
        // 如果还有其它渲染前置处理，请在 src、common、utils、initial-utils、index.js 的 systemInitial 里面添加
        await InitialUtils.systemInitial({
            loadingTarget: (container || document).querySelector('#app')
        });
    } catch (error) {
        console.log(
            '%c systemInitial Caught Error',
            'font-size:18px;color:red;font-weight:700;',
            error
        );
    }
    return Promise.resolve();
};

if (!window.__POWERED_BY_QIANKUN__) {
    Vue.use(EcpUI);
    Vue.use(CommonPart);
    Vue.use(EcpPlayer);
    Vue._ecpPlayerInit('./vendor/');

    const cookieItem = Cookies.get('isClient');
    const isClient = cookieItem === 'true';
    console.log('---isClient', isClient);
    // 判断是否是客户端
    if (window.CefSharp || isClient) {
        store.commit('wpf/setIsClient', true);
        let planSolveParam;
        wpf.toPie({ command: 'yacz' }).then((res) => {
            let data = res.param?.data;
            planSolveParam = _.mapKeys(data, (value, key) => _.camelCase(key));
            store.commit('planSolve/SET_PLANSOLVEPARAM', planSolveParam);
        });
    } else {
        store.commit('wpf/setIsClient', false);
        let planSolveParam = {
            eventId: null,
            planId: 222,
            eventCode: 'czsy01',
            isDrill: false,
            processId: null,
            entrance: 0
        };
        store.commit('planSolve/SET_PLANSOLVEPARAM', planSolveParam);
    }

    Vue.prototype._ = _;

    // wpf.receive();

    store.dispatch('mqtt/init');

    initApp().then(() => {
        new Vue({
            router,
            store,
            render: h => h(App)
        }).$mount('#app');
    });
}

let instance = null;

export async function bootstrap () {
    // console.log('flood-prevention-web bootstraped');
}

export async function mount (props) {
    // console.log('flood-prevention-web props from main framework', props);
    Vue.use({ ...EcpUI });
    Vue.use({ ...CommonPart });

    initApp(props.componentName && props.container).then(() => {
        instance = new Vue({
            router,
            store,
            el: props.container
                ? props.container.querySelector('#app')
                : '#app',
            render: h =>
                h(App, {
                    props: {
                        componentName: props.componentName,
                        componentProps: props.componentProps
                    }
                })
        });
    });
}

export async function unmount () {
    // 必须确保 有实例 且 有 $destroy 才能调用销毁, 否则会把主子应用整个qiankun拉宕掉
    if (instance) {
        instance?.$destroy && instance.$destroy();
        instance = null;
    }
}
