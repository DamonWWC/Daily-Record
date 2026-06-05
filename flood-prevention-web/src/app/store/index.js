import Vue from 'vue';
import Vuex from 'vuex';
import Axios from 'axios';
import Cookies from 'js-cookie';

import Api from '@api';

import modules from './modules';

Vue.use(Vuex);

const pureAxios = Axios.create();

export default new Vuex.Store({
    state: {
        globalConfigs: {},

        userInfo: {},

        token: '',

        clientId: 'mft-hxy-1', // 用于设备控制请求参数
        sessionId: '3a7f2a79-2371-4b9c-b46d-e490ed1f6ec6', // 用于设备控制请求参数
    },
    getters: {
        globalConfigs: state => state.globalConfigs,

        userInfo: state => state.userInfo,

        token: state => state.token
    },
    mutations: {
        setGlobalConfigs (state, config) {
            state.globalConfigs = config || {};
        },

        setUserInfo (state, info) {
            state.userInfo = info || {};
        },

        SET_TOKEN (state, token) {
            state.token = token;
        }
    },
    actions: {
        async getGlobalConfigs (context) {
            try {
                if (_.keys(context.state.globalConfigs).length) return;
                // eslint-disable-next-line
                const name = `${__webpack_public_path__}config.json`;
                const result = await pureAxios(name).then(res => res.data);
                context.commit('setGlobalConfigs', result);
            } catch (error) {
                console.log(
                    '%c [PORTAL] getGlobalConfigs Caught Error',
                    'font-size:18px;color:red;font-weight:700;',
                    error
                );
            }
        },
        async getUserInfo (context) {
            try {
                if (_.keys(context.state.userInfo).length) return;
                const result = await Api.SystemService.getUserInfo();
                context.commit('setUserInfo', result);
            } catch (err) {
                console.log(err);
            }
        },

        /**
         * @method loginWithUserCode 通过UserCode登录
         */
        async loginWithUserCode ({ commit }, userCode) {
            try {
                if (!userCode) {
                    throw new Error('userCode is Required!');
                }
                await Api.Login.logout(true);
                const token = await Api.Login.tokenLogin({
                    userCode
                });
                Cookies.set('x_auth_token', token);

                commit('SET_TOKEN', token);
                return Promise.resolve(token);
            } catch (error) {
                return Promise.reject(error);
            }
        }
    },
    modules
});
