export const DemoApi = {
    getList (params) {
        return Axios({
            url: '/mock/list/getList',
            method: 'get',
            params
        });
    }
};
