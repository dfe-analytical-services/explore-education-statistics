/* eslint-disable no-param-reassign */
const flowRight = require('lodash/fp/flowRight');
const withTranspileModules = require('next-transpile-modules');
const path = require('path');

const cspConnectSrc = [
  "'self'",
  process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL.replace('/api', ''),
  process.env.NEXT_PUBLIC_DATA_API_BASE_URL.replace('/api', ''),
  process.env.NEXT_PUBLIC_NOTIFICATION_API_BASE_URL.replace('/api', ''),
  'https://www.google-analytics.com',
  'https://dc.services.visualstudio.com/v2/track',
];

const cspScriptSrc = [
  "'self'",
  'https://www.google-analytics.com/',
  "'unsafe-inline'",
  "'unsafe-eval'",
];

const frameScriptSrc = [
  "'self'",
  'https://department-for-education.shinyapps.io/',
  'https://dfe-analytical-services.github.io/',
];

const contentSecurityPolicy = `
  default-src 'self';
  script-src ${cspScriptSrc.join(' ')};
  style-src 'self' 'unsafe-inline';
  img-src * blob: data: https://www.google-analytics.com/;
  media-src 'self' blob: data:;
  font-src 'self';
  connect-src ${
    process.env.NODE_ENV !== 'production' ? '*' : cspConnectSrc.join(' ')
  };
  frame-src ${frameScriptSrc.join(' ')};
  frame-ancestors 'self';
  child-src 'self';
`;

const securityHeaders = [
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP
  {
    key: 'Content-Security-Policy',
    value: contentSecurityPolicy.replace(/\n/g, ''),
  },
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
  {
    key: 'Referrer-Policy',
    value: 'origin-when-cross-origin',
  },
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
  {
    key: 'X-Frame-Options',
    value: 'DENY',
  },
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
  {
    key: 'X-Content-Type-Options',
    value: 'nosniff',
  },
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-DNS-Prefetch-Control
  {
    key: 'X-DNS-Prefetch-Control',
    value: 'on',
  },
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security
  {
    key: 'Strict-Transport-Security',
    value: 'max-age=31536000; includeSubDomains; preload',
  },
  // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Feature-Policy
  {
    key: 'Permissions-Policy',
    value: 'camera=(), microphone=(), geolocation=()',
  },
];

const nextConfig = {
  reactStrictMode: true,
  poweredByHeader: false,
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
        source: '/(.*)',
        headers: securityHeaders,
      },
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
