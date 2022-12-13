import createLocalStorageMock from '@common-test/createLocalStorageMock';
import mockDate from '@common-test/mockDate';

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

  Element.prototype.scrollIntoView = jest.fn();

  window.scroll = jest.fn();
  window.scrollTo = jest.fn();

  window.navigator.clipboard = {
    writeText: jest.fn(),
  };

  URL.createObjectURL = jest.fn();
});

beforeEach(() => {
  // eslint-disable-next-line no-underscore-dangle
  window._localStorage = createLocalStorageMock();

  window.matchMedia = jest.fn(() => {
    return {
      addListener: jest.fn(),
      removeListener: jest.fn(),
      matches: true,
    };
  });

  delete window.ResizeObserver;

  window.ResizeObserver = jest.fn(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn(),
  }));
});

afterEach(() => {
  window.location.hash = '';
  mockDate.reset();
});
