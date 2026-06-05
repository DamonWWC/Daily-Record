import Vue from 'vue';
import EcpUI from '@ecp/ecp-ui';
import store from '@/app/store';
import login from './login.vue';

import '@ecp/ecp-ui/theme/default/index.scss';

Vue.use(EcpUI);

const initApp = async () => {
    try {
        await store.dispatch('getGlobalConfigs');
    } catch (error) {
        console.log(
            '%c getGlobalConfigs Caught Error',
            'font-size:18px;color:red;font-weight:700;',
            error
        );
    }
    return Promise.resolve();
};

initApp().then(() => {
    new Vue({
        render: h => h(login),
        store
    }).$mount('#login');
});
