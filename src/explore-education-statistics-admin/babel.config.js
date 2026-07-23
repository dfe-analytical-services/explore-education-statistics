module.exports = {
  presets: [['react-app', { flow: false, typescript: true }]],
  assumptions: {
    setPublicClassFields: true,
    privateFieldsAsProperties: true,
  },
  plugins: [
    // needed to silence a warning about loose mode in unit tests
    // and admin webpack logs
    ['@babel/plugin-transform-class-properties'],
    ['@babel/plugin-transform-private-methods'],
    ['@babel/plugin-transform-private-property-in-object'],
  ],
  inputSourceMap: true,
  sourceMaps: true,
  env: {
    test: {
      presets: [
        [
          '@babel/preset-react',
          {
            runtime: 'automatic',
          },
        ],
      ],
    },
  },
};
