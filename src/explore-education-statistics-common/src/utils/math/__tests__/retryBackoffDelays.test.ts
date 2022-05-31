import {
  BackoffDelayOptions,
  exponentialBackoffDelay,
  linearBackoffDelay,
  LinearBackoffDelayOptions,
} from '@common/utils/math/retryBackoffDelays';

describe('exponentialBackoffDelay', () => {
  test('returns correct default delay for retry number 1', () => {
    const delay = exponentialBackoffDelay(1);
    expect(delay).toBeGreaterThanOrEqual(1000);
    expect(delay).toBeLessThanOrEqual(1200);
  });

  test('returns correct default delay for retry number 2', () => {
    const delay = exponentialBackoffDelay(2);
    expect(delay).toBeGreaterThanOrEqual(2000);
    expect(delay).toBeLessThanOrEqual(2400);
  });

  test('returns correct default delay for retry number 3', () => {
    const delay = exponentialBackoffDelay(3);
    expect(delay).toBeGreaterThanOrEqual(4000);
    expect(delay).toBeLessThanOrEqual(4800);
  });

  test('returns correct default delay for retry number 4', () => {
    const delay = exponentialBackoffDelay(4);
    expect(delay).toBeGreaterThanOrEqual(8000);
    expect(delay).toBeLessThanOrEqual(9600);
  });

  test('throws when retry number is invalid', () => {
    const error = 'Retry number must be greater than 0';

    expect(() => exponentialBackoffDelay(0)).toThrow(error);
    expect(() => exponentialBackoffDelay(-1)).toThrow(error);
    expect(() => exponentialBackoffDelay(Number.NEGATIVE_INFINITY)).toThrow(
      error,
    );
  });

  test('returns correct delay when `maxRandomDelayFraction` is 0', () => {
    const options: BackoffDelayOptions = { maxRandomDelayFraction: 0 };

    expect(exponentialBackoffDelay(1, options)).toBe(1000);
    expect(exponentialBackoffDelay(2, options)).toBe(2000);
    expect(exponentialBackoffDelay(3, options)).toBe(4000);
    expect(exponentialBackoffDelay(4, options)).toBe(8000);
  });

  test('returns correct delay when custom `maxRandomDelayFraction` is set for retry number 1', () => {
    const delay = exponentialBackoffDelay(1, {
      maxRandomDelayFraction: 0.5,
    });

    expect(delay).toBeGreaterThanOrEqual(1000);
    expect(delay).toBeLessThanOrEqual(1500);
  });

  test('returns correct delay when custom `maxRandomDelayFraction` is set for retry number 2', () => {
    const delay = exponentialBackoffDelay(2, {
      maxRandomDelayFraction: 0.5,
    });

    expect(delay).toBeGreaterThanOrEqual(2000);
    expect(delay).toBeLessThanOrEqual(3000);
  });

  test('throws when `maxRandomDelayFraction` is invalid', () => {
    const error =
      'Maximum random delay fraction must be greater than or equal to 0';

    expect(() =>
      exponentialBackoffDelay(1, {
        maxRandomDelayFraction: -1,
      }),
    ).toThrow(error);

    expect(() =>
      exponentialBackoffDelay(1, {
        maxRandomDelayFraction: Number.NEGATIVE_INFINITY,
      }),
    ).toThrow(error);
  });

  test('returns delay that cannot exceed `maxDelay`', () => {
    expect(
      exponentialBackoffDelay(1, { maxDelay: 100, maxRandomDelayFraction: 0 }),
    ).toBe(100);
    expect(
      exponentialBackoffDelay(2, { maxDelay: 1999, maxRandomDelayFraction: 0 }),
    ).toBe(1999);
    expect(
      exponentialBackoffDelay(4, { maxDelay: 5000, maxRandomDelayFraction: 0 }),
    ).toBe(5000);
  });

  test('returns normal delay when `maxDelay` is set to positive infinity', () => {
    const delay = exponentialBackoffDelay(4, {
      maxDelay: Number.POSITIVE_INFINITY,
    });
    expect(delay).toBeGreaterThanOrEqual(8000);
    expect(delay).toBeLessThanOrEqual(9600);
  });

  test('returns delay of 0 when `maxDelay` is set to negative infinity', () => {
    expect(
      exponentialBackoffDelay(4, {
        maxDelay: Number.NEGATIVE_INFINITY,
      }),
    ).toBe(0);
  });
});

describe('linearBackoffDelay', () => {
  test('returns correct default delay for retry number 1', () => {
    const delay = linearBackoffDelay(1);
    expect(delay).toBeGreaterThanOrEqual(1000);
    expect(delay).toBeLessThanOrEqual(1200);
  });

  test('returns correct default delay for retry number 2', () => {
    const delay = linearBackoffDelay(2);
    expect(delay).toBeGreaterThanOrEqual(2000);
    expect(delay).toBeLessThanOrEqual(2400);
  });

  test('returns correct default delay for retry number 3', () => {
    const delay = linearBackoffDelay(3);
    expect(delay).toBeGreaterThanOrEqual(3000);
    expect(delay).toBeLessThanOrEqual(3600);
  });

  test('returns correct default delay for retry number 4', () => {
    const delay = linearBackoffDelay(4);
    expect(delay).toBeGreaterThanOrEqual(4000);
    expect(delay).toBeLessThanOrEqual(4800);
  });

  test('returns correct delay when `maxRandomDelayFraction` is 0', () => {
    const options: BackoffDelayOptions = { maxRandomDelayFraction: 0 };

    expect(linearBackoffDelay(1, options)).toBe(1000);
    expect(linearBackoffDelay(2, options)).toBe(2000);
    expect(linearBackoffDelay(3, options)).toBe(3000);
    expect(linearBackoffDelay(4, options)).toBe(4000);
  });

  test('throws when retry number is invalid', () => {
    const error = 'Retry number must be greater than 0';

    expect(() => linearBackoffDelay(0)).toThrow(error);
    expect(() => linearBackoffDelay(-1)).toThrow(error);
    expect(() => linearBackoffDelay(Number.NEGATIVE_INFINITY)).toThrow(error);
  });

  test('returns correct delay when custom `maxRandomDelayFraction` is set for retry number 1', () => {
    const delay = linearBackoffDelay(1, {
      maxRandomDelayFraction: 0.5,
    });

    expect(delay).toBeGreaterThanOrEqual(1000);
    expect(delay).toBeLessThanOrEqual(1500);
  });

  test('returns correct delay when custom `maxRandomDelayFraction` is set for retry number 2', () => {
    const delay = linearBackoffDelay(2, {
      maxRandomDelayFraction: 0.5,
    });

    expect(delay).toBeGreaterThanOrEqual(2000);
    expect(delay).toBeLessThanOrEqual(3000);
  });

  test('throws when `maxRandomDelayFraction` is invalid', () => {
    const error =
      'Maximum random delay fraction must be greater than or equal to 0';

    expect(() =>
      linearBackoffDelay(1, {
        maxRandomDelayFraction: -1,
      }),
    ).toThrow(error);

    expect(() =>
      linearBackoffDelay(1, {
        maxRandomDelayFraction: Number.NEGATIVE_INFINITY,
      }),
    ).toThrow(error);
  });

  test('returns delay that cannot exceed `maxDelay`', () => {
    expect(
      linearBackoffDelay(1, { maxDelay: 100, maxRandomDelayFraction: 0 }),
    ).toBe(100);
    expect(
      linearBackoffDelay(2, { maxDelay: 1999, maxRandomDelayFraction: 0 }),
    ).toBe(1999);
    expect(
      linearBackoffDelay(4, { maxDelay: 3999, maxRandomDelayFraction: 0 }),
    ).toBe(3999);
  });

  test('returns normal delay when `maxDelay` is set to positive infinity', () => {
    const delay = linearBackoffDelay(4, {
      maxDelay: Number.POSITIVE_INFINITY,
    });

    expect(delay).toBeGreaterThanOrEqual(4000);
    expect(delay).toBeLessThanOrEqual(4800);
  });

  test('returns delay of 0 when `maxDelay` is set to negative infinity', () => {
    expect(
      linearBackoffDelay(4, {
        maxDelay: Number.NEGATIVE_INFINITY,
      }),
    ).toBe(0);
  });

  test('returns correct delay when a custom `interval` is set', () => {
    const options: LinearBackoffDelayOptions = {
      interval: 2000,
      // Set to 0 so we don't need to do range assertions
      maxRandomDelayFraction: 0,
    };
    expect(linearBackoffDelay(1, options)).toBe(2000);
    expect(linearBackoffDelay(2, options)).toBe(4000);
    expect(linearBackoffDelay(3, options)).toBe(6000);
    expect(linearBackoffDelay(4, options)).toBe(8000);
  });
});
