const state = {
    wpfData: {}, // wpf数据
    isClient: false
};

const mutations = {
    setWpfData: (state, data) => {
        state.wpfData = data;
    },
    setIsClient (state, params) {
        state.isClient = !!params;
    }
};

export default {
    name: 'wpf',
    namespaced: true,
    state,
    mutations
};
