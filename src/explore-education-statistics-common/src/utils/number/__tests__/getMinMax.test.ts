import getMinMax from '@common/utils/number/getMinMax';

describe('getMinMax', () => {
  test('returns the correct min/max for multiple values', () => {
    const { min, max } = getMinMax([5, 2, 80, -44, 12]);

    expect(min).toBe(-44);
    expect(max).toBe(80);
  });

  test('returns the correct min/max when there are non-numeric values', () => {
    const { min, max } = getMinMax([
      null,
      5,
      2,
      80,
      undefined,
      -44,
      12,
      Number.NaN,
      'not a number',
      '90', // Numeric string, but will not be considered as numeric value
    ]);

    expect(min).toBe(-44);
    expect(max).toBe(80);
  });

  test('returns no min/max if only non-numeric values', () => {
    const { min, max } = getMinMax([
      null,
      undefined,
      Number.NaN,
      'not a number',
      '90', // Numeric string, but will not be considered as numeric value
    ]);

    expect(min).toBeUndefined();
    expect(max).toBeUndefined();
  });

  test('returns a single min/max for a single value', () => {
    const { min, max } = getMinMax([5]);

    expect(min).toBe(5);
    expect(max).toBe(5);
  });

  test('returns undefined min/max if there are no values', () => {
    const { min, max } = getMinMax([]);

    expect(min).toBeUndefined();
    expect(max).toBeUndefined();
  });

  test('calls custom iteratee function on each item before min/max is calculated', () => {
    const { min, max } = getMinMax(
      ['5', '2', '80', '-44', '12'],
      value => Number(value) + 10,
    );

    expect(min).toBe(-34);
    expect(max).toBe(90);
  });
});
