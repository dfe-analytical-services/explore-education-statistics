import {
  BackoffDelayOptions,
  exponentialBackoffDelay,
  linearBackoffDelay,
} from '@common/utils/math/retryBackoffDelays';
import { IRetryPolicy, RetryContext } from '@microsoft/signalr';
import clamp from 'lodash/clamp';

// Defaults to 10 minutes
const DEFAULT_MAX_RETRY_TIME = 600000;

interface ExponentialBackoffRetryPolicyOptions extends BackoffDelayOptions {
  /**
   * The maximum amount of time to retry for.
   * Defaults to 60 seconds.
   */
  maxRetryTime?: number;
}

export function exponentialBackoffPolicy(
  options: ExponentialBackoffRetryPolicyOptions = {},
): IRetryPolicy {
  const { maxRetryTime = DEFAULT_MAX_RETRY_TIME, ...backoffOptions } = options;

  return {
    nextRetryDelayInMilliseconds({
      elapsedMilliseconds,
      previousRetryCount,
    }: RetryContext): number | null {
      if (elapsedMilliseconds < maxRetryTime) {
        return clamp(
          exponentialBackoffDelay(previousRetryCount + 1, backoffOptions),
          maxRetryTime - elapsedMilliseconds,
        );
      }

      return null;
    },
  };
}

interface LinearBackoffRetryPolicyOptions extends BackoffDelayOptions {
  /**
   * The maximum amount of time to retry for.
   * Defaults to 60 seconds.
   */
  maxRetryTime?: number;
}

export function linearBackoffPolicy(
  options: LinearBackoffRetryPolicyOptions = {},
): IRetryPolicy {
  const { maxRetryTime = DEFAULT_MAX_RETRY_TIME, ...backoffOptions } = options;

  return {
    nextRetryDelayInMilliseconds({
      elapsedMilliseconds,
      previousRetryCount,
    }: RetryContext): number | null {
      if (elapsedMilliseconds < maxRetryTime) {
        return clamp(
          linearBackoffDelay(previousRetryCount + 1, backoffOptions),
          maxRetryTime - elapsedMilliseconds,
        );
      }

      return null;
    },
  };
}
