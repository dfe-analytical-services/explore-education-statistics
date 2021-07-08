import { createMemoryHistory } from 'history';

export default function createMemoryHistoryWithMockedPush() {
  const history = createMemoryHistory();
  return {
    ...history,
    push: jest.fn(),
  };
}
