/*
 * 全局组件自动注册
 *
 * 注意：需要全局组成的组件文件名都需要以app-开头
 */
const requireComponent = require.context('.', true, /app-[\w-]+\.vue$/);

export default requireComponent.keys().map(fileName => {
    // 获取组件配置
    const componentConfig = requireComponent(fileName);
    const ctrl = componentConfig.default || componentConfig;

    return ctrl;
});
