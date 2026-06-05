module.exports = {
    presets: [
        ['@vue/app',
            {
                polyfills: [
                    'es6.promise',
                    'es6.symbol'
                ]
            }
        ],
        [
            '@babel/preset-env',
            {
                'modules': false
            }
        ]
    ],
    sourceType: 'unambiguous', // 严格区分import 和 require 不可混用
    'plugins': [
        '@babel/plugin-proposal-class-properties',
        '@babel/plugin-proposal-optional-chaining'
    ]
};
