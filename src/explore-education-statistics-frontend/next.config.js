/* eslint-disable no-param-reassign */

const withCss = require('@zeit/next-css');
const cssLoaderConfig = require('@zeit/next-css/css-loader-config');
const withTypescript = require('@zeit/next-typescript');
const DotEnv = require('dotenv');
const DotEnvPlugin = require('dotenv-webpack');
const fs = require('fs');
const compose = require('lodash/fp/compose');
const withImages = require('next-images');
const withTranspileModules = require('next-transpile-modules');
const path = require('path');

const { BUILD_ENV } = process.env;

if (['local', 'example'].includes(BUILD_ENV)) {
  throw new Error('Invalid BUILD_ENV provided');
}

const localEnvFile = fs.existsSync('.env.local') ? '.env.local' : '.env';

const envFilePath = BUILD_ENV ? `.env.${BUILD_ENV}` : localEnvFile;
const envConfig = DotEnv.parse(fs.readFileSync(envFilePath));

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
        importLoaders: 1,
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
  transpileModules: ['explore-education-statistics-common', '@common'],
  webpack(config, options) {
    const { dev, isServer } = options;

    if (isServer) {
      const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');

      config.plugins.push(
        new ForkTsCheckerPlugin({
          tsconfig: path.resolve(__dirname, 'tsconfig.json'),
        }),
      );
    }

    if (dev) {
      const CaseSensitivePathsPlugin = require('case-sensitive-paths-webpack-plugin');
      const StylelintPlugin = require('stylelint-webpack-plugin');

      config.plugins.push(
        new CaseSensitivePathsPlugin(),
        new StylelintPlugin({
          // Next doesn't play nicely with emitted errors
          // so we'll just display warnings instead
          emitErrors: false,
        }),
      );
    }

    const originalEntry = config.entry;

    config.entry = async () => {
      const entries = await originalEntry();

      if (entries['main.js'] && !entries['main.js'].includes('./polyfill.js')) {
        entries['main.js'].unshift('./polyfill.js');
      }

      return entries;
    };

    config.plugins.push(
      new DotEnvPlugin({
        path: envFilePath,
        safe: true,
      }),
    );

    config.resolve.alias = {
      ...config.resolve.alias,
      '@frontend': path.resolve(__dirname, 'src'),
      '@common': path.resolve(
        __dirname,
        '../explore-education-statistics-common/src',
      ),
      react: path.resolve(__dirname, 'node_modules/react'),
      formik: path.resolve(__dirname, 'node_modules/formik'),
    };

    options.defaultLoaders.babel.options.configFile = path.resolve(
      __dirname,
      'babel.config.js',
    );

    return config;
  },
};

module.exports = compose(
  withTranspileModules,
  withFonts,
  withImages,
  withCss,
  withSassModules,
  withTypescript,
  withESLint,
)(nextConfig);
