<docs>
# 根据路由手动加载子应用页面

## props
- targetPath 页面完整链接
- targetProps 页面组件props

## events
- @render-error 渲染错误或其他微前端报错，传参错误信息
- @before-render 手动加载微前端前hooks, 传参microApp实例
- @render-complate 手动加载微前端后hooks, 传参microApp实例

</docs>
<template>
    <div class="micro-component-content-flood-prevention-web" :system-name="systemName" id="micro-component-content-flood-prevention-web" :key="targetPath" v-loading="renderLoading" ref="micro-component-content-flood-prevention-web"></div>
</template>

<script>
import EcpUI, { Utils as EcpUtils } from '@ecp/ecp-ui';
import {
    addGlobalUncaughtErrorHandler,
    removeGlobalUncaughtErrorHandler
} from 'qiankun';
export default {
    name: 'app-micro-component',
    props: {
        targetPath: {
            type: String,
            default: ''
        },
        targetProps: {
            type: Object,
            default: () => ({})
        }
    },
    data () {
        return {
            microApp: null,
            renderLoading: false,

            systemName: ''
        };
    },
    mounted () {
        this.initMicroApp();
        addGlobalUncaughtErrorHandler(this.handleError);
    },
    beforeDestroy () {
        this.unmountMicroApp();
    },
    methods: {
        async unmountMicroApp () {
            try {
                removeGlobalUncaughtErrorHandler(this.handleError);
                if (typeof this.microApp?.unmount === 'function') {
                    await this.microApp.unmount();
                }
                // this.microApp = null;
                this.$set(this, 'microApp', null);
                return Promise.resolve();
            } catch (error) {
                return Promise.reject(error);
            }
        },
        handleError (e) {
            this.renderLoading = false;
            console.log('render-error', e);
            this.$emit('render-error', e);
        },
        initMicroApp () {
            const { MicroUtils } = EcpUtils;
            const arr = _.cloneDeep(this.targetPath.match(/(\S*)#\//));
            if (arr && arr.length >= 2) {
                const entry = arr[1];
                const symbolName = entry.replace(/^\/|\/$/, '');
                this.systemName = symbolName;
                this.microApp = MicroUtils.loadMicroApp(
                    {
                        name: `flood-prevention-web-micro-component-${symbolName}`,
                        entry,
                        container: this.$refs['micro-component-content-flood-prevention-web'],
                        props: {
                            componentName: this.targetPath.replace(entry, ''),
                            componentProps: this.targetProps
                        }
                    },
                    {
                        autoStart: true
                    },
                    {
                        beforeLoad: () => {
                            this.renderLoading = true;
                            if (window?.Vue?.use) {
                                window.Vue._use = window.Vue.use;
                                window.Vue.use = (data) => {
                                    if (data?.name !== 'VueRouter') {
                                        window.Vue._use(data);
                                    }
                                };
                            }
                        },
                        beforeMount: () => {
                            this.renderLoading = true;
                            this.$emit('before-render', this.microApp);
                            // console.log('app before load');
                        },
                        afterMount: () => {
                            this.renderLoading = false;
                            console.log('render-complate');
                            this.$emit('render-complate', this.microApp);
                            // console.log('app after mount');
                        },
                        beforeUnmount: () => {
                            removeGlobalUncaughtErrorHandler(this.handleError);
                        },
                        afterUnmount: () => {
                            if (window?.Vue?._use) {
                                window.Vue.use = window.Vue._use;
                            }
                        }
                    }
                );
            } else {
                this.handleError(
                    new Error(`microApp ${this.targetPath} isn't existed`)
                );
            }
        },
        getContainerEl () {
            return this.$refs['micro-component-content-flood-prevention-web'];
        }
    }
};
</script>
<style lang="scss">
.micro-component-content-flood-prevention-web {
    > div:first-child {
        display: flex;
        min-width: 100%;
        min-height: 100%;
        flex-direction: column;

        > #app.sub-module {
            display: flex;
            flex-direction: column;
            min-width: 100%;
            min-height: 100%;
            flex-grow: 1;
            flex-shrink: 0;
            > .ecp-panel {
                flex-grow: 1;
                flex-shrink: 0;
            }
        }
    }
}
</style>
