/**
 * 通过 require.context() 函数来创建自己的 context，自动引入其他子模块
 * 子模块出口文件默认export default subModeleName
 * 可在子模块中添加name作为store modules的key
 * 或者以文件路径中的子模块目录为store modeules的key
 *
 * 如果子模块出口export const subModuleName
 * 同上可自定义name
 * 或者以subModuleName为store modeules的key
 *
 */

const reg = /.\/(\w+-?\w*)\/index.js/;

const context = require.context('./', true, /\/\w+-?\w*\/index.js$/);

const modules = context.keys().reduce((prev, item) => {
    // subModuleDirName => ./xxx/index.js => xxx
    if (context(item).default) {
        const { name, ...store } = context(item).default;
        let subModuleDirName = '';
        if (reg.test(item)) {
            subModuleDirName = item.match(reg)[1];
        }
        prev[name || subModuleDirName] = store;
    } else {
        prev = Object.keys(context(item)).reduce((prev1, key) => {
            const { name, ...store } = context(item)[key];
            prev1[name || key] = store;
            return prev1;
        }, prev);
    }
    return prev;
}, {});

export default modules;
