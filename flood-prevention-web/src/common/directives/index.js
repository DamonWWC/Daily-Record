let Directives = {
    // 如有用到export default，请在此处添加
};

const context = require.context('./', true, /\.js$/);

Directives = context
    .keys()
    // 去掉入口和只保留其他子模块的入口文件
    .filter(p => p !== './index.js' && !/.\/\w+\/((?!index\.js).)+$/.test(p))
    .reduce((allDirectives, current) => {
        const module = context(current);
        allDirectives = {
            ...allDirectives,
            ...module
        };
        return allDirectives;
    }, Directives);

export default Directives;
