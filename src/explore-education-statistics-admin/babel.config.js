module.exports = {
  inputSourceMap: true,
  sourceMaps: true,
  env: {
    test: {
      plugins: ['babel-plugin-transform-import-meta'],
      presets: [
        ['react-app', { flow: false, typescript: true }],
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
