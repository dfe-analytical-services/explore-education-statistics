import countDecimals from '@common/utils/number/countDecimals';

describe('countDecimals', () => {
  test('returns 0 when there are no decimal places', () => {
    expect(countDecimals(0)).toBe(0);
    expect(countDecimals(1)).toBe(0);
    expect(countDecimals(10)).toBe(0);
    expect(countDecimals(100)).toBe(0);
  });

  test('returns 0 when there are no decimal places for numeric strings', () => {
    expect(countDecimals('0')).toBe(0);
    expect(countDecimals('1')).toBe(0);
    expect(countDecimals('10')).toBe(0);
    expect(countDecimals('100')).toBe(0);
  });

  test('returns 0 when there are no decimal places for non-numeric strings', () => {
    expect(countDecimals('0.')).toBe(0);
    expect(countDecimals('0.ABC')).toBe(0);
    expect(countDecimals('0.123.234')).toBe(0);
    expect(countDecimals('')).toBe(0);
    expect(countDecimals('not a number')).toBe(0);
  });

  test('returns correct non-zero decimals places', () => {
    expect(countDecimals(0.1)).toBe(1);
    expect(countDecimals(0.02)).toBe(2);
    expect(countDecimals(0.003)).toBe(3);
    expect(countDecimals(0.0004)).toBe(4);
  });

  test('returns correct non-zero decimals places for numeric strings', () => {
    expect(countDecimals('0.1')).toBe(1);
    expect(countDecimals('0.02')).toBe(2);
    expect(countDecimals('0.003')).toBe(3);
    expect(countDecimals('0.0004')).toBe(4);

    expect(countDecimals('1.0')).toBe(1);
    expect(countDecimals('1.00')).toBe(2);
    expect(countDecimals('1.000')).toBe(3);
  });
});
