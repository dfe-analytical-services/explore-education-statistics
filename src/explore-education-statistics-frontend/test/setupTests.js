import 'react-testing-library/cleanup-after-each';
import 'jest-dom/extend-expect';

const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  clear: jest.fn(),
};
global.localStorage = localStorageMock;

Element.prototype.scrollIntoView = jest.fn();

afterEach(() => {
  location.hash = '';
});
