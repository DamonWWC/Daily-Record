module.exports = {
    plugins: {
        tailwindcss: {},
        autoprefixer: {},
        'postcss-pxtorem': {
            rootValue: 16,
            propList: ['*'],
            // unitPrecision: 5,
            // minPixelValue: 12,
            exclude: ['node_modules']
        }
    }
};
