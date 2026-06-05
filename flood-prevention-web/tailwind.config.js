module.exports = {
    purge: ['./src/app/**/*.vue'],
    darkMode: false, // or 'media' or 'class'
    theme: {
        extend: {
            colors: { // color和textColor都可以作用于字体颜色
                primary: 'var(--color-primary)',
                warning: 'var(--color-warning)'
            },
            borderColor: {
                primary: 'rgba(23,85,127,0.65);'
            },
        }
    },
    variants: {
        extend: {
            cursor: ['hover']
        }
    },
    plugins: []
};
