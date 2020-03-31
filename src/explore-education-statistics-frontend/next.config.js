/* eslint-disable no-param-reassign */

const withCss = require('@zeit/next-css');
const cssLoaderConfig = require('@zeit/next-css/css-loader-config');
const DotEnv = require('dotenv');
const fs = require('fs');
const compose = require('lodash/fp/compose');
const withImages = require('next-images');
const withTranspileModules = require('next-transpile-modules');
const path = require('path');

const envFilePath = fs.existsSync('.env.local') ? '.env.local' : '.env';
const envConfig = DotEnv.config(fs.readFileSync(envFilePath));

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

  const sassLoader = useModules =>
    cssLoaderConfig(config, {
      extensions: ['scss', 'sass'],
      cssModules: useModules,
      dev,
      isServer,
      loaders: [
        {
          loader: 'sass-loader',
          options: {},
        },
      ],
      cssLoaderOptions: {
        localIdentName: 'dfe-[name]__[local]--[hash:base64:5]',
      },
    });

  config.module.rules.push(
    {
      test: /\.module.scss$/,
      use: sassLoader(true),
    },
    {
      test: /\.module.sass$/,
      use: sassLoader(true),
    },
    {
      test: /\.scss$/,
      exclude: /\.module.scss$/,
      use: sassLoader(false),
    },
    {
      test: /\.sass$/,
      exclude: /\.module.scss$/,
      use: sassLoader(false),
    },
  );

  return config;
});

const withESLint = createPlugin((config, options) => {
  const { dev } = options;

  if (dev && envConfig.ESLINT_DISABLE !== 'true') {
    config.module.rules.push({
      enforce: 'pre',
      test: /\.(ts|tsx|js|jsx)$/,
      exclude: /node_modules/,
      use: {
        loader: 'eslint-loader',
        options: {
          failOnError: true,
        },
      },
    });
  }

  return config;
});

const nextConfig = {
  publicRuntimeConfig: {
    CONTENT_API_BASE_URL: process.env.CONTENT_API_BASE_URL,
    DATA_API_BASE_URL: process.env.DATA_API_BASE_URL,
    FUNCTION_API_BASE_URL: process.env.FUNCTION_API_BASE_URL,
    APPINSIGHTS_INSTRUMENTATIONKEY: process.env.APPINSIGHTS_INSTRUMENTATIONKEY,
    GA_TRACKING_ID: process.env.GA_TRACKING_ID,
    HOTJAR_ID: process.env.HOTJAR_ID,
  },
  webpack(config, options) {
    const { dev, isServer } = options;

    if (isServer) {
      const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');

      config.plugins.push(
        new ForkTsCheckerPlugin({
          tsconfig: path.resolve(__dirname, 'src/tsconfig.json'),
        }),
      );

      config.externals = [...config.externals, 'react', 'react-dom'];
    }

    if (dev) {
      const CaseSensitivePathsPlugin = require('case-sensitive-paths-webpack-plugin');
      const StylelintPlugin = require('stylelint-webpack-plugin');

      config.plugins.push(new CaseSensitivePathsPlugin());

      if (envConfig.ESLINT_DISABLE !== 'true') {
        config.plugins.push(
          new StylelintPlugin({
            // Next doesn't play nicely with emitted errors
            // so we'll just display warnings instead
            emitErrors: false,
          }),
        );
      }
    }

    config.resolve.alias = {
      ...config.resolve.alias,
      './dist/cpexcel.js': '',
      '@frontend': path.resolve(__dirname, 'src'),
      '@common': path.resolve(
        __dirname,
        '../explore-education-statistics-common/src',
      ),
      react: path.resolve(__dirname, 'node_modules/react'),
      formik: path.resolve(__dirname, 'node_modules/formik'),
    };

    config.node = {
      ...config.node,
      Buffer: false,
    };

    return config;
  },
};

module.exports = compose(
  withTranspileModules(['explore-education-statistics-common', '@common']),
  withFonts,
  withImages,
  withCss,
  withSassModules,
  withESLint,
)(nextConfig);
