module.exports = {
  presets: [['react-app', { flow: false, typescript: true }]],
  plugins: ['@babel/plugin-proposal-class-properties'],
  inputSourceMap: true,
  sourceMaps: true,
};
