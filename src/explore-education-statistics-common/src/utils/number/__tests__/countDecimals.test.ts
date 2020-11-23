import countDecimals from '@common/utils/number/countDecimals';

describe('countDecimals', () => {
  test('returns 0 when there are no decimal places', () => {
    expect(countDecimals(0)).toBe(0);
    expect(countDecimals(1)).toBe(0);
    expect(countDecimals(10)).toBe(0);
    expect(countDecimals(100)).toBe(0);
  });

  test('returns correct number of decimals places when there are some', () => {
    expect(countDecimals(0.1)).toBe(1);
    expect(countDecimals(0.02)).toBe(2);
    expect(countDecimals(0.003)).toBe(3);
    expect(countDecimals(0.0004)).toBe(4);
  });
});
