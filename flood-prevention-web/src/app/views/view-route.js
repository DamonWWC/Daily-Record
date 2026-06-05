import { NetFloodRoutes } from './flood-net/flood-net.route';
import { LineFloodRoutes } from './flood-line/flood-line.route';
import { ExceptionRoutes } from './exception/exception.route';
import { StationFloodRoutes } from './flood-station/flood-station.route';

export let ViewRoutes = [
    ...NetFloodRoutes(),
    ...LineFloodRoutes(),
    ...StationFloodRoutes(),
    ...ExceptionRoutes() // 默认路由需要放在最后
];
