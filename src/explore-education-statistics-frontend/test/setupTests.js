import '@testing-library/jest-dom';
import 'core-js/features/array/flat-map';
import '@common-test/setupGlobals';
import '@common-test/extend-expect';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
}
