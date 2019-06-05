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

beforeEach(() => {
  window.matchMedia = jest.fn(() => {
    return {
      addListener: jest.fn(),
      removeListener: jest.fn(),
      matches: true,
    };
  });
});

afterEach(() => {
  window.location.hash = '';
});
