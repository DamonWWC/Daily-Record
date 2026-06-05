import axios from 'axios';
import { Loading, Message } from '@ecp/ecp-ui';
import { LoginUtils } from 'ecp-login-component';
import { Sso, Sysmanager } from '../prefix.config';

const AxiosService = axios.create();
AxiosService.defaults.withCredentials = true; // 让ajax携带cookie

// // 如使用自定义登录页, 用这个↓
// // eslint-disable-next-line
// const loginUrl = `${window.location.origin}/${window.__POWERED_BY_QIANKUN____?'flood-prevention-web':__webpack_public_path__.replace(/^\/+|\/+$/g, '')}/login.html`;

const loginUrl = '';

export const Login = {
    logout (requestOnly) {
        return new Promise((resolve, reject) => {
            let loadingInstance = Loading.service({
                target: document.querySelector('#app'),
                body: false,
                fullscreen: true,
                lock: true
            });
            AxiosService.get(`${Sso}/logout`)
                .then(res => {
                    if (requestOnly) {
                        resolve();
                    } else {
                        if (res.data.OpCode === 0) {
                            window.location.href = loginUrl;
                        } else if (res.data.OpCode === 403) {
                            this.goToLoginPage(res.data);
                        } else {
                            const messageText =
                                res.data.OpDesc || '服务访问异常';
                            Message({
                                type: 'error',
                                message: messageText
                            });
                            reject(new Error(messageText));
                        }
                    }
                })
                .catch(error => {
                    reject(error);
                })
                .finally(() => {
                    loadingInstance &&
                        loadingInstance.close &&
                        loadingInstance.close();
                });
        });
    },
    goToLoginPage (resData) {
        var param = {
            response: resData,
            loginUrl,
            appName: PACKAGE_NAME
            // , isComplexNetwork: true // 如应用部署网络环境复杂(例如: 需要支持非内网访问等), 请放开isComplexNetwork
        };
        LoginUtils.loginInterceptors(param);
    },
    tokenLogin (params) {
        return AxiosService.get(`${Sso}/generateToken`, { params }).then(
            res => res.data.Data
        );
    }
};
