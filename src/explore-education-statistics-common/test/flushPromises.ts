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
export default function flushPromises() {
  return new Promise(resolve => setImmediate(resolve));
}
