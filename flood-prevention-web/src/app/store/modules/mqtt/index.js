import MqttClass from '@utils/mqtt';

const state = {
    response: null,
    mqttInstance: null
};

const connection = {
    protocol: 'ws',
    // host: 'broker.emqx.io',
    host: process.env.NODE_ENV === 'development' ? '10.51.9.133' : window.location.hostname,
    // ws: 8083; wss: 8084
    // port: 30398,
    port: 30399,
    endpoint: '/mqtt',
    // for more options, please refer to https://github.com/mqttjs/MQTT.js#mqttclientstreambuilder-options
    clean: true,
    connectTimeout: 30 * 1000, // ms
    reconnectPeriod: 4000, // ms
    clientId: 'emqx_vue_' + Math.random().toString(16).substring(2, 8),
    // auth
    username: 'emqx_test',
    password: 'emqx_test'
};

const mutations = {
    INIT: (state) => {
        state.mqttInstance = new MqttClass(connection, data => {
            state.response = data;
        });
        state.mqttInstance.connect();
    },
    PUBLISH: (state, publish) => {
        console.log('消息发布:', publish);
        state.mqttInstance.doPublish(publish);
    },
    SUBSCRIBE: (state, subscription) => {
        if (!subscription.hasOwnProperty('qos')) {
            subscription['qos'] = 0;
        }
        console.log('订阅主题:', subscription);
        state.mqttInstance.doSubscribe(subscription);
    },
    UNSUBSCRIBE: (state, subscription) => {
        console.log('取消订阅:', subscription);
        state.mqttInstance.doUnSubscribe(subscription);
    },
    CLOSE: (state) => {
        if (state.mqttInstance) {
            state.mqttInstance.close();
        }
        state.mqttInstance = null;
    }
};

const actions = {
    // 初始化MQTT
    init ({ commit }) {
        commit('INIT');
    },

    // 消息发布
    publish ({ commit }, publish) {
        commit('PUBLISH', publish);
    },

    // 订阅主题
    subscribe ({ commit }, subscription) {
        commit('SUBSCRIBE', subscription);
    },

    // 取消订阅
    unsubscribe ({ commit }, subscription) {
        commit('UNSUBSCRIBE', subscription);
    },

    // 断开连接
    close ({ commit }) {
        commit('CLOSE');
    }
};

const getters = {
    isConnected: state => state.mqttInstance?.client?.connected
};

export default {
    name: 'mqtt',
    namespaced: true,
    state,
    mutations,
    actions,
    getters
};
