/*
 * 工具库的自动注册
 *
 * 注意：目前不支持export default
 *
 * * 使用方式：this.$utils，import Constants from '@utils'
 */

let Utils = {
    // 如有用到export default，请在此处添加
};

const context = require.context('./', true, /\.js$/);

Utils = context.keys()
    // 去掉入口
    .filter(p => p !== './index.js')
    .reduce((total, current) => {
        const module = context(current);
        total = {
            ...total,
            ...module
        };
        return total;
    }, Utils);

module.exports = Utils;
