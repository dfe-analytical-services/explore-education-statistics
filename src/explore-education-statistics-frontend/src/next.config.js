const compose = require('lodash/fp/compose');
const withTypescript = require('@zeit/next-typescript');
const cssLoaderConfig = require('@zeit/next-css/css-loader-config');
const withSass = require('@zeit/next-sass');
const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');
const withImages = require('next-images');
const path = require('path');

const withSassModules = (nextConfig = {}) => {
  return withSass(
    Object.assign({}, nextConfig, {
      webpack(config, options) {
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

        if (typeof nextConfig.webpack === 'function') {
          return nextConfig.webpack(config, options);
        }

        return config;
      },
    }),
  );
};

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
  withImages,
  withSassModules,
  withTypescript,
)(config);
