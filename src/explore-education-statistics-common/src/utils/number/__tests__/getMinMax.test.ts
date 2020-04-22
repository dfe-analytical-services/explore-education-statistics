import getMinMax from '@common/utils/number/getMinMax';

describe('getMinMax', () => {
  test('returns the correct min/max for multiple values', () => {
    const { min, max } = getMinMax([5, 2, 80, -44, 12]);

    expect(min).toBe(-44);
    expect(max).toBe(80);
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
});
