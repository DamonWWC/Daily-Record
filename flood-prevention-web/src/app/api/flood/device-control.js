import Axios from 'axios';
import { deviceControl } from '@api/prefix.config';

export const DeviceControl = {
    // 获取广播序列列表
    async getBroadcastSequenceList(lineId, stationId = null) {
        const { data } = await Axios.post(`${deviceControl}/broadcast-sequence/select-list`, { lineId, stationId });
        return data.data;
    },
    // 获取预置广播列表
    async getPresetBroadcast(lineNo, stationNo = null) {
        const { data } = await Axios.get(`${deviceControl}/hjdp/pa-dva-message/get`, { params: { lineNo, stationNo } });
        return data.data;
    },
    // 获取广播区域列表
    async getPaAreas(lineNo, stationNo = null) {
        const { data } = await Axios.get(`${deviceControl}/commom/paArea`, { params: { lineNo, stationNo } });
        return data.data;
    },
    // 播放广播
    async playBroadcast2(params) {
        const { data } = await Axios.post(`${deviceControl}/commom/pa-play`, params);
        return data.data;
    },
    // 播放广播
    async PlayBroadcast(params) {
        const { data } = await Axios.post(`${deviceControl}/api/pa-control-command-syn`, params);
        return data.data;
    },
    // 停止广播
    async stopBroadcast(params) {
        const { data } = await Axios.post(`${deviceControl}/commom/pa-close`, params);
        return data.data;
    },
    // 获取AFC设备控制面板
    async getAFCControlPanel(lineId, stationId) {
        const { data } = await Axios.post('/hjmos-device/content/deviceContentTreeInculdeDevice/AFC_DC_CTRL', { lineId, stationId });
        return data.data;
    },
    // 开启所有PIS屏幕
    async paOpenCloseScreen(params) {
        const { data } = await Axios.post(`${deviceControl}/commom/paOpenCloseScreen`, params);
        return data.data;
    },
    async getPidsAreasNew(params) {
        const { data } = await Axios.get(`${deviceControl}/commom/pidsPlayArea`, { params });
        return data.data;
    },
    async getPisDevice(params) {
        const { data } = await Axios.get(`${deviceControl}/commom/pidsDevice`, { params });
        return data.data;
    },
    async distributionPidsText(params) {
        const { data } = await Axios.post(`${deviceControl}/api/pids-control-command-syn`, params);
        return data.data;
    },
    async switchPisScene(params) {
        const { data } = await Axios.post(`${deviceControl}/api/pids-emergency-cancel`, params);
        return data.data;
    },
    async switchPisSceneNormal(params) {
        const { data } = await Axios.post(`${deviceControl}/api/pids-common-cancel`, params);
        return data.data;
    },
    async setDeviceControlCommand(params) {
        const { data } = await Axios.post('/hjmos-flood-serve/api/control-command-asyn', params);
        return data.data;
    },
    async getSessionIdByClinetId(clientId) {
        const { data } = await Axios.post(`/hjmos-authcenter/integration/mics/getSessionIdByClientId?clientId=${clientId}`);
        return data.data;
    },
    async getRollingShutterDoorList(params) {
        const { data } = await Axios.post('/hjmos-device/content/deviceContentTreeInculdeDevice/JLM_CTRL', params);
        return data.data;
    }

};
