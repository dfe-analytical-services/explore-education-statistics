import clamp from 'lodash/clamp';

export interface BackoffDelayOptions {
  /**
   * The maximum random delay proportion that can be added on top
   * of the calculated exponential backoff. This avoids synchronized
   * retries across many clients in the event of a mass failure.
   *
   * For example, the default of 0.2 means that up to 20% of
   * the calculated exponential backoff can be added.
   */
  maxRandomDelayFraction?: number;
  /**
   * The maximum delay that can be calculated (in milliseconds).
   */
  maxDelay?: number;
}

/**
 * Calculate a retry delay (in milliseconds) using exponential
 * backoff given the current {@param retryNumber}.
 *
 * Optional {@param options} can be used to configure the behaviour.
 *
 * Exponential backoff is calculated by using the retry
 * attempt as an exponent (i.e. 2^n-1). This delay increases
 * until a maximum amount is reached (defaults to 32 seconds).
 */
export function exponentialBackoffDelay(
  retryNumber: number,
  options: BackoffDelayOptions = {},
): number {
  if (retryNumber <= 0) {
    throw new Error('Retry number must be greater than 0');
  }

  const { maxDelay = 32000, maxRandomDelayFraction } = options;

  const backoffDelay = clamp(2 ** (retryNumber - 1) * 1000, 0, maxDelay);
  return addRandomDelay(backoffDelay, maxRandomDelayFraction);
}

export interface LinearBackoffDelayOptions extends BackoffDelayOptions {
  /**
   * The interval (in milliseconds) to increase the
   * delay by on each retry attempt.
   */
  interval?: number;
}

/**
 * Calculate a retry delay (in milliseconds) using linear backoff
 * given the current {@param retryNumber}.
 *
 * Optional {@param options} can be used to configure the behaviour.
 *
 * Linear backoff is calculated by increasing the delay
 * by a specific interval for each retry attempt. This delay increases
 * until a maximum amount is reached (defaults to 30 seconds).
 */
export function linearBackoffDelay(
  retryNumber: number,
  options: LinearBackoffDelayOptions = {},
): number {
  if (retryNumber <= 0) {
    throw new Error('Retry number must be greater than 0');
  }

  const { maxDelay = 30000, maxRandomDelayFraction, interval = 1000 } = options;

  const delay = clamp(retryNumber * interval, 0, maxDelay);
  return addRandomDelay(delay, maxRandomDelayFraction);
}

/**
 * Add a random delay to avoid many clients retrying
 * simultaneously in the event of a mass failure.
 */
function addRandomDelay(delay: number, maxRandomDelayFraction = 0.2): number {
  if (maxRandomDelayFraction < 0) {
    throw new Error(
      'Maximum random delay fraction must be greater than or equal to 0',
    );
  }

  const randomDelay = delay * maxRandomDelayFraction * Math.random();
  return delay + randomDelay;
}
