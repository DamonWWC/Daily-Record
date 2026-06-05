import Axios from 'axios';
import { Sysmanager, DictManager } from '../prefix.config';

export const Dictionary = {
    /**
     * 系统字典相关
     * @method queryCatalogTree 行政区划树
     */
    queryCatalogTree: function () {
        return Axios.get(`${Sysmanager}/api/sysStructureInfo/getAllTree`).then(
            ({ data }) => data.Data
        );
    },
    /**
     * 系统字典相关
     * @method queryCatalogTreeOnly 行政区划树 (没有虚拟节点)
     */
    queryCatalogTreeOnly: function () {
        return Axios.get(
            `${Sysmanager}/api/sysStructureInfo/getAllTree?nodeType=1`
        ).then(({ data }) => data.Data);
    },
    /**
     * 系统字典相关
     * @method queryCatalogDictionaryTree 行政区划树 (字典子树-行政区划树)
     * 与获取字典不同在于所传参数为OrgCode而非ParentId
     */
    queryCatalogDictionaryTree: function () {
        return Axios.get(`${DictManager}/dictionary/orgTree`).then(
            ({ data }) => data.Data
        );
    },

    /**
     * 系统字典相关
     * @method getDictionary 获取字典 (平铺)
     * !!!!! 请通过 VueX 获取, 业务代码不要直接调用本接口 !!!!!
     * @param { String, Number } Kind 字典Kind
     */
    getDictionary (Kind) {
        return Axios.get(
            `${DictManager}/dictionary/listSysDict?Kind=${Kind}`
        ).then(({ data }) => {
            // 字典为空时提示
            if (!data.Data || data.Data.length === 0) {
                console.warn(
                    `>>>>>> 字典Kind=${Kind} 数据为空，请确认是否已初始化字典数据`
                );
                return [];
            }
            return data.Data;
        });
    },
    /**
     * 系统字典相关
     * @method getDictionaryTree 获取字典树
     * !!!!! 请通过 VueX 获取, 业务代码不要直接调用本接口 !!!!!
     * @param { String, Number } Kind 字典Kind
     */
    getDictionaryTree (Kind) {
        return Axios.get(
            `${DictManager}/dictionary/getSimpleTreeByKind?Kind=${Kind}`
        ).then(({ data }) => {
            // 字典为空时提示
            if (!data.Data || data.Data.length === 0) {
                console.warn(
                    `>>>>>> 字典树Kind=${Kind} 数据为空，请确认是否已初始化字典数据`
                );
                return [];
            }
            return data.Data;
        });
    }
};
