import {
  exponentialBackoffPolicy,
  linearBackoffPolicy,
} from '@admin/services/hubs/utils/retryPolicies';

describe('exponentialBackoffPolicy', () => {
  test('returns correct retry delay on retry 1 with defaults', () => {
    const policy = exponentialBackoffPolicy();

    const nextDelay = policy.nextRetryDelayInMilliseconds({
      elapsedMilliseconds: 0,
      previousRetryCount: 0,
      retryReason: new Error('Network error'),
    });

    expect(nextDelay).toBeGreaterThanOrEqual(1000);
    expect(nextDelay).toBeLessThanOrEqual(1200);
  });

  test('returns correct retry delay on retry 2 with defaults', () => {
    const policy = exponentialBackoffPolicy();

    const nextDelay = policy.nextRetryDelayInMilliseconds({
      elapsedMilliseconds: 0,
      previousRetryCount: 1,
      retryReason: new Error('Network error'),
    });

    expect(nextDelay).toBeGreaterThanOrEqual(2000);
    expect(nextDelay).toBeLessThanOrEqual(2400);
  });

  test('returns correct retry delay on retry 3 with defaults', () => {
    const policy = exponentialBackoffPolicy();

    const nextDelay = policy.nextRetryDelayInMilliseconds({
      elapsedMilliseconds: 0,
      previousRetryCount: 2,
      retryReason: new Error('Network error'),
    });

    expect(nextDelay).toBeGreaterThanOrEqual(4000);
    expect(nextDelay).toBeLessThanOrEqual(4800);
  });

  test('returns null retry delay if elapsed time exceeds default `maxRetryTime`', () => {
    const policy = exponentialBackoffPolicy();

    expect(
      policy.nextRetryDelayInMilliseconds({
        elapsedMilliseconds: 99999999,
        previousRetryCount: 1,
        retryReason: new Error('Network error'),
      }),
    ).toBeNull();
  });

  test('returns null retry delay if elapsed time exceeds `maxRetryTime`', () => {
    const policy = exponentialBackoffPolicy({
      maxRetryTime: 1000,
    });

    expect(
      policy.nextRetryDelayInMilliseconds({
        elapsedMilliseconds: 2000,
        previousRetryCount: 2,
        retryReason: new Error('Network error'),
      }),
    ).toBeNull();
  });

  test('returns clamped retry delay if normal delay exceeds `maxRetryTime`', () => {
    const policy = exponentialBackoffPolicy({
      maxRetryTime: 4000,
    });

    expect(
      policy.nextRetryDelayInMilliseconds({
        elapsedMilliseconds: 3000,
        previousRetryCount: 3,
        retryReason: new Error('Network error'),
      }),
    ).toBe(1000);
  });
});

describe('linearBackoffPolicy', () => {
  test('returns correct retry delay on retry 1 with defaults', () => {
    const policy = linearBackoffPolicy();

    const nextDelay = policy.nextRetryDelayInMilliseconds({
      elapsedMilliseconds: 0,
      previousRetryCount: 0,
      retryReason: new Error('Network error'),
    });

    expect(nextDelay).toBeGreaterThanOrEqual(1000);
    expect(nextDelay).toBeLessThanOrEqual(1200);
  });

  test('returns correct retry delay on retry 2 with defaults', () => {
    const policy = linearBackoffPolicy();

    const nextDelay = policy.nextRetryDelayInMilliseconds({
      elapsedMilliseconds: 0,
      previousRetryCount: 1,
      retryReason: new Error('Network error'),
    });

    expect(nextDelay).toBeGreaterThanOrEqual(2000);
    expect(nextDelay).toBeLessThanOrEqual(2400);
  });

  test('returns correct retry delay on retry 3 with defaults', () => {
    const policy = linearBackoffPolicy();

    const nextDelay = policy.nextRetryDelayInMilliseconds({
      elapsedMilliseconds: 0,
      previousRetryCount: 2,
      retryReason: new Error('Network error'),
    });

    expect(nextDelay).toBeGreaterThanOrEqual(3000);
    expect(nextDelay).toBeLessThanOrEqual(3600);
  });

  test('returns null retry delay if elapsed time exceeds `maxRetryTime`', () => {
    const policy = linearBackoffPolicy({
      maxRetryTime: 1000,
    });

    expect(
      policy.nextRetryDelayInMilliseconds({
        elapsedMilliseconds: 2000,
        previousRetryCount: 2,
        retryReason: new Error('Network error'),
      }),
    ).toBeNull();
  });

  test('returns clamped retry delay if normal delay exceeds`maxRetryTime`', () => {
    const policy = linearBackoffPolicy({
      maxRetryTime: 4000,
    });

    expect(
      policy.nextRetryDelayInMilliseconds({
        elapsedMilliseconds: 3000,
        previousRetryCount: 3,
        retryReason: new Error('Network error'),
      }),
    ).toBe(1000);
  });
});
