import qs from 'qs';
import EcpUI, { Utils as EcpUtils } from '@ecp/ecp-ui';
import store from '@/app/store';

const MicroUtils = EcpUtils.MicroUtils;

const defaultMenuProps = {
    id: 'Id',
    label: 'Text',
    route: 'Target',
    url: 'Url',
    symbol: 'symbol',
    children: 'ChildNodes'
};

/**
 * @method formatMenu 菜单处理
 * @param {Object} params
 *** @property {Array} menu 菜单树
 *** @property {Array} menuProps 菜单键映射
 *** @property {String} symbol 链接前缀匹配规则
 *** @property {Number} level 层级
 */
export const formatMenu = ({ menu, menuProps, symbol = '/s-', level = 0 }) => {
    if (!menuProps) {
        menuProps = _.cloneDeep(defaultMenuProps);
    }
    return menu.map(d => {
        let children = d[menuProps['children']];
        const indexSymbol = symbol;
        if (children && children.length) {
            // if (level === 1) {
            //     indexSymbol = `/s${padStart(++index, 3)}-`;
            // }
            children = formatMenu({
                menu: children,
                menuProps,
                symbol: indexSymbol,
                level: level + 1
            });
        }
        const menuRoute = menuProps['route'];
        let route = d[menuRoute];
        // if (+d?.Value?.Funtype === 1) {
        route = MicroUtils.formatRoute(d[menuRoute], indexSymbol);
        // }
        const result = {
            ...d,
            symbol: indexSymbol,
            [menuProps['route']]: route
        };
        if (children && children.length) {
            result[menuProps['children']] = children;
        } else {
            delete result[menuProps['children']];
        }
        return result;
    });
};

/**
 * @method systemInitial 系统初始化
 */
export const systemInitial = async ({ loadingTarget }) => {
    let loadingInstance = EcpUI.Loading.service({
        target: loadingTarget || document.querySelector('#container'),
        body: !loadingTarget,
        fullscreen: true,
        background: 'rgba(255,255,255, 0.85)',
        text: '请稍等...',
        lock: true
    });

    /**
     * 获取配置 START
     */
    try {
        await store.dispatch('getGlobalConfigs');
        if (loadingInstance) {
            loadingInstance.text =
                store.state.globalConfigs.IMPORT_CONFIGS.title;
        }
    } catch (error) {
        console.log(
            '%c [SYSTEM INITIAL] getGlobalConfigs Caught Error',
            'font-size:18px;color:red;font-weight:700;',
            error
        );
    }
    /**
     * 获取配置 END
     */

    // // 免登录处理, 有需要可以放开这段
    // /**
    //  * 通过链接传入的Usercode登录 START
    //  */
    // const queryStr = location.hash.split('?')[1];
    // const initQuery = qs.parse(queryStr);
    // const userCode = initQuery?.usercode;

    // try {
    //     if (userCode) {
    //         await store.dispatch('loginWithUserCode', userCode);
    //     }
    // } catch (error) {
    //     console.log(
    //         `%c [SYSTEM INITIAL] loginWithUserCode ${userCode} Caught Error`,
    //         'font-size:18px;color:red;font-weight:700;',
    //         error
    //     );
    // }
    // /**
    //  * 通过链接传入的Usercode登录 END
    //  */

    /**
     * 获取用户信息 START
     */
    try {
        // await store.dispatch('getUserInfo'); // 实际使用请放开这行

        await store.dispatch('permission/getPermission');
    } catch (error) {
        console.log(
            '%c [SYSTEM INITIAL] Initial Permission Caught Error',
            'font-size:18px;color:red;font-weight:700;',
            error
        );
    }
    /**
     * 获取用户信息 END
     */

    loadingInstance?.close && loadingInstance.close();
    loadingInstance = null;
};

/**
 * @method reformatSubSystem 处理qiankun加载子应用样式优先级有误的bug
 * portal 内调用
 * @param {*} appName
 */
export const reformatSubSystem = appName => {
    // eslint-disable-next-line
    const portalAppName = `${__webpack_public_path__.replace(
        /^\/+|\/+$/g,
        ''
    )}`;
    appName = appName || portalAppName; // 子应用标识名
    let head = document.querySelector('head');
    // 为避免 MicroApp的cache缓存 重复加载样式，需要先把这些特定标记标签给干掉
    head.querySelectorAll(`[system-symbol="${appName}"]`).forEach(
        elem => elem && elem.remove()
    );
    // 在head的portal最后一个标签的后面追加，以保证追加的标签是在页面所有css的前面
    let portalTags = head.querySelectorAll('[data-symbol="portal"]');
    let targetPortalTag = portalTags && portalTags[portalTags.length - 1];
    // 找到 qiankun 指定的 DOM
    const wrapper = document.querySelector('#sub-wrapper');
    // 找到需要处理的标签起始点，往前逐个处理（这里以子应用原本的noscript标签作为查找起始点，因为是在head的portal最后一个标签的后面追加，所以要处理的标签顺序应该是从后往前处理）
    let elem = wrapper && wrapper.querySelector('noscript');
    let elemList = []; // 需要删除的标签列表
    const setPreviousElementSibling = () => {
        elem = elem && elem.previousElementSibling;
        if (elem) {
            // 仅样式需要处理
            if (
                elem.localName === 'style' ||
                (elem.localName === 'link' &&
                    (!elem.rel || elem.rel.match(/^style/)))
            ) {
                const elemClone = elem.cloneNode(true);
                elemClone.setAttribute('system-symbol', appName);
                // 插入样式
                targetPortalTag.after(elemClone);
                elemList.push(elem);
            }
            setPreviousElementSibling();
        }
    };
    setPreviousElementSibling();
    // 要删掉有问题的style才能使样式正常
    elemList.forEach(
        duplicatedElem => duplicatedElem && duplicatedElem.remove()
    );

    document.body.setAttribute('class', [appName, portalAppName].join(' '));
};

/**
 * @method resumeSubSystemManual 处理手动加载的子应用销毁后主应用样式有误的的bug
 * @param systemName 子应用名称
 * @param systemContainer 子应用包裹容器（即 loadMicroApp 入参 container 的那个DOM）
 */
export const resumeSubSystemManual = (systemName, systemContainer) => {
    const pathname = window.location.pathname;
    let elemList = []; // 需要删除的标签列表
    let head = document.querySelector('head');
    let elem = systemContainer.querySelector('noscript');
    function setNextElementSibling () {
        elem = elem && elem.nextElementSibling;
        if (elem) {
            // 仅样式需要处理
            if (
                elem.localName === 'style' ||
                (elem.localName === 'link' &&
                    (!elem.rel || elem.rel.match(/^style/)))
            ) {
                const elemClone = elem.cloneNode(true);
                elemClone.setAttribute('system-symbol', systemName);
                // 插入样式
                head.appendChild(elemClone);
                elemList.push(elem);
            }
            setNextElementSibling();
        }
    }
    if (pathname && systemName && pathname.match(systemName)) {
        setNextElementSibling();
        elemList.forEach(
            duplicatedElem => duplicatedElem && duplicatedElem.remove()
        );
    }
};
