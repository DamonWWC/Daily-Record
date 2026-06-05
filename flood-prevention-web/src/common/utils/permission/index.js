import { Loading } from '@ecp/ecp-ui';
import store from '@/app/store';

export async function permissionInit () {
    let loadingInstance = null;
    try {
        loadingInstance = Loading.service({
            target: document.querySelector('#app'),
            body: false,
            fullscreen: true,
            background: 'rgba(255,255,255, 0.85)',
            text: store.state.globalConfigs?.IMPORT_CONFIGS?.title || '加载中',
            lock: true
        });
        await store.dispatch('getGlobalConfigs');

        if (loadingInstance) {
            loadingInstance.text =
                store.state.globalConfigs.IMPORT_CONFIGS.title;
        }

        try {
            const queryStr = location.hash.split('?')[1];
            const initQuery =
                queryStr &&
                queryStr.split('&').reduce((prev, curr) => {
                    const currItem = curr && curr.split('=');
                    return {
                        ...prev,
                        [currItem?.[0]]: currItem?.[1]
                    };
                }, {});
            const userCode = initQuery?.usercode;
            if (userCode) {
                await store.dispatch('tokenLogin/login', userCode);
            }
        } catch (error) {
            console.log(
                '%c tokenLogin Failed',
                'font-size:18px;color:red;font-weight:700;',
                error
            );
        }
        await store.dispatch('getUserInfo');

        await store.dispatch('permission/getPermission');
        await store.dispatch('getDeptInfo');
        await store.dispatch('getUserGroupListInfo');
        if (store.getters?.isCompany) {
            await store.dispatch('getEnterpriseInitInfo');
        }
    } catch (error) {
        console.log(
            '%c permissionInit Caught Error',
            'font-size:18px;color:red;font-weight:700;',
            error
        );
    }
    loadingInstance && loadingInstance.close && loadingInstance.close();
}

export function permissionCheck (funTag = '') {
    if (!funTag) return true;
    const { 'permission/funTagList': funTagList } = store.getters;
    if (Array.isArray(funTag)) {
        return funTag.some((funTag) => funTagList.includes(funTag));
    } else {
        return funTagList.includes(funTag);
    }
}
