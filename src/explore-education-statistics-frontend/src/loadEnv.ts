import getConfig from 'next/config';

const { publicRuntimeConfig } = getConfig();

Object.assign(process.env, publicRuntimeConfig, {
  APP_ROOT_ID: '__next',
});

// PUBLIC_URL may be configured with or without a trailing slash, so normalise
// it to never have one, allowing paths to be appended to it safely.
if (process.env.PUBLIC_URL) {
  process.env.PUBLIC_URL = process.env.PUBLIC_URL.replace(/\/+$/, '');
}
