import delay from './delay';

/**
 * Delay the completion of a timeout until a
 * minimum amount of time has passed.
 */
export default async function minDelay<T>(
  task: (() => T | Promise<T>) | Promise<T>,
  minTimeout: number,
): Promise<T> {
  const start = Date.now();
  const result = typeof task === 'function' ? await task() : await task;
  const end = Date.now();

  const timeTaken = end - start;

  if (timeTaken < minTimeout) {
    await delay(minTimeout - timeTaken);
  }

  return result;
}
