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
        // use next.js defaults for /_next/*
        source: '/((?!_next/).*)',
        headers: [
          ...generalHeaders,
          {
            key: 'Cache-Control',
            value: getGeneralCacheControlHeaderValue(),
          },
        ],
      },
      // specific cases (that override the general case headers)
      {
        source: '/api/:path*',
        headers: [
          {
            key: 'Cache-Control',
            value: 'no-store, no-cache, must-revalidate',
          },
        ],
      },
      {
        source: '/api/assets/:path*',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=86400', // 1 day
          },
        ],
      },
      {
        source: '/:file(favicon.svg|manifest.json)',
        headers: metaHeaders,
      },
      {
        source: '/assets/:path*',
        headers: assetHeaders,
      },
    ];
  },
  async redirects() {
    return [
      {
        source:
          '/find-statistics/:publication/:page(data-guidance|prerelease-access-list)',
        destination: '/find-statistics/:publication',
        permanent: true,
      },
      {
        source: '/find-statistics/:publication/:release/data-guidance',
        destination:
          '/find-statistics/:publication/:release/explore#data-guidance-section',
        permanent: true,
      },
      {
        source: '/find-statistics/:publication/:release/prerelease-access-list',
        destination:
          '/find-statistics/:publication/:release/help#pre-release-access-list-section',
        permanent: true,
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

function getGeneralCacheControlHeaderValue() {
  if (process.env.NODE_ENV !== 'production') {
    return 'no-store, no-cache, must-revalidate';
  }
  const defaultPublicSiteCacheMaxAgeSeconds = Number(
    process.env.DEFAULT_CACHE_MAX_AGE_SECONDS ?? 30,
  );
  return `public, max-age=0, s-maxage=${defaultPublicSiteCacheMaxAgeSeconds}, stale-while-revalidate=30`;
}

module.exports = withBundleAnalyzer({
  enabled: process.env.ANALYZE === 'true',
})(nextConfig);
