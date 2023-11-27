/**
 * @license Copyright (c) 2014-2021, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or https://ckeditor.com/legal/ckeditor-oss-license
 */

/* eslint-env node */

const path = require('path');
const webpack = require('webpack');
const { bundler, styles } = require('@ckeditor/ckeditor5-dev-utils');
const TerserWebpackPlugin = require('terser-webpack-plugin');

module.exports = {
  devtool: 'source-map',
  performance: { hints: false },

  entry: path.resolve(__dirname, 'src', 'ckeditor.js'),

  output: {
    // The name under which the editor will be exported.
    library: 'ClassicEditor',

    path: path.resolve(__dirname, 'build'),
    filename: 'ckeditor.js',
    libraryTarget: 'umd',
    libraryExport: 'default',
  },

  optimization: {
    minimizer: [
      new TerserWebpackPlugin({
        terserOptions: {
          output: {
            // Preserve CKEditor 5 license comments.
            comments: /^!/,
          },
          sourceMap: true,
        },
        extractComments: false,
      }),
    ],
  },

  plugins: [
    new webpack.BannerPlugin({
      banner: bundler.getLicenseBanner(),
      raw: true,
    }),
  ],

  module: {
    rules: [
      {
        test: /\.svg$/,
        use: ['raw-loader'],
      },
      {
        test: /\.css$/,
        use: [
          {
            loader: 'style-loader',
            options: {
              injectType: 'singletonStyleTag',
              attributes: {
                'data-cke': true,
              },
            },
          },
          {
            loader: 'css-loader',
          },
          {
            loader: 'postcss-loader',
            options: {
              postcssOptions: styles.getPostCssConfig({
                themeImporter: {
                  themePath: require.resolve('@ckeditor/ckeditor5-theme-lark'),
                },
                minify: true,
              }),
            },
          },
        ],
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env'],
          },
        },
      },
    ],
  },
};
