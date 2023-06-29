import flushPromises from '@common-test/flushPromises';

/**
 * Flushes all macro and micro task queues (including promises).
 *
 * Especially useful in cases where a promise wrapping a {@see setTimeout}
 * is used and would normally cause Jest to timeout.
 */
export default async function flushTasks(): Promise<void> {
  do {
    jest.runAllTimers();

    // eslint-disable-next-line no-await-in-loop
    await flushPromises();
  } while (jest.getTimerCount() > 0);
}
