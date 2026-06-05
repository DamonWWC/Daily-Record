// (整点骚操作) 重置全局组件属性默认值

const ResetDefaultProps = ({ context, name, props }) => {
    if (context.$options.name === name) {
        const targetProto = Object.getPrototypeOf(context);
        Object.keys(props).forEach(
            key =>
                targetProto?.constructor?.extendOptions?.props?.[key] &&
                (targetProto.constructor.extendOptions.props[key].default =
                    props[key])
        );
        Object.setPrototypeOf(context, targetProto);
    }
};

// 设置全局组件属性默认值
export const ResetDefault = {
    beforeCreate () {
        const self = this;
        /**
         * 有需要就在这里添加，具体参考↓
         */
        // // 如需要全局分页添加每页显示条数可放开这里
        // ResetDefaultProps({
        //     context: self,
        //     name: 'ecp-layout-pagination',
        //     props: { layout: 'prev,pager,next,sizes,jumper' }
        // });
    }
};
