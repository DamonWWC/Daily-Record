<docs>
# layout最外层布局统一组件
- 包含两部分内容：头部导航菜单栏 + 主体部分（高度为可视区域高度 - 菜单栏高度）
- 主体是一个插槽，name 是 content。

## 使用场景（注：后面使用过此组件人员但列表中没有该场景值时，请自行在下面添加对应的场景）
- 统一给各个不同中心使用（例如管理控制台、应用中心）

## props
- name: String，对应中心的名称（头部菜单栏的标题），例如 '管理控制台'
- menu: Array，头部导航栏菜单的数据
</docs>

<template>
    <div class="app-layout">
        <!-- <ecp-menu :data="menu" :props="props" @menuClick="handleSelect" :maxWidth="maxMenuContentWidth"> -->
        <!-- 以上一行为旧的菜单收起功能实现，1.0.19之后提供新的方式（旧的方式仍然兼容），使用方式如下： -->
        <ecp-menu :data="menu" :props="props" @menuClick="handleSelect" :auto-resize="true" :toolBoxWidth="32">
            <div class="app-layout__header-title ecp-menu-title" slot="title">
                <img :src="config.logo" alt="">
                <span class="ecp-menu-title-text">{{ name }}</span>
            </div>
            <div class="app-layout__header-tools" slot="tools">
                <!-- <span>系统管理员</span> -->
            </div>
        </ecp-menu>
        <div class="app-layout-content">
            <slot name="content" />
        </div>
    </div>
</template>

<script>
import { EcpMenuUtils, EcpMenu } from 'ecp-menu';
import * as config from '@/constants';
export default {
    name: 'app-layout',
    components: {
        EcpMenu
    },
    props: {
        name: {
            type: String
        },
        menu: {
            type: Array,
            default: () => {
                return [];
            }
        },
        props: {
            type: Object,
            default: () => ({
                ChildNodes: 'ChildNodes',
                Text: 'Text',
                Target: 'Target'
            })
        }
    },
    data () {
        return {
            active: '/',
            maxMenuContentWidth: 600,
            config
        };
    },
    mounted () {
        this.getMaxWidth();
        window.addEventListener('resize', this.debounceMethod);
    },
    beforeDestroy () {
        window.removeEventListener('resize', this.debounceMethod);
    },
    methods: {
        getMaxWidth () {
            this.maxMenuContentWidth = document.body.clientWidth - 450;
            console.log('当前菜单最大展示宽度为>>', this.maxMenuContentWidth);
        },
        debounceMethod () {
            if (this.timer) {
                clearTimeout(this.timer);
                this.timer = setTimeout(this.getMaxWidth, 300);
            } else {
                this.timer = setTimeout(this.getMaxWidth, 300);
            }
        },
        handleSelect (menuItem) {
            this.$router.push(
                (menuItem[this.props.Target] || '').replace(/(.*#)/, '')
            );
        }
    }
};
</script>

<style lang="scss" scoped>
.app-layout {
    display: flex;
    flex-direction: column;
    width: 100%;
    height: 100vh;
    overflow: hidden;
    background: $--background-color-page;
    &__header {
        flex: 0 0 auto;
        &-tools {
            color: #fff;
            padding-right: 24px;
            height: 64px;
            line-height: 64px;
            vertical-align: middle;
        }
        &-title {
            padding-left: 32px;
            display: flex;
            align-items: center;
        }
        &-title .ecp-menu-title-text {
            margin-left: 16px;
        }
    }
    &-content {
        flex: 1 1 auto;
        height: 100%;
        overflow: auto;
    }
}
</style>
