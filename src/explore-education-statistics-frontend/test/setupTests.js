import errorOnConsoleError from '@common-test/errorOnConsoleError';
import '@common-test/extend-expect';
import '@common-test/setupGlobals';
import { loadEnvConfig } from '@next/env';
import '@testing-library/jest-dom';
import 'urlpattern-polyfill';

loadEnvConfig(process.cwd());

// Configure the API clients as the app does at startup. This must be
// required after `loadEnvConfig` so the env vars are available.
require('@frontend/loadEnv');

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  require('intersection-observer');
}

global.Request = jest.requireActual('node-fetch').Request;
global.Response = jest.requireActual('node-fetch').Response;

errorOnConsoleError();
