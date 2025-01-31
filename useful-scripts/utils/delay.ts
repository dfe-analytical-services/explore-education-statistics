/**
 * Returns a promise that will not complete until a timeout has elapsed.
 *
 * This is basically just a promisified version of {@link setTimeout}.
 */
export default async function delay(timeout?: number): Promise<void> {
  await new Promise<void>(resolve => {
    setTimeout(async () => {
      resolve();
    }, timeout);
  });
}
