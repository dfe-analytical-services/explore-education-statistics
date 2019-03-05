const withTypescript = require('@zeit/next-typescript');
const withSass = require('@zeit/next-sass');
const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');
const path = require('path');

const config = {
  cssModules: true,
  webpack(config, options) {
    if (options.isServer) {
      config.plugins.push(
        new ForkTsCheckerPlugin({
          tsconfig: path.resolve(__dirname, '../tsconfig.json'),
        }),
      );
    }

    return config;
  },
};

module.exports = withSass(withTypescript(config));
