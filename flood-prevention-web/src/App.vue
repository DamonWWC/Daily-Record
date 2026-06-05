<template>
    <div class="app" id="app" ref="app">
        <template v-if="!componentName">
            <app-layout name="flood-prevention-web" :menu="menu" v-if="showNav">
                <template #content>
                    <transition name="fade-transform" mode="out-in">
                        <router-view></router-view>
                    </transition>
                </template>
            </app-layout>
            <template v-else>
                <transition name="fade-transform" mode="out-in">
                    <router-view></router-view>
                </transition>
            </template>
        </template>

        <template v-else>
            <transition name="fade-transform" mode="out-in">
                <!-- 如果是路由页面作为组件加载，则使用router的matcher匹配路由对应component渲染 (以斜杠为标识, 规范的组件名是没有斜杠的) -->
                <app-route-component :target-path="componentName" v-bind="{targetProps: componentProps}" v-if="isRoute" />

                <!-- 其它组件组要在本文件内引入，或使用其它全局组件 -->
                <template v-else>
                    <component :is="componentName" v-bind="componentProps" />
                </template>
            </transition>
        </template>
    </div>
</template>

<script>
export default {
    name: 'app',
    props: {
        componentName: {
            type: String,
            default: ''
        },
        componentProps: {
            type: Object,
            default: () => ({})
        }
    },
    data () {
        return {
            // menu: [],
            mark: null
        };
    },
    computed: {
        isRoute () {
            const componentName = this.componentName;
            return !!componentName.match('/');
        },
        isSubMode () {
            return window.__POWERED_BY_QIANKUN__; // 子模块
        },
        showNav () {
            // return !this.isSubMode;
            return false;
        },
        userInfo () {
            return this.$store.state.userInfo;
        },
        globalConfigs () {
            return this.$store.state.globalConfigs;
        },
        menu () {
            return this.$store.state.permission.navMenu;
        }
    },
    watch: {
        $route: {
            handler (newVal, oldVal) {
                this.theme = newVal?.query.theme || 'darken';
            },
            deep: true,
            immediate: true
        }
    },
    mounted () {
        let key = 'data-theme';
        let themeName = 'blue';
        window.sessionStorage.setItem(key, themeName);
        document.body.setAttribute(key, themeName);

        if (!this.componentName && !this.isSubMode) {
            this.setTextMark();
        }
        this.componentName &&
            console.log(
                `%c [MicroApp] Load Component ${this.componentName}`,
                'font-size:18px;color:red;font-weight:700;',
                this.componentProps
            );
    },
    beforeDestroy () {
        this.mark && this.mark.remove();
    },
    methods: {
        setTextMark () {
            if (this.globalConfigs?.IMPORT_CONFIGS?.textMark === false) return;
            if (!this.userInfo?.UserName) return;
            if (this.mark) this.mark.remove();
            this.mark = new this.$utils.Textmark(this.$refs['app']);
            this.mark.setOptions({
                text: [this.userInfo.UserName]
            });
        }
    }
};
</script>
<style lang="scss" scoped>
/* fade-transform */
.fade-transform-leave-active,
.fade-transform-enter-active {
    transition: all 0.2s;
}

.fade-transform-enter {
    opacity: 0;
    transform: translateX(-30px);
}

.fade-transform-leave-to {
    opacity: 0;
    transform: translateX(30px);
}
#app {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
}
</style>
