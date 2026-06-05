import Vue from 'vue';
import Router from 'vue-router';
import { ViewRoutes } from '../views/view-route';

Vue.use(Router);

/**
// 作为子模块不能添加默认路由
// TODO 存疑, 不加载默认路由怎么处理子应用页面404的情况？
if (window.__POWERED_BY_QIANKUN__) {
    ViewRoutes.pop();
}
*/

export default new Router({
    routes: ViewRoutes
});
