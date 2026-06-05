import Axios from 'axios';
import { Sso, Sysmanager, DictManager } from '../prefix.config';

import { Dictionary } from './dictionary';

export const SystemService = {
    /**
     * @method getUserInfo 获取当前用户信息 (公服)
     */
    getUserInfo () {
        return Axios.get(`${Sysmanager}/api/sysUser/getUserInfo`).then(
            res => res.data.Data
        );
    },
    /**
     * @method getOtherUserInfo 获取当前用户信息 (业务系统)
     */
    getOtherUserInfo () {
        return Promise.resolve({});
    },

    /**
     * @method getClientIp 获取客户端Ip
     */
    getClientIp () {
        return Axios.get(`${Sso}/sysUser/getClientIp`).then(
            res => res.data.Data
        );
    },
    /**
     * @method getGlobalSetting 获取系统设置 (公服)
     */
    getGlobalSetting () {
        return Axios.get(
            `${Sysmanager}/api/sysLoginConfig/getSysLoginConfig`
        ).then(({ data }) => data);
    },

    /**
     * @method updateUserPassword 修改用户密码
     */
    updateUserPassword (params) {
        return Axios.put(
            `${Sysmanager}/inner/sysUser/modifyPassword`,
            params
        ).then(({ data }) => data);
    },

    /**
     * @method getBtnAuth 获取当前用户所有按钮权限
     */
    getBtnAuth: function (params) {
        return Axios.get(`${Sysmanager}/inner/sysMenu/getUserButtons`, {
            params
        }).then(({ data }) => data.Data);
    },
    /**
     * @method getUserMenuTree 获取当前用户菜单权限
     */
    getUserMenuTree (params) {
        return Axios.get(
            `${Sysmanager}/inner/sysMenu/getSortedUserMenuTreeWithoutButton`,
            { params }
        ).then(({ data }) => data.Data);
    },

    ...Dictionary
};
