import 'react-testing-library/cleanup-after-each';
import 'jest-dom/extend-expect';
import 'core-js/fn/array/virtual/flat-map';
import './setupGlobals';

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
}
