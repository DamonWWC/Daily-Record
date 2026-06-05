/* 开发环境 */
// const FRONTEND_HOST = '172.25.22.160';
// const API_URL = '172.25.22.160:9081';
// const SSO_URL = '172.25.22.160:30922';
// const FILE_URL = '172.25.22.160:9000';

/* 测试环境 */
const FRONTEND_HOST = '172.25.22.160';
const API_URL = '172.25.22.160:9081';
const SSO_URL = '172.25.22.160:30922';
const FILE_URL = '172.25.22.160:29000';
// easymock 项目地址 http://172.25.20.65:7300/project/639ad669b218da0016ccb763
const MOCK_URL = 'http://172.25.20.65:7300';

module.exports = {
    '^/api/mock': {
        target: MOCK_URL,
        pathRewrite: {
            '/api/mock': '/mock/639ad669b218da0016ccb763'
        }
    },
    /**
     * 接口代理 Start
     */
    // 在网关注册的服务
    '^/(api|sso|sysmanager)': {
        target: `http://${API_URL}`
    },
    // , '^/sso': {
    //     target: `http://${API_URL}`
    // }
    /**
     * 接口代理 End
     */

    /**
     * 前端代理 Start
     */
    // 公服
    '^/common-frontend': {
        target: `http://${SSO_URL}`
    },
    // 公服-iframe 嵌入用
    '^/c-common-frontend': {
        target: `http://${SSO_URL}`,
        pathRewrite: {
            '/c-': '/s-'
        }
    },

    // usePortal时，本地调试作为子应用使用 registerMicroApps 加载的当前应用, 不需要就注掉这段
    '^/flood-prevention-web': {
        // Dev环境
        target: 'http://localhost:8992',
        // 线上环境
        // target: `http://${FRONTEND_HOST}:8992`,

        // 只要有作为主应用加载，这里代理的同上下文子应用就必须加上路径重写
        pathRewrite: {
            '/flood-prevention-web': ''
        }
    },
    // usePortal时，本地调试作为子应用使用 loadMicroApp 加载的当前应用(即当成另外一个普通子应用使用，需要临时手动修改packageName为代理匹配的上下文，作为与修改后的packageName同名的子应用服务需要注掉这段), 不需要就注掉这段
    '^/x-flood-prevention-web': {
        // Dev环境
        target: 'http://localhost:8081'

        // 线上环境
        // target: `http://${FRONTEND_HOST}:8081`,
        // pathRewrite: {
        //     '/x-flood-prevention-web': ''
        // }
    },

    // 子应用-xxxxx
    '^/xxxxx-frontend': {
        // 子应用Dev环境
        target: 'http://localhost:8888'

        // // 子应用线上环境
        // target: `http://${FRONTEND_HOST}:8888`,
        // pathRewrite: {
        //     '/xxxxx-frontend': ''
        // }
    },
    '/template-vite-vue': {
        target: 'http://localhost:7100',
        pathRewrite: {
            '/template-vite-vue': '/'
        }
    },
    // 水淹评估
    '^/hjmos-wia': {
        target: 'http://10.51.9.133:30390'
    },
    // 应急指挥
    '^/nec': {
        target: 'http://10.51.9.130:30769'
        // target: 'http://172.29.20.34:8080', // 志明本地环境
        // pathRewrite: {
        //     '/nec': '/'
        // }
    },
    // 应急物资
    '^/material-manage': {
        target: 'http://10.51.9.130:30769'
    },
    // Dfs
    '^/hjmos-dfs': {
        target: 'http://10.51.9.130:30769'
    },

    '^/hjmos-passengerline': {
        target: 'http://10.51.9.130:30769'
    },
    // 业务协同处置（工作指令管理）
    '^/hjmos-wom-server': {
        target: 'http://10.51.9.130:30769'
    },
    // 基础数据
    '^/hjmos-basicdata-server': {
        target: 'http://10.51.9.130:30769'
    },
    // 信息通报
    '^/infor-notification': {
        target: 'http://10.51.9.130:30769'
    },
    '^/hjmos-device': {
        target: 'http://10.51.9.130:30769'
    },
    '^/hjmos-flood-serve': {
        target: 'http://10.51.9.130:30769'
    },
    '^/hjmos-authcenter': {
        target: 'http://10.51.9.130:30769'
    },
    '^/hjmos-mediacenter-admin': {
        target: 'http://10.51.9.130:30769'
    }
    /**
     * 前端代理 End
     */
};
