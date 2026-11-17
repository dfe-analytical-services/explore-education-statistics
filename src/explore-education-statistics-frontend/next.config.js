/* eslint-disable no-param-reassign */
const withBundleAnalyzer = require('@next/bundle-analyzer');
const path = require('path');

/**
 * @type {import('next').NextConfig}
 */

const nextConfig = {
  compress: false,
  reactStrictMode: true,
  env: {
    BUILD_NUMBER: process.env.BUILD_BUILDNUMBER ?? '',
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
          issue: {
            // Next generates types into `.next/types` (build) and
            // `.next/dev/types` (dev), and both are in tsconfig's `include`.
            // Leftover generated files from a previous build can reference
            // routes that no longer exist, so don't report issues in them.
            // `pnpm tsc` and `next build` check freshly generated types.
            exclude: [{ file: '**/.next/**' }],
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
            files: 'src/**/*.{css,scss}',
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
  sassOptions: { charset: false }, // prod CSS was breaking due to BOMs being included in the middle of files
  transpilePackages:
    process.env.NEXT_CONFIG_MODE !== 'server'
      ? ['explore-education-statistics-common']
      : [],
};

module.exports = withBundleAnalyzer({
  enabled: process.env.ANALYZE === 'true',
})(nextConfig);
