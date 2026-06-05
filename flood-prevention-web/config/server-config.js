const SERVER_CONFIG = {
    // APP_IP: '', // IP，不需要配IP
    APP_PORT: 30392, // 端口
    APP_NAME: 'hjmos-flood-prevention-web', // 应用名
    APP_ALIAS: 'hjmos-flood-prevention-web' // 别名，网关访问路径
};

// nacos配置项
const NACOS_CONFIG = {
    enabled: true, // 默认不关联nacos
    // serviceList: true, // 是否获取所有前端服务列表
    registerService: true, // 注册当前应用
    address: `${process.env.NACOS_SERVER || 'nacos-center.v-base:30848'}`, // 服务域名:端口 (如关联nacos, nacos-center.v-base是在服务器的host配的)
    namespace: 'a85a37ef-5bec-478c-a60f-0b11f10b3da4', // nacos PROD namespace, 固定值
    namespacePro: '86634aa4-90c1-4d2c-babe-fe07b660e76b',
    items: [
        // 前端配置 (其它前端配置都放这里)
        {
            dataId: 'settings-frontend',
            group: 'frontend',
            frontend: true
        },
        // 公共配置，系统应用相关配置放application里面 (代理ip、port等)
        {
            dataId: 'applications',
            group: 'prophet'
        }
    ],
    // 服务注册到nacos时，获取ip的规则，请在config.ini或者nacos上配置
    ipRules: {
        preferredNetworks: [], // 首选
        ignoredInterfaces: [] // 忽略
    }
};

// 门户需要使用的子模块，在前端服务订阅和子模块监控中，根据此列表获取
const APPS_ENTRY = [
    // 除了name，其余配置都会按 local代理配置、nacos代理配置、nacos服务订阅 的顺序更新
    /**
     * 如果ip、端口、需要修改，请在var.ini中配置变量，不要直接修改此文件，如：
     * metadata-frontend.ip=172.25.21.205
     * metadata-frontend.port=30013
     */
    {
        name: SERVER_CONFIG.APP_NAME,
        alias: SERVER_CONFIG.APP_ALIAS,
        ip: SERVER_CONFIG.APP_IP, // 注册到 nacos 的这里不要配 ip, 记得去掉这行 !!!!!
        port: SERVER_CONFIG.APP_PORT // 注册到 nacos 的这里不要配 port, 记得去掉这行 !!!!!
    },
    {
        // 公服
        name: 'common-frontend',
        alias: 'common-frontend'
    }
    // {
    //     name: 'data-govern-frontend',
    //     alias: 'dgs',
    //     ip: '', // 注册到 nacos 的这里不要配 ip, 记得去掉这行 !!!!!
    //     port: '30017' // 注册到 nacos 的这里不要配 port, 记得去掉这行 !!!!!
    // }
];

module.exports = {
    SERVER_CONFIG,
    NACOS_CONFIG,
    APPS_ENTRY
};
