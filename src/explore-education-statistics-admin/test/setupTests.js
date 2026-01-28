import '@testing-library/jest-dom';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';
import '@common-test/setupGlobals';
import '@common-test/extend-expect';
import failOnConsole from 'jest-fail-on-console';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
  require('intersection-observer');
}

failOnConsole({
  skipTest: ({ testName }) => testName.includes('skip-console-errors'),
  allowMessage: errorMessage =>
    errorMessage.includes('`DialogContent` requires a `DialogTitle`'),
  shouldFailOnWarn: false,
});
