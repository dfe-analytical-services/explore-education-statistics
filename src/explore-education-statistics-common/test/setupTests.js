import 'react-testing-library/cleanup-after-each';
import 'jest-dom/extend-expect';
import 'core-js/fn/array/virtual/flat-map';

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
}

const localStorageMock = {
  clear: jest.fn(),
  getItem: jest.fn(),
  setItem: jest.fn(),
};
global.localStorage = localStorageMock;

Element.prototype.scrollIntoView = jest.fn();

// This is just a little hack to silence a warning that we'll get until React
// fixes this: https://github.com/facebook/react/pull/14853
const originalError = console.error;
beforeAll(() => {
  console.error = (...args) => {
    if (/Warning.*not wrapped in act/.test(args[0])) {
      return;
    }
    originalError.call(console, ...args);
  };
});

beforeEach(() => {
  window.matchMedia = jest.fn(() => {
    return {
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      matches: true,
    };
  });
});

afterEach(() => {
  window.location.hash = '';
});
