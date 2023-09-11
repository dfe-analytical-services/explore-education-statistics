import react from '@vitejs/plugin-react';
import path from 'node:path';
import { defineConfig, loadEnv } from 'vite';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd());

  const aliases = {
    '@admin': path.resolve(__dirname, 'src'),
    '@common': path.resolve('../explore-education-statistics-common/src'),
    formik: require.resolve('formik'),
    react: require.resolve('react'),
  };

  return {
    plugins: [
      react({
        jsxRuntime: 'classic',
      }),
    ],
    // build: {
    //   cssMinify: 'lightningcss',
    // },
    // css: {
    //   transformer: 'lightningcss',
    // },
    define: {
      'process.env': {
        ...env,
        APP_ROOT_ID: 'root',
        NODE_ENV: mode,
      },
    },
    resolve: {
      alias: [
        ...Object.entries(aliases).map(([find, replacement]) => {
          return { find, replacement };
        }),
        // Fix for importing Sass from node_modules.
        // See: https://github.com/vitejs/vite/issues/5764
        {
          find: /^~(.*)$/,
          replacement: '$1',
        },
      ],
    },
  };
});
