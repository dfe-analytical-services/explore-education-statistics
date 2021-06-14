import createMemoryHistory from 'history/createMemoryHistory';

export default function createMemoryHistoryWithMockedPush() {
  const history = createMemoryHistory();
  return {
    ...history,
    push: jest.fn(),
  };
}
