/**
 * @type {import('postcss-load-config').Config}
 */
module.exports = {
  parser: 'postcss-scss',
  plugins: [
    require('postcss-flexbugs-fixes'),
    require('postcss-preset-env')({
      autoprefixer: {
        flexbox: 'no-2009',
      },
      stage: 3,
    }),
    require('postcss-normalize'),
  ],
};
