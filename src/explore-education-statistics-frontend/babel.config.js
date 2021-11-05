module.exports = {
  presets: [
    [
      'next/babel',
      {
        'preset-env': {
          useBuiltIns: 'entry',
          corejs: 3,
          targets: {
            browsers: ['ie>=11', 'safari>=12', 'ios>=9.3', 'last 1 version'],
          },
        },
      },
    ],
  ],
  env: {
    test: {
      plugins: [
        'explore-education-statistics-common/babel-url-import-meta-plugin.js',
      ],
    },
  },
};
