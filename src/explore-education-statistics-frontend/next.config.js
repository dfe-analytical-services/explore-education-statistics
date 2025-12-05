/* eslint-disable no-param-reassign */
const withBundleAnalyzer = require('@next/bundle-analyzer');
const path = require('path');

/**
 * @type {import('next').NextConfig}
 */

const assetHeaders = [
  {
    key: 'Cache-Control',
    value: 'public, max-age=31536000, immutable',
  },
];

const metaHeaders = [
  {
    key: 'Cache-Control',
    value: 'public, max-age=3600',
  },
];

const generalHeaders = [
  {
    key: 'X-Content-Type-Options',
    value: 'nosniff',
  },
];

if (process.env.NODE_ENV === 'development') {
  // For local development, so hotreload works correctly
  generalHeaders.push({
    key: 'Cache-Control',
    value: 'no-store, no-cache, must-revalidate, proxy-revalidate, private',
  });
}

const nextConfig = {
  compress: !process.env.WEBSITES_DISABLE_CONTENT_COMPRESSION,
  reactStrictMode: true,
  eslint: {
    ignoreDuringBuilds: true,
  },
  env: {
    BUILD_NUMBER: process.env.BUILD_BUILDNUMBER ?? '',
  },
  publicRuntimeConfig: {
    APP_ENV: process.env.APP_ENV,
    CONTENT_API_BASE_URL: process.env.CONTENT_API_BASE_URL,
    DATA_API_BASE_URL: process.env.DATA_API_BASE_URL,
    PUBLIC_API_BASE_URL: process.env.PUBLIC_API_BASE_URL,
    PUBLIC_API_DOCS_URL: process.env.PUBLIC_API_DOCS_URL,
    NOTIFICATION_API_BASE_URL: process.env.NOTIFICATION_API_BASE_URL,
    APPINSIGHTS_INSTRUMENTATIONKEY: process.env.APPINSIGHTS_INSTRUMENTATIONKEY,
    GA_TRACKING_ID: process.env.GA_TRACKING_ID,
    PUBLIC_URL: process.env.PUBLIC_URL,
  },
  serverRuntimeConfig: {
    AZURE_SEARCH_QUERY_KEY: process.env.AZURE_SEARCH_QUERY_KEY,
    AZURE_SEARCH_ENDPOINT: process.env.AZURE_SEARCH_ENDPOINT,
    AZURE_SEARCH_INDEX: process.env.AZURE_SEARCH_INDEX,
  },
  async headers() {
    return [
      // general case
      {
        source: '/:path*',
        headers: generalHeaders,
      },
      // specific cases (that override the general case headers)
      {
        source: '/:file(favicon.svg|manifest.json)',
        headers: metaHeaders,
      },
      {
        source: '/assets/:path*',
        headers: assetHeaders,
      },
      {
        source: '/_next/static/:path*',
        headers: assetHeaders,
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
  transpilePackages:
    process.env.NEXT_CONFIG_MODE !== 'server'
      ? ['explore-education-statistics-common']
      : [],
};

module.exports = withBundleAnalyzer({
  enabled: process.env.ANALYZE === 'true',
})(nextConfig);
