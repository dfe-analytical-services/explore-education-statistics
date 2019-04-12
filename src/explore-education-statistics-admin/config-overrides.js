const DotEnvPlugin = require('dotenv-webpack');
const StylelintPlugin = require('stylelint-webpack-plugin');
const path = require('path');

const DEPLOY_ENV = process.env.DEPLOY_ENV;

if (DEPLOY_ENV === 'example') {
  throw new Error('DEPLOY_ENV cannot be `example`');
}

module.exports = {
  webpack: config => {
    config.plugins = [
      ...config.plugins,

      new DotEnvPlugin({
        path: DEPLOY_ENV ? `./.env.${DEPLOY_ENV}` : './.env',
        safe: true,
      }),
    ];

    config.module.rules.push({
      test: /\.(woff|woff2|eot|ttf|otf)$/,
      use: [
        {
          loader: 'url-loader',
          options: {
            limit: 8192,
            fallback: 'file-loader',
            publicPath: '/static/fonts/',
            outputPath: `static/fonts`,
            name: '[name]-[hash].[ext]',
          },
        },
      ],
    });

    return config;
  },
};
