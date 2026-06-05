export default {
    name: 'planSolve', // 建议打开 后续新增模块过多时区分
    namespaced: true, // 建议打开 后续新增模块过多时区分
    state: () => {
        return {
            isProcessStart: false, // 处置流程开始与否
            planSolveParam: {},
            isReady: false // 标记获取预案处置参数完毕
        };
    },
    mutations: {
        SET_PROCESSSTART (state, value) {
            state.isProcessStart = value;
        },
        SET_PLANSOLVEPARAM (state, value) {
            state.planSolveParam = value;
            state.isReady = true;
        }
    }
};
