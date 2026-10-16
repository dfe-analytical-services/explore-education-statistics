import { act } from '@testing-library/react';

/**
 * Creates a mock async handler that stays pending until the test decides to
 * complete it via `resolveHandler`.
 *
 * This allows tests to assert on transient UI state (e.g. loading spinners,
 * disabled buttons) without racing against an arbitrary {@see delay}, which
 * makes them prone to flaking on slower machines.
 */
export default function createDeferredHandler<TArgs extends unknown[] = []>(): {
  handler: jest.Mock<Promise<void>, TArgs>;
  resolveHandler: () => Promise<void>;
} {
  let resolve: () => void = () => {};

  const settled = new Promise<void>(res => {
    resolve = res;
  });

  return {
    handler: jest.fn<Promise<void>, TArgs>(() => settled),
    /**
     * Wrapped in `act` so that any state updates made once the handler's
     * promise settles are flushed before the test asserts on them.
     */
    resolveHandler: async () => {
      await act(async () => {
        resolve();
        await settled;
      });
    },
  };
}
