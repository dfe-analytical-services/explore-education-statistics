import '@common-test/extend-expect';
import '@common-test/setupGlobals';
import { loadEnvConfig } from '@next/env';
import '@testing-library/jest-dom';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';
import failOnConsole from 'jest-fail-on-console';
import 'urlpattern-polyfill';

loadEnvConfig(process.cwd());

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
  require('intersection-observer');
}

global.Request = jest.requireActual('node-fetch').Request;
global.Response = jest.requireActual('node-fetch').Response;

failOnConsole({
  allowMessage: errorMessage =>
    errorMessage.includes('`DialogContent` requires a `DialogTitle`'),
  shouldFailOnWarn: false,
});
