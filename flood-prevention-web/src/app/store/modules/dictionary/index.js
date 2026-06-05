import Axios from 'axios';
import Api from '@api';

import { DICTIONARY_MAP } from '@/constants/dictionary-map';

/* token登录相关 */
export default {
    name: 'dictionary', // 建议打开 后续新增模块过多时区分
    namespaced: true, // 建议打开 后续新增模块过多时区分
    state: () => {
        return {
            // 字典缓存
            dictionaryCache: {},
            // 字典树缓存
            dictionaryTreeCache: {}
        };
    },
    getters: {
        dictionaryCache: state => state.dictionaryCache,
        dictionaryTreeCache: state => state.dictionaryTreeCache
    },
    mutations: {
        SET_DICTIONARYCACHE ({ dictionaryCache }, { key, value }) {
            dictionaryCache[key] = value;
        },
        SET_DICTIONARYTREECACHE ({ dictionaryTreeCache }, { key, value }) {
            dictionaryTreeCache[key] = value;
        }
    },
    actions: {
        /**
         * @method getDictionaryCache 获取并缓存字典 (平铺)
         * @param {String,Number} KindKey
         * @returns {Promise.resolve(Array)}
         */
        async getDictionaryCache ({ commit, state }, KindKey, force) {
            const existDictionaryMap = DICTIONARY_MAP.hasOwnProperty(KindKey);
            const Kind = existDictionaryMap ? DICTIONARY_MAP[KindKey] : KindKey;
            if (
                !force &&
                state.dictionaryCache[KindKey] &&
                state.dictionaryCache[KindKey] instanceof Array &&
                state.dictionaryCache[KindKey].length > 0
            ) {
                return state.dictionaryCache[KindKey];
            } else {
                if (window.dictionaryCallbacks?.[KindKey]) {
                    return new Promise(async (resolve, reject) => {
                        window.dictionaryCallbacks[KindKey].push(res =>
                            resolve(res)
                        );
                    });
                } else {
                    if (!window.dictionaryCallbacks) {
                        window.dictionaryCallbacks = {};
                    }
                    window.dictionaryCallbacks[KindKey] = [];
                    const dict = await Api.SystemService.getDictionary(Kind);
                    window.dictionaryCallbacks[KindKey].forEach(callback =>
                        callback(dict)
                    );
                    window.dictionaryCallbacks[KindKey] = null;
                    delete window.dictionaryCallbacks[KindKey];
                    commit('SET_DICTIONARYCACHE', {
                        key: KindKey,
                        value: dict
                    });
                    return dict;
                }
            }
        },
        /**
         * @method getDictionaryTreeCache 获取并缓存字典树
         * @param {String,Number} KindKey
         * @returns {Promise.resolve(Array)}
         */
        async getDictionaryTreeCache ({ commit, state }, KindKey, force) {
            const existDictionaryMap = DICTIONARY_MAP.hasOwnProperty(KindKey);
            const Kind = existDictionaryMap ? DICTIONARY_MAP[KindKey] : KindKey;
            if (
                !force &&
                state.dictionaryTreeCache[KindKey] &&
                state.dictionaryTreeCache[KindKey] instanceof Array &&
                state.dictionaryTreeCache[KindKey].length > 0
            ) {
                return state.dictionaryTreeCache[KindKey];
            } else {
                const dictTree = await Api.SystemService.getDictionaryTree(Kind);
                commit('SET_DICTIONARYTREECACHE', {
                    key: KindKey,
                    value: dictTree
                });
                return dictTree;
            }
        }
    }
};
