module.exports = {
  presets: [['react-app', { flow: false, typescript: true }]],
  plugins: ['@babel/plugin-proposal-class-properties'],
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
