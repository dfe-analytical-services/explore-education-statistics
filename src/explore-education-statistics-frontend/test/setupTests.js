import 'react-testing-library/cleanup-after-each';
import 'jest-dom/extend-expect';

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
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      matches: true,
    };
  });
});

afterEach(() => {
  window.location.hash = '';
});
