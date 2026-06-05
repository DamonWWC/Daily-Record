import Axios from 'axios';
import { HjmosDfs, BasicData } from '@api/prefix.config';

// 文件服务接口
export const HjmosDfsApi = {
    // 附件上传（这个上传有问题，暂时改用下面的接口）
    // async fileUploaderByStream (file) {
    //     const { data } = await Axios.post(`${PlanSolve}/event/uploadbystream`, file, {
    //         headers: { 'content-type': 'multipart/form-data' }
    //     });
    //     return data.data;
    // },

    // 附件上传
    async fileUploaderByStream(file) {
        const { data } = await Axios.post(`${HjmosDfs}/file/uploadbystream`, file, {
            headers: { 'content-type': 'multipart/form-data' }
        });
        return data.data;
    }
};

let basicDataService = `${BasicData}/basicData`;

export const BasicDataApi = {
    // 获取地铁线路信息
    async fetchLineList() {
        const { data } = await Axios.get(`${basicDataService}/getLineList`);
        return data.data;
    },
    // 获取线路车站
    async fetchStationList(params) {
        const { data } = await Axios.get(`${basicDataService}/getStationList`, { params });
        return data.data;
    }
};
