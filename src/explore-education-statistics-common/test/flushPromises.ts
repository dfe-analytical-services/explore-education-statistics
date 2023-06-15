/**
 * Flush all pending promises.
 *
 * This is mainly intended for situations where we use promises
 * and fake timers in test code.
 *
 * Due to how Jest and JavaScript works, promises may get stuck in
 * the underlying promise queue (aka microtask queue), and need to
 * be flushed before we can observe them being resolved correctly.
 * @see https://stackoverflow.com/questions/52177631/jest-timer-and-promise-dont-work-well-settimeout-and-async-function
 */
export default async function flushPromises() {
  do {
    jest.runAllTimers();
    // eslint-disable-next-line no-await-in-loop
    await new Promise(jest.requireActual('timers').setImmediate);
  } while (jest.getTimerCount() > 0);
}
// export default async function flushPromises() {
//   return new Promise(jest.requireActual('timers').setImmediate);
// }
