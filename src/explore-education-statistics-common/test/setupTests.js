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

const createElementNSOrig = global.document.createElementNS;
// eslint-disable-next-line
global.document.createElementNS = function(namespaceURI, qualifiedName) {
  if (
    namespaceURI === 'http://www.w3.org/2000/svg' &&
    qualifiedName === 'svg'
  ) {
    // eslint-disable-next-line prefer-rest-params
    const element = createElementNSOrig.apply(this, arguments);
    element.createSVGRect = () => {};
    return element;
  }
  // eslint-disable-next-line prefer-rest-params
  return createElementNSOrig.apply(this, arguments);
};
