<template>
    <div class="portal" id="container">
        <portal-layout :name="title" :showNav="!isFreeNav" @select="goto" @goBack="goBack" :menu="menu" :props="menuProps" :defaultPath="config.defaultPath">
            <template #content>
                <div id="sub-wrapper"></div>
            </template>
        </portal-layout>
    </div>
</template>

<script>
import { Utils as EcpUtils } from '@ecp/ecp-ui';
import { FreeNavConfig } from './config';
import PortalLayout from './portal-layout.vue';

import store from '@/app/store';

const MicroUtils = EcpUtils.MicroUtils;

export default {
    name: 'container',
    components: {
        PortalLayout
    },
    props: {
        config: {
            type: Object,
            default: () => ({})
        },
        menuProps: {
            type: Object,
            default: () => ({})
        }
    },
    data () {
        return {
            // menu: this.config.menu,
            isFreeNav: true
        };
    },
    computed: {
        title () {
            return this.globalConfigs?.IMPORT_CONFIGS?.title || 'flood-prevention-web';
        },
        menu () {
            return this.config.menu;
        }
    },
    watch: {
        menu: {
            handler (newVal) {
                console.log(
                    '%c menu',
                    'font-size:18px;color:green;font-weight:700;',
                    newVal
                );
            },
            immediate: true,
            deep: true
        }
    },
    created () {
        // console.log(this.menuProps);
        // MicroUtils.runAfterFirstMounted(() => {
        //     const locationPath = window.location.href.replace(
        //         window.location.origin,
        //         ''
        //     );
        //     const isLocationFree =
        //         FreeNavConfig.indexOf(
        //             '/' + MicroUtils.recoverRoute(locationPath)
        //         ) !== -1;
        //     const isDefaultFree =
        //         FreeNavConfig.indexOf(
        //             '/' + MicroUtils.recoverRoute(this.config.defaultPath)
        //         ) !== -1;
        //     this.isFreeNav = isLocationFree || isDefaultFree;
        // });

        // this.caughtError();
    },
    methods: {
        goto (data) {
            let href = '';
            if (data.type === 'iframe') {
                href = MicroUtils.getIframeUrl(
                    data,
                    this.menuProps,
                    this.config.startsWith
                );
            } else if (data.type === 'open') {
                if (this.opener && !this.opener.closed) {
                    this.opener.close();
                }
                this.opener = window.open(data[this.menuProps.url]);
                return;
            } else if (data.type === 'reload') {
                // 刷掉当前页面
                window.location.href = data[this.menuProps.url];
                return;
            } else {
                href = data[this.menuProps.route];
            }

            window.history.pushState({}, '', href);
        },
        goBack () {
            window.location.href = '/';
        },
        caughtError () {
            window.eventBus.on('PORTAL_APP_MOUNT_ERROR', err => {
                if (
                    err.reason === 'cancel' ||
                    err.type === 'unhandledrejection'
                ) { return; }

                const h = this.$createElement;
                this.$msgbox({
                    title: '',
                    message: h('div', null, [
                        h('h3', { style: 'margin: -20px 0 16px 0;' }, [
                            h(
                                'i',
                                {
                                    class: 'el-icon-error',
                                    style:
                                        'color: #f5222d; font-size: 18px; margin-right: 12px;'
                                },
                                null
                            ),
                            h('span', null, '微应用加载失败，是否跳转到首页？')
                        ]),
                        h(
                            'span',
                            { style: 'color: rgba(0, 0, 0, 0.45)' },
                            `错误信息：${err.message}`
                        )
                    ]),
                    showCancelButton: true,
                    closeOnClickModal: false,
                    showClose: false,
                    confirmButtonText: '确 定',
                    cancelButtonText: '取 消',
                    callback: () => {},
                    beforeClose: (action, instance, done) => {
                        if (action === 'confirm') {
                            instance.confirmButtonLoading = true;
                            instance.confirmButtonText = '跳转中...';
                            setTimeout(() => {
                                window.location.href = '/';
                                done();
                                setTimeout(() => {
                                    instance.confirmButtonLoading = false;
                                }, 300);
                            }, 1000);
                        } else {
                            done();
                        }
                    }
                });
            });
        }
    }
};
</script>

<style lang="scss">
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
</style>

<style lang="scss">
@mixin full {
    width: 100%;
    height: 100%;
}

#container {
    font-family: 'Avenir', Helvetica, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    color: #2c3e50;
    @include full();
    background: #eff3f9;
}
.container-nav {
    color: rgb(101, 250, 255);
    cursor: pointer;
    margin-left: 20px;
    padding: 0 8px;
    height: 30px;
    line-height: 30px;
}
.container-nav:hover {
    color: rgb(52, 144, 167);
}

#sub-wrapper {
    @include full();
    [id^='__qiankun_microapp_wrapper'] {
        @include full();
    }
}
</style>
