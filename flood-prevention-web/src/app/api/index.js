import './interceptors';

const reg = /.\/([\w-]+)\/index.js/;

const context = require.context('./', true, /\/([\w-]+)\/index.js$/);

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
        prev = Reflect.ownKeys(context(item)).reduce((prev1, key) => {
            const { name, ...store } = context(item)[key];
            prev1[name || key] = store;
            return prev1;
        }, prev);
    }
    return prev;
}, {});

export default modules;
