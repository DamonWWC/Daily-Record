const path = require('path');
const config = require('../config/server-config');
const UniServer = require('@ecp/uni-server');

UniServer.init(path.resolve(__dirname, './'), config);
