const path = require('path');
const config = require('../config/server-config');
const UniServer = require('@ecp/uni-server');
const { monitor } = require('@ecp/uni-monitor');

const MainServer = UniServer.server;

const server = new MainServer({
    context: path.resolve(__dirname, './'),
    config,

    // app实例的扩展点：preload，用于对app实例进行扩展的预配置
    preload (app, config) {
        /* 允许跨域访问
        app.all('*', (req, res, next) => {
            // 允许跨域访问
            res.header('Access-Control-Allow-Origin', '*');
            res.header('Access-Control-Allow-Headers', '*');
            res.header('Access-Control-Allow-Methods', 'PUT,POST,GET,DELETE,OPTIONS');
            if (req.method === 'OPTIONS') {
                res.send(200); // 让options请求快速返回
            } else {
                next();
            }
        });
        */
    },

    // app实例的扩展点：middleware，用于插入自定义的路由等
    middleware (app, config) {
        /* 作为主应用时请放开以下注释
        // 注释 Start
        const renderIndex = (req, res) => {
            res.sendFile(path.resolve(__dirname, '../dist/index.html'));
        };

        // 自定义的路由都指向首页
        app.get('/', renderIndex);
        app.get('/s-*', renderIndex);

        // 子模块监控
        // app.use('/monitor', monitor(app, config, { title: '多维模块监控' }));

        // 注释 End
        */
    }
});
