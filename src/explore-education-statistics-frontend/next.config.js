const withCss = require('@zeit/next-css');
const cssLoaderConfig = require('@zeit/next-css/css-loader-config');
const withTypescript = require('@zeit/next-typescript');
const DotEnvPlugin = require('dotenv-webpack');
const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');
const compose = require('lodash/fp/compose');
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

const config = {
  webpack(config, options) {
    const { isServer } = options;

    if (isServer) {
      config.plugins.push(
        new ForkTsCheckerPlugin({
          tsconfig: path.resolve(__dirname, 'tsconfig.json'),
        }),
      );
    }

    config.plugins.push(
      new DotEnvPlugin({
        path: path.resolve(
          __dirname,
          process.env.BUILD_ENV ? `.env.${process.env.BUILD_ENV}` : '.env',
        ),
        safe: true,
      }),
    );

    config.resolve.alias.src = path.resolve(__dirname, 'src');

    return config;
  },
};

module.exports = compose(
  withFonts,
  withImages,
  withCss,
  withSassModules,
  withTypescript,
)(config);
