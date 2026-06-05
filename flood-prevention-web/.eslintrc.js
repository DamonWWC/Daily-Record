module.exports = {
    root: true,
    env: {
        node: true
    },
    globals: {
        _: true,
        PACKAGE_NAME: true,
        Axios: true
    },
    'extends': [
        'plugin:vue/essential',
        '@vue/standard'
    ],
    rules: {
        'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
        'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',

        'indent': ['error', 4],
        'no-unused-vars': 'off',
        'quotes': ['error', 'single'],
        'semi': ['error', 'always'],
        'vue/no-unused-components': 'off',
        'space-before-function-paren': 0
    },
    parserOptions: {
        parser: 'babel-eslint'
    }
};
