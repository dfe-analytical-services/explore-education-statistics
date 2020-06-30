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

  URL.createObjectURL = jest.fn();
});

beforeEach(() => {
  // eslint-disable-next-line no-underscore-dangle
  window._localStorage = {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
  };

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
