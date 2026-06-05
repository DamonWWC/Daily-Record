import Api from '@/app/api';
import menuConfig from '@constants/mock/menu-config';

import { Utils as EcpUtils } from '@ecp/ecp-ui';

// 递归生成导航菜单
function reformatMenu (menu, leafOnly, ignoreCase) {
    return (
        (Array.isArray(menu) &&
            menu.reduce((prev, menuItem) => {
                let { Id, Text, Value, ChildNodes } = menuItem;
                let result = prev;
                if (ignoreCase) {
                    let ignoreMatched = Object.keys(ignoreCase).every(key => {
                        let targetMenuItem = (key || '')
                            .split('.')
                            .reduce(
                                (prev, curr) =>
                                    prev === null || prev === undefined
                                        ? prev
                                        : prev[curr],
                                menuItem
                            );
                        return typeof targetMenuItem === 'string'
                            ? targetMenuItem.match(ignoreCase[key])
                            : targetMenuItem === ignoreCase[key];
                    });
                    if (ignoreMatched) {
                        return result;
                    }
                }
                if (
                    (Value['Target'] &&
                        (!leafOnly || Value['Target'].match('/'))) ||
                    Value['Url']
                ) {
                    result.push({
                        Id,
                        Text,
                        Value,
                        Url: Value['Url'] || null,
                        Target: Value['Target'] || null,
                        FunTag: Value['FunTag'] || null,
                        ChildNodes: reformatMenu(
                            ChildNodes,
                            leafOnly,
                            ignoreCase
                        )
                    });
                } else if (leafOnly && ChildNodes) {
                    result = [
                        ...result,
                        ...reformatMenu(ChildNodes, leafOnly, ignoreCase)
                    ];
                }
                return result;
            }, [])) ||
        []
    );
}

// 深度搜索权限树
function findDeep (menu, key, value) {
    if (menu && menu instanceof Array && menu.length > 0) {
        for (let menuItem of menu) {
            if (key) {
                let targetMenuItem = (key || '')
                    .split('.')
                    .reduce(
                        (prev, curr) =>
                            prev === null || prev === undefined
                                ? prev
                                : prev[curr],
                        menuItem
                    );
                if (targetMenuItem === value) {
                    return menuItem;
                }
            }
            if (menuItem['ChildNodes']) {
                let childItem = findDeep(menuItem['ChildNodes'], key, value);
                if (childItem) {
                    return childItem;
                }
            }
        }
    }
    return null;
}

/**
 * @method flattenDeep 深度展开权限树
 * @param { Array } menu 权限树
 * @param { string } key 需要扁平化的字段 不可为空
 * @param { string } filterKey 权限筛选字段，由于菜单列表已经是默认返回用户所能看的全部权限，所以默认与key一个值
 * @returns { Array }
 */
function flattenDeep (menu, key, filterKey = key) {
    return (
        (Array.isArray(menu) &&
            key &&
            menu.reduce(
                (prev, curr) => [
                    ...prev,
                    ...(curr[key] !== undefined &&
                    curr[key] !== null &&
                    curr[filterKey]
                        ? [curr[key]]
                        : []),
                    ...flattenDeep(curr['ChildNodes'], key, filterKey)
                ],
                []
            )) ||
        []
    );
}
export default {
    name: 'permission', // 建议打开 后续新增模块过多时区分
    namespaced: true, // 建议打开 后续新增模块过多时区分
    state: () => {
        return {
            logo: '',
            logoText: '',
            hideInitInfo: false,
            currMenu: 'navMenu',
            navMenu: [],
            portalNavigation: [],
            extMenu: [],
            systemMenu: [],
            funTagList: [],
            urlList: [],
            cachedViews: []
        };
    },
    getters: {
        funTagList: state => state.funTagList,
        urlList: state => state.urlList,
        menuTargetList: state => {
            return state.menuTargetList;
        }
    },
    mutations: {
        SET_CURR_MENU (state, currMenu) {
            if (state[currMenu]) {
                state.currMenu = currMenu;
            }
        },
        ADD_CACHED_ROUTES (state, view) {
            if (state.cachedViews.includes(view.name)) return;
            if (!view.meta.noCache) {
                state.cachedViews.push(view.name);
            }
        },
        DEL_CACHED_ROUTES (state, view) {
            const index = state.cachedViews.indexOf(view.name);
            index > -1 && state.cachedViews.splice(index, 1);
        },

        SET_NAVMENU (state, navMenu) {
            state.navMenu = navMenu;
        },
        SET_EXTMENU (state, extMenu) {
            state.extMenu = extMenu;
        },
        SET_SYSTEMMENU (state, systemMenu) {
            state.systemMenu = systemMenu;
        },
        SET_FUNTAGLIST (state, funTagList) {
            state.funTagList = funTagList;
        },
        SET_URLLIST (state, urlList) {
            state.urlList = urlList;
        }
    },
    actions: {
        addCachedView ({ commit }, view) {
            return new Promise(resolve => {
                commit('ADD_CACHED_ROUTES', view);
                resolve();
            });
        },
        delCachedView ({ commit }, view) {
            return new Promise(resolve => {
                commit('DEL_CACHED_ROUTES', view);
                resolve();
            });
        },

        // 获取权限树
        async getPermission ({ commit, rootState }) {
            /**
             * 如不使用公服管理菜单权限，请使用这段 Start
             */
            const menuList = []; // 获取菜单

            const btnList = []; // 获取按钮权限
            /**
             * 如不使用公服管理菜单权限，请使用这段 End
             */

            /**
             * 如使用公服管理菜单权限，请使用这段 Start
             */
            // const menuList = await Api.SystemService.getUserMenuTree({
            //     UserCode: rootState.userInfo.UserCode
            // }); // 获取菜单

            // const btnList = await Api.SystemService.getBtnAuth({
            //     UserCode: rootState.userInfo.UserCode
            // }); // 获取按钮权限
            /**
             * 如使用公服管理菜单权限，请使用这段 End
             */

            let params = [menuList, 'Id', 'TargetTag']; /* 匹配标识 单个标识 String */
            const navMenuList =
                findDeep.apply(this, params)?.['ChildNodes'] || []; // 业务菜单权限

            params = [
                menuList,
                'Value.FunTag',
                'SYSTEM_SERVICE'
            ]; /* 匹配标识 单个标识 String */
            const systemMenuList =
                findDeep.apply(this, params)?.['ChildNodes'] || []; // 用户管理后台权限

            let navMenu =
                reformatMenu(navMenuList, false, {
                    'Value.FunTag': 'SYSTEM_SERVICE' // 排除的标识 Regx String
                }) || []; // 根据排除的标识去除排除的菜单
            let systemMenu = reformatMenu(systemMenuList) || []; // 根据排除的标识去除排除的菜单

            // 开发环境例子
            // if (process.env.NODE_ENV === 'development') {
            navMenu = [...(navMenu || []), ...(menuConfig?.['menu'] || [])];
            // }

            const funTagList = [
                ...new Set([
                    ...flattenDeep(navMenu, 'FunTag'),
                    ...flattenDeep(systemMenu, 'FunTag'),
                    ...flattenDeep(btnList, 'FunId', 'Auth')
                ])
            ];

            const urlList = [
                ...new Set([
                    ...flattenDeep(navMenu, 'Target'),
                    ...flattenDeep(systemMenu, 'Target'),
                    ...flattenDeep(btnList, 'Url', 'Auth')
                ])
            ];

            commit('SET_NAVMENU', navMenu);
            commit('SET_SYSTEMMENU', systemMenu);
            commit('SET_FUNTAGLIST', funTagList);
            commit('SET_URLLIST', urlList);
            return navMenu;
        },
        resetData ({ commit }) {
            return new Promise(resolve => {
                commit('SET_NAVMENU', []);
                commit('SET_SYSTEMMENU', []);
                commit('SET_FUNTAGLIST', []);
                commit('SET_URLLIST', []);
                resolve();
            });
        }
    }
};
