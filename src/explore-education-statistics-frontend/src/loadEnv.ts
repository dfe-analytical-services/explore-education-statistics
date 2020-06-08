import getConfig from 'next/config';

export default function loadEnv() {
  const { publicRuntimeConfig } = getConfig();

  Object.assign(process.env, publicRuntimeConfig, {
    APP_ROOT_ID: '__next',
  });
}
