export const example = {
    test: function () {
        return Axios.get('/test').then((response) => {
            return response.data;
        });
    }
};
