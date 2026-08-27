import getConfig from 'next/config';

const { publicRuntimeConfig } = getConfig();

Object.assign(process.env, publicRuntimeConfig, {
  APP_ROOT_ID: '__next',
});

if (process.env.PUBLIC_URL) {
  // Remove trailing slashes
  process.env.PUBLIC_URL = process.env.PUBLIC_URL.replace(/\/+$/, '');
}
