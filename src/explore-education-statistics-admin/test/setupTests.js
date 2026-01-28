import errorOnConsoleError from '@common-test/errorOnConsoleError';
import '@common-test/extend-expect';
import '@common-test/setupGlobals';
import '@testing-library/jest-dom';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
  require('intersection-observer');
}

errorOnConsoleError();
