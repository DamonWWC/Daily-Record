import Directives from './directives';
import Components from './components';
import Filters from './filters';
import Utils from './utils';

import { DefaultMixins } from './mixins';

export default {
    install: (Vue) => {
        Components.forEach(component => {
            Vue.component(component.name, component);
        });
        for (let name in Directives) {
            Vue.directive(name, Directives[name]);
        }
        for (let [name, Filter] of Object.entries(Filters)) {
            Vue.filter(name, Filter);
        }

        Object.values(DefaultMixins).forEach(mixin => Vue.mixin(mixin));

        Vue.prototype.$utils = Object.assign(Vue.prototype.$utils ? Vue.prototype.$utils : {}, Utils);
    }
};
