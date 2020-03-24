import getConfig from 'next/config';

const { publicRuntimeConfig } = getConfig();

process.env = {
  ...process.env,
  ...publicRuntimeConfig,
  APP_ROOT_ID: '__next',
};
