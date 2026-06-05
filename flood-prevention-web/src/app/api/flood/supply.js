import Axios from 'axios';
import { material } from '@api/prefix.config';

export const MaterialApi = {
    async getoverviewPage(params) {
        const { data } = await Axios.post(`${material}/emergency/goods/overviewPage`, params);
        return data.data;
    },
    async getApplyMaterial(params) {
        const { data } = await Axios.post(`${material}/emergency/goods/allocate/page`, params);
        return data.data;
    },
    async applyAllocate(params) {
        const { data } = await Axios.post(`${material}/emergency/goods/allocate/apply`, params);
        return data.data;
    },
    async getGoodsTypeList() {
        const { data } = await Axios.get(`${material}/emergency/goodsType/getList`);
        return data.data;
    },
    async getGoodsInfo(id) {
        const { data } = await Axios.get(`${material}/emergency/goods/allocate/getInfo?id=${id}`);
        return data.data;
    }
};
