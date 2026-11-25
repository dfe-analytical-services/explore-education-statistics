/**
 * @license Copyright (c) 2014-2021, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or https://ckeditor.com/legal/ckeditor-oss-license
 */

const path = require('path');
const webpack = require('webpack');
const { bundler, styles } = require('@ckeditor/ckeditor5-dev-utils');
const TerserWebpackPlugin = require('terser-webpack-plugin');

/**
 * pnpm can install the same version of a CKEditor package more than once
 * (one copy per peer dependency variant, e.g. of `supports-color`), which
 * leads to CKEditor being bundled twice and failing at runtime with
 * `ckeditor-duplicated-modules`. Resolve every `@ckeditor/ckeditor5-*`
 * package from the `ckeditor5` package's own location so that only a single
 * copy of each is bundled.
 */
function getCKEditorAliases() {
  const ckeditor5Dir = path.dirname(require.resolve('ckeditor5/package.json'));
  const { dependencies } = require('ckeditor5/package.json');

  return Object.fromEntries(
    Object.keys(dependencies)
      .filter(name => name.startsWith('@ckeditor/ckeditor5-'))
      .map(name => [
        name,
        path.dirname(
          require.resolve(`${name}/package.json`, { paths: [ckeditor5Dir] }),
        ),
      ]),
  );
}

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

  resolve: {
    alias: getCKEditorAliases(),
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
