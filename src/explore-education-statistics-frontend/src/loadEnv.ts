import getConfig from 'next/config';

const { publicRuntimeConfig } = getConfig();

Object.assign(process.env, publicRuntimeConfig, {
  APP_ROOT_ID: '__next',
});
