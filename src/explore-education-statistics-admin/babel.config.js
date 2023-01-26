module.exports = {
  presets: [['react-app', { flow: false, typescript: true }]],
  plugins: [
    // needed to silence a warning about loose mode in unit tests
    // and admin webpack logs
    ['@babel/plugin-proposal-class-properties', { loose: true }],
    ['@babel/plugin-proposal-private-methods', { loose: true }],
    ['@babel/plugin-proposal-private-property-in-object', { loose: true }],
  ],
  inputSourceMap: true,
  sourceMaps: true,
  env: {
    test: {
      plugins: [
        'explore-education-statistics-common/babel-url-import-meta-plugin.js',
      ],
    },
  },
};
