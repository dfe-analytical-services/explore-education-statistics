/* eslint-disable no-param-reassign */
const flowRight = require('lodash/fp/flowRight');
const withTranspileModules = require('next-transpile-modules');
const path = require('path');

/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: false,
  eslint: {
    ignoreDuringBuilds: true,
  },
  images: {
    remotePatterns: [
      {
        hostname: process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL.replace(
          '/api',
          '',
        ),
      },
    ],
    formats: ['image/avif', 'image/webp'],
  },
  env: {
    BUILD_NUMBER: process.env.BUILD_BUILDNUMBER,
  },
  async redirects() {
    return [
      {
        source: '/download-latest-data',
        destination: '/data-catalogue',
        permanent: true,
      },
      {
        source: '/find-statistics/:publication/meta-guidance',
        destination: '/find-statistics/:publication/data-guidance',
        permanent: true,
      },
      {
        source: '/find-statistics/:publication/:release/meta-guidance',
        destination: '/find-statistics/:publication/:release/data-guidance',
        permanent: true,
      },
    ];
  },
  async headers() {
    return [
      {
        source: '/fonts/(.*)',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable',
          },
        ],
      },
    ];
  },
  webpack(config, options) {
    const { dev, isServer } = options;

    if (isServer) {
      const ForkTsCheckerPlugin = require('fork-ts-checker-webpack-plugin');

      config.plugins.push(
        new ForkTsCheckerPlugin({
          typescript: {
            configFile: path.resolve(__dirname, 'tsconfig.json'),
          },
        }),
      );

      config.externals = [...config.externals, 'react', 'react-dom'];
    }

    if (dev) {
      const CaseSensitivePathsPlugin = require('case-sensitive-paths-webpack-plugin');

      config.plugins.push(new CaseSensitivePathsPlugin());

      if (process.env.STYLELINT_DISABLE !== 'true') {
        const StylelintPlugin = require('stylelint-webpack-plugin');

        config.plugins.push(
          new StylelintPlugin({
            // Next doesn't play nicely with emitted errors
            // so we'll just display warnings instead
            emitErrors: false,
          }),
        );
      }

      if (dev && process.env.ESLINT_DISABLE !== 'true') {
        const ESLintPlugin = require('eslint-webpack-plugin');

        config.plugins.push(
          new ESLintPlugin({
            extensions: ['js', 'jsx', 'ts', 'tsx'],
          }),
        );
      }
    }

    config.resolve.alias = {
      ...config.resolve.alias,
      './dist/cpexcel.js': false,
      react: path.resolve(__dirname, 'node_modules/react'),
      formik: path.resolve(__dirname, 'node_modules/formik'),
    };

    return config;
  },
};

// Plugins are applied to the
// Next config from left to right
module.exports = flowRight(
  withTranspileModules(
    // Need to modify the following as next-transpile-modules
    // throws when running server in a production environment
    // because we remove the target modules as part
    // of the build to reduce total artifact size.
    process.env.NEXT_CONFIG_MODE !== 'server'
      ? ['explore-education-statistics-common']
      : [],
  ),
)(nextConfig);
