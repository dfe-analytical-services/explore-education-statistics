/* eslint-disable no-param-reassign */
import { Dictionary } from '@common/types';

export default function createLocalStorageMock(
  context: Dictionary<string> = {},
): jest.Mocked<Storage> {
  return {
    length: Object.keys(context).length,
    key: jest.fn(),
    getItem: jest.fn(key => context[key]),
    setItem: jest.fn((key, value) => {
      context[key] = value;
    }),
    removeItem: jest.fn(key => {
      delete context[key];
    }),
    clear: jest.fn(() => {
      Object.keys(context).forEach(key => {
        delete context[key];
      });
    }),
  };
}
