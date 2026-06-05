import store from '@/app/store';

class Wpf {
    /**
 * @description 订阅方法
 * @param {Object} sData H5传递给大屏的数据
 * --- command 命令码
 * --- param 内容体
 */
    async setCallBack (sData) {
        if (!window.CefSharp) return;
        let obj = {
            command: sData.command,
            param: sData.param
        };
        await window.CefSharp.BindObjectAsync('boundEventHandler');
        window.boundEventHandler.raiseEvent('wpfToH5', JSON.stringify(obj));
    }
    /**
 * @description wpf执行回调方法,同步
 * --- command 命令码
 * --- data 内容体
 */
    receive () {
        return new Promise(resolve => {
            window.wpfToH5 = (result = '{}') => {
                let res = JSON.parse(result);
                store.commit('wpf/setWpfData', res);
                console.log('wpfToH5 receive:', res);
                resolve(res);
            };
        });
    }
    /**
 * @description h5通知wpf,wpf不用返回东西
 * @param {Object} data 传递数据
 */
    async emit (sData = {}) {
        console.log('emit', sData);
        if (!window.CefSharp) return;
        let obj = {
            command: sData.command,
            param: sData.param
        };
        await window.CefSharp.BindObjectAsync('boundEventHandler');
        window.boundEventHandler.raiseEvent('test', JSON.stringify(obj));
    }
    /**
 * @description H5给大屏，同时大屏返回消息
 * @param {Object} data 传递给大屏的数据
 * --- command 命令码
 * --- param 内容体
 */
    async toPie (data = {}) {
        this.setCallBack(data);
        let res = await this.receive();
        return res;
    }
}
export default new Wpf();
