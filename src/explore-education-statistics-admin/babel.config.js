module.exports = {
  presets: [
    ['react-app', { flow: false, typescript: true }],
    // [
    //   '@babel/preset-env',
    //   {
    //     useBuiltIns: 'entry',
    //     targets: {
    //       browsers: ['ie>=11', 'safari>=12', 'ios>=9.3', 'last 1 version'],
    //     },
    //   },
    // ],
  ],
  plugins: ['@babel/plugin-proposal-class-properties'],
};
