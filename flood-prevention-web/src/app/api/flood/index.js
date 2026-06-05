import Axios from 'axios';
import { Flood } from '../prefix.config';
import { Instruction, PlanHandle, DisposalProcess, ProcessStage, Plan, Rescue, EventProcessRecord, ExecutionLog, NotificationRecord, RoutingPath, Camera, VideoLinkage, LineFloodStation, HjmosDeviceAPI } from './plan-solve';
import { MaterialApi } from './supply';
import { DeviceControl } from './device-control';

// 线网水淹评估
export const NetFloodAssessmentApi = {
    // 查询车站出入口列表
    async queryEntranceExitList(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/infra/station/${lineId}/${stationId}/getEaEInfo`);
        return data.data;
    },

    // 查询水泵故障列表
    async queryFailureList(lineId, stationId, businessId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/water-pump/getFailureList/${businessId}`);
        return data.data;
    },

    // 意见接口
    async setOpinion(businessId, businessCode, status) {
        const { data } = await Axios.get(`${Flood}/opinion/add/${businessId}/${businessCode}/${status}`);
        return data.data;
    },

    // 一键评估
    async setAssessment(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/assessment`);
        return data.data;
    },

    // 车站水泵异常运行状态
    async queryStationStatus(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/water-pump/select-station-abnormal-item`);
        return data.data;
    },

    // 车站水泵组态信息
    async queryConfiguration(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/configuration/get`);
        return data.data;
    },

    // 导出故障水泵列表
    async downloadFailureListExcel(lineId, stationId, businessId) {
        const res = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/water-pump/exportFailureList/${businessId}`, { responseType: 'blob' });
        return res;
    }
};

// 线路水淹评估
export const LineFloodAssessmentApi = {
    // 查询车站出入口列表
    async queryEntranceExitList(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/infra/station/${lineId}/${stationId}/getEaEInfo`);
        return data.data;
    },

    // 查询水泵故障列表
    async queryFailureList(lineId, stationId, businessId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/water-pump/getFailureList/${businessId}`);
        return data.data;
    },

    // 意见接口
    async setOpinion(businessId, businessCode, status) {
        const { data } = await Axios.get(`${Flood}/opinion/add/${businessId}/${businessCode}/${status}`);
        return data.data;
    },

    // 一键评估
    async setAssessment(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/assessment`);
        return data.data;
    },

    // 车站水泵异常运行状态
    async queryStationStatus(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/water-pump/select-station-abnormal-item`);
        return data.data;
    },

    // 车站水泵组态信息
    async queryConfiguration(lineId, stationId) {
        const { data } = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/configuration/get`);
        return data.data;
    },

    // 导出故障水泵列表
    async downloadFailureListExcel(lineId, stationId, businessId) {
        const res = await Axios.get(`${Flood}/biz/wia/${lineId}/${stationId}/water-pump/exportFailureList/${businessId}`, { responseType: 'blob' });
        return res;
    }
};
export const moreFunctionAPI = {
    async getDutyInfo(eventCode) {
        const { data } = await Axios.get('/nec/duty/getByEventCode', { params: { eventCode } });
        return data.data;
    }
};

// 预案处置
export const PlanSolveApi = {
    ...Instruction,
    ...PlanHandle,
    ...DisposalProcess,
    ...ProcessStage,
    ...Plan,
    ...Rescue,
    ...EventProcessRecord,
    ...ExecutionLog,
    ...MaterialApi,
    ...DeviceControl,
    ...NotificationRecord,
    ...RoutingPath,
    ...Camera,
    ...VideoLinkage,
    ...LineFloodStation,
    ...HjmosDeviceAPI
};
