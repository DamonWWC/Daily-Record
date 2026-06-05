<docs>
# 路由匹配渲染组件

## props
- needInit 是否需要调用项目初始化接口，主要用于作为手动拉取的子应用组件，默认 false
- targetName 展示名称，用于加载失败时展示用
- targetPath 页面完整链接
- targetProps 页面组件props

</docs>

<script>
export default {
    name: 'app-route-component',
    props: {
        needInit: {
            type: Boolean,
            default: false
        },
        targetName: {
            type: String,
            default: ''
        },
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
        return {};
    },
    render (h) {
        this.$nextTick(() => {
            this.getRenderComponent();
        });
        return h();
    },
    renderError (h, err) {
        console.log(
            '%c renderError',
            'font-size:18px;color:red;font-weight:700;',
            err?.stack
        );
        return h();
    },
    methods: {
        async getRenderComponent () {
            try {
                if (this.needInit) {
                    await this.$store.dispatch('permission/getPermission');
                }
                let targetPath = this.targetPath;
                // 使用 $router 的 matcher获取到匹配路由list
                targetPath = (targetPath || '').replace(/.*#/, '');
                const matchedRoute = this.$router?.matcher.match(targetPath);

                // 获取路由参数
                const matchedRouteQuery = matchedRoute?.query;
                const matchedRouteParams = matchedRoute?.params;

                // 获取匹配路由component
                const matchedLength = matchedRoute?.matched?.length || 1;
                const matchedComponent =
                    matchedRoute?.matched?.[matchedLength - 1]?.components
                        ?.default;
                if (!matchedComponent) {
                    throw new Error(`${this.targetName || ''}加载失败！`);
                }
                const targetComponentModule =
                    typeof matchedComponent === 'function'
                        ? await matchedComponent()
                        : { default: matchedComponent };
                const targetComponent = targetComponentModule?.default;

                // 创建匹配路由component VNode
                const targetComponentVNode = this.$createElement(
                    targetComponent,
                    {
                        attrs: {
                            query: matchedRouteQuery // 因为query类参数传入较随意, 所以通过attrs传入
                        },
                        props:
                            { ...this.targetProps, ...matchedRouteParams } || {} // params类参数一般会确定有那些参数, 因此通过props传入
                    }
                );
                // 使用 Vue 的 _update 方法渲染 VNode
                this._update(targetComponentVNode);
                return targetComponent;
            } catch (error) {
                console.log(
                    '%c matchComponent Caught Error',
                    'font-size:18px;color:red;font-weight:700;',
                    error
                );
                this.$message.error(error.message);
            }
        }
    }
};
</script>
