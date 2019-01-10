const DotEnvPlugin = require('dotenv-webpack');
const StylelintPlugin = require('stylelint-webpack-plugin');

const DEPLOY_ENV = process.env.DEPLOY_ENV;

if (DEPLOY_ENV === 'example') {
  throw new Error('DEPLOY_ENV cannot be `example`');
}

module.exports = {
  webpack: (config) => {
    config.plugins = [
      ...config.plugins,
      new StylelintPlugin(),
      new DotEnvPlugin({
        path: DEPLOY_ENV ? `./.env.${DEPLOY_ENV}` : './.env',
        safe: true,
      }),
    ];

    return config;
  },
};
