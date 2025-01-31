/**
 * Returns a promise that will not complete
 * until a specific {@param timeout} has elapsed.
 *
 * This is basically just a promisified
 * version of {@see setTimeout}.
 */
export default async function delay(timeout?: number): Promise<void> {
  await new Promise<void>(resolve => {
    setTimeout(async () => {
      resolve();
    }, timeout);
  });
}
