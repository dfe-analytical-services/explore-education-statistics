const compose = require('lodash/fp/compose');
const withTypescript = require('@zeit/next-typescript');
const cssLoaderConfig = require('@zeit/next-css/css-loader-config');
const withSass = require('@zeit/next-sass');
const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');
const withImages = require('next-images');
const path = require('path');

const createPlugin = plugin => {
  return (nextConfig = {}) =>
    Object.assign({}, nextConfig, {
      webpack(config, options) {
        plugin(config, options, nextConfig);

        if (typeof nextConfig.webpack === 'function') {
          return nextConfig.webpack(config, options);
        }

        return config;
      },
    });
};

const withFonts = createPlugin((config, options) => {
  const { isServer } = options;

  config.module.rules.push({
    test: /\.(woff|woff2|eot|ttf|otf)$/,
    use: [
      {
        loader: 'url-loader',
        options: {
          limit: 8192,
          fallback: 'file-loader',
          publicPath: '/_next/static/fonts/',
          outputPath: `${isServer ? '../' : ''}static/fonts/`,
          name: '[name]-[hash].[ext]',
        },
      },
    ],
  });

  return config;
});

const withSassModules = createPlugin((config, options) => {
  const { dev, isServer } = options;

  const sassModuleLoader = cssLoaderConfig(config, {
    extensions: ['scss', 'sass'],
    cssModules: true,
    dev,
    isServer,
    loaders: [
      {
        loader: 'sass-loader',
        options: {},
      },
    ],
  });

  config.module.rules.push(
    {
      test: /\.module.scss$/,
      use: sassModuleLoader,
    },
    {
      test: /\.module.sass$/,
      use: sassModuleLoader,
    },
  );

  return config;
});

const config = {
  webpack(config, options) {
    const { isServer } = options;

    if (isServer) {
      config.plugins.push(
        new ForkTsCheckerPlugin({
          tsconfig: path.resolve(__dirname, '../tsconfig.json'),
        }),
      );
    }

    config.resolve.alias.src = path.resolve(__dirname);

    return config;
  },
};

module.exports = compose(
  withFonts,
  withImages,
  withSass,
  withSassModules,
  withTypescript,
)(config);
