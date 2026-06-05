<docs>
# layout最外层布局统一组件
- 包含两部分内容：头部导航菜单栏 + 主体部分（高度为可视区域高度 - 菜单栏高度）
- 主体是一个插槽，name 是 content。

## props
- name: String，对应中心的名称（头部菜单栏的标题），例如 '管理控制台'
- menu: Array，头部导航栏菜单的数据
- defaultPath: String，默认的选中项
</docs>

<template>
    <div class="portal-layout">
        <div class="portal-layout-header">
            <ecp-menu class="portal-layout-nav" :data="menu" :props="props" @menuClick="handleSelect" :auto-resize="true" :toolBoxWidth="32">
                <div class="portal-layout__header-title ecp-menu-title" slot="title">
                    <span class="ecp-menu-title-logo"></span>
                    <span class="ecp-menu-title-text">{{ name }}</span>
                </div>
                <div class="app-layout__header-tools" slot="tools">
                    <!-- <span>系统管理员</span> -->
                </div>
            </ecp-menu>
        </div>
        <div class="portal-layout-content" id="portal-layout-content">
            <slot name="content" />
        </div>
    </div>
</template>

<script>
import { EcpMenuUtils, EcpMenu } from 'ecp-menu';

export default {
    name: 'portal-layout',
    components: {
        EcpMenu
    },
    props: {
        name: {
            type: String
        },
        defaultPath: {
            type: String,
            default: ''
        },
        props: {
            type: Object,
            default: () => ({
                ChildNodes: 'ChildNodes',
                Text: 'Text',
                Target: 'Target',
                HasChildren: 'hasChild'
            })
        },
        menu: {
            type: Array,
            default: () => {
                return [];
            }
        }
    },
    data () {
        return {
            active: '/',
            userName: ''
        };
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
    mounted () {
        this.userName = window.UserName;
    },
    methods: {
        handleSelect (data) {
            this.$emit('select', data);
        },
        handleVersion () {
            this.$refs.VersionDesc.open();
        }
    }
};
</script>

<style lang="scss" scoped>
.portal-layout {
    $nav-height: 64px;
    width: 100%;
    height: 100%;
    background: #eff3f9;
    &-header {
        display: flex;
        flex-direction: row;
        width: 100%;
        height: $nav-height;
        color: #fff;

        &-btn {
            height: $nav-height;
            line-height: $nav-height;
            padding-right: 24px;

            .version-btn {
                color: #fff;
            }
        }
    }
    &-nav {
        // flex: 1 1 auto;
        // display: flex;
        // align-items: center;
        // height: $nav-height;
        // box-sizing: border-box;
        // font-size: 20px;
        // font-weight: bold;
    }
    &-switch {
        font-size: 14px;
        height: 60px;
        line-height: 60px;
        padding: 0 20px;
        cursor: pointer;
        color: rgba(255, 255, 255, 0.85);
        &:hover {
            color: #fff;
        }
        &-btn {
            margin-right: 8px;
        }
    }
    &-content {
        height: calc(100% - #{$nav-height});
        overflow: auto;
        box-sizing: border-box;
    }

    // 修复样式抖动的问题
    ::v-deep .ecp-menu-content .ecp-menu-wrap .ecp-menu-item {
        background: transparent;
        border-color: transparent;
    }
}
</style>
