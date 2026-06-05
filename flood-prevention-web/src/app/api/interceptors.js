import { Message } from '@ecp/ecp-ui';
import { LoginUtils } from 'ecp-login-component';
import * as Prefix from './prefix.config';
import Cookies from 'js-cookie';

// // 如使用自定义登录页, 用这个↓
// // eslint-disable-next-line
// const loginUrl = `${window.location.origin}/${window.__POWERED_BY_QIANKUN____?'flood-prevention-web':__webpack_public_path__.replace(/^\/+|\/+$/g, '')}/login.html`;
const loginUrl = '';

// 设置默认请求头信息
Axios.defaults.headers.post['Content-Type'] = 'application/json;charset=UTF-8';
Axios.defaults.headers.delete['Content-Type'] =
    'application/json;charset=UTF-8';
Axios.defaults.headers.put['Content-Type'] = 'application/json;charset=UTF-8';

// 新建请求拦截器
Axios.interceptors.request.use(
    // 正常请求拦截
    requestConfig => {
        requestConfig.baseURL = Prefix.BaseUrl;
        const mosToken = Cookies.get('mosToken');
        const authorization = `Basic MTAxOjEyMzQ1Ng==`; // 暂时写死，正常情况下应该使用Base编码根据系统编号生成
        if (mosToken) {
            requestConfig.headers['ID-Token'] = mosToken;
        }
        requestConfig.headers['Authorization'] = authorization;
        return requestConfig;
    },
    // 错误请求拦截
    error => {
        return Promise.reject(error);
    }
);

async function fileToJson (file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = res => {
            const { result } = res.target; // 得到字符串
            try {
                const data = JSON.parse(result); // 解析成json对象
                resolve(data);
            } catch {
                resolve(file);
            }
        }; // 成功回调
        reader.onerror = err => {
            reject(err);
        }; // 失败回调
        reader.readAsText(new Blob([file]), 'utf-8'); // 按照utf-8编码解析
    });
}

// 新建响应拦截器
Axios.interceptors.response.use(
    async response => {
        // mock接口数据返回数据为随机数据，不做拦截
        if (response.config.url.includes('mock')) {
            return response;
        }

        if (response.data instanceof Blob) {
            let disposition = response.headers['content-disposition'];
            if (disposition && disposition.includes('attachment;')) {
                // 文件下载
                return response;
            } else {
                response.data = await fileToJson(response.data);
            }
        } else {
            // 登录拦截处理
            // if (response?.data?.OpCode === 403) {
            //     const param = {
            //         response: response.data,
            //         loginUrl: loginUrl || window.location.origin + '/sso/login',
            //         appName: PACKAGE_NAME
            //         // , isComplexNetwork: true // 如应用部署网络环境复杂(例如: 需要支持非内网访问等), 请放开isComplexNetwork
            //     };
            //     LoginUtils.loginInterceptors(param);
            //     return response;
            // }

            // 没有OpCode，先特殊处理
            if (!(response?.data?.OpCode === 0 || response?.data?.code === 0)) {
                // if (response.data.OpCode !== 0) {
                console.log('request error ===> ', response.config, response.data);
                Message.error({
                    message: response.data.OpDesc || '服务访问异常',
                    showClose: true
                });
                return Promise.reject(new Error(response.data.OpDesc));
            }
        }
        return response;
    },
    // 错误响应拦截
    error => {
        Message.error(error.message || '服务访问异常');
        return Promise.reject(error);
    }
);
