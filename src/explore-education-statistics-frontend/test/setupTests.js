import errorOnConsoleError from '@common-test/errorOnConsoleError';
import '@common-test/extend-expect';
import '@common-test/setupGlobals';
import { loadEnvConfig } from '@next/env';
import '@testing-library/jest-dom';
import 'urlpattern-polyfill';

loadEnvConfig(process.cwd());

// Mirror the normalisation that `src/loadEnv.ts` applies at runtime, so tests
// see the same PUBLIC_URL as the running app.
if (process.env.PUBLIC_URL) {
  process.env.PUBLIC_URL = process.env.PUBLIC_URL.replace(/\/+$/, '');
}

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  require('intersection-observer');
}

global.Request = jest.requireActual('node-fetch').Request;
global.Response = jest.requireActual('node-fetch').Response;

errorOnConsoleError();
