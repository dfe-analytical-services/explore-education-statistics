import countDecimalPlaces from '@common/utils/number/countDecimalPlaces';

describe('countDecimalPlaces', () => {
  test('returns 0 when no decimal places', () => {
    expect(countDecimalPlaces(0)).toBe(0);
    expect(countDecimalPlaces(1)).toBe(0);
    expect(countDecimalPlaces(10)).toBe(0);
    expect(countDecimalPlaces(100)).toBe(0);
  });

  test('returns 0 when no decimal places for numeric strings', () => {
    expect(countDecimalPlaces('0')).toBe(0);
    expect(countDecimalPlaces('1')).toBe(0);
    expect(countDecimalPlaces('10')).toBe(0);
    expect(countDecimalPlaces('100')).toBe(0);
    expect(countDecimalPlaces('2e4')).toBe(0);
    expect(countDecimalPlaces('-2e4')).toBe(0);
  });

  test('returns 0 when no decimal places for numeric scientific string', () => {
    expect(countDecimalPlaces('2e4')).toBe(0);
    expect(countDecimalPlaces('-2e4')).toBe(0);
  });

  test('returns 0 for numeric edge case strings', () => {
    expect(countDecimalPlaces('0.')).toBe(0);
    expect(countDecimalPlaces('1.')).toBe(0);
    expect(countDecimalPlaces('-1.')).toBe(0);
    expect(countDecimalPlaces('0x23')).toBe(0);
    expect(countDecimalPlaces('0x23')).toBe(0);
  });

  test('returns undefined for invalid values', () => {
    expect(countDecimalPlaces(NaN)).toBeUndefined();
    expect(countDecimalPlaces(Infinity)).toBeUndefined();
    expect(countDecimalPlaces('0.ABC')).toBeUndefined();
    expect(countDecimalPlaces('0.123.234')).toBeUndefined();
    expect(countDecimalPlaces('')).toBeUndefined();
    expect(countDecimalPlaces('  ')).toBeUndefined();
    expect(countDecimalPlaces('not a number')).toBeUndefined();
    expect(countDecimalPlaces('undefined')).toBeUndefined();
    expect(countDecimalPlaces('Infinity')).toBeUndefined();
    expect(countDecimalPlaces('NaN')).toBeUndefined();
  });

  test('returns non-zero decimal places', () => {
    expect(countDecimalPlaces(0.1)).toBe(1);
    expect(countDecimalPlaces(0.02)).toBe(2);
    expect(countDecimalPlaces(0.003)).toBe(3);
    expect(countDecimalPlaces(0.0004)).toBe(4);

    expect(countDecimalPlaces(2.3232e2)).toBe(2);
    expect(countDecimalPlaces(-2.3232e2)).toBe(2);
  });

  test('returns non-zero decimal places for numeric strings', () => {
    expect(countDecimalPlaces('0.1')).toBe(1);
    expect(countDecimalPlaces('0.02')).toBe(2);
    expect(countDecimalPlaces('0.003')).toBe(3);
    expect(countDecimalPlaces('0.0004')).toBe(4);

    expect(countDecimalPlaces('-0.1')).toBe(1);
    expect(countDecimalPlaces('-0.02')).toBe(2);
    expect(countDecimalPlaces('-0.003')).toBe(3);
    expect(countDecimalPlaces('-0.0004')).toBe(4);
  });

  test('returns non-zero decimal places for numeric strings with fractional part of 0', () => {
    expect(countDecimalPlaces('1.0')).toBe(1);
    expect(countDecimalPlaces('1.00')).toBe(2);
    expect(countDecimalPlaces('1.000')).toBe(3);

    expect(countDecimalPlaces('-1.0')).toBe(1);
    expect(countDecimalPlaces('-1.00')).toBe(2);
    expect(countDecimalPlaces('-1.000')).toBe(3);
  });

  test('returns non-zero decimal places for numeric scientific strings', () => {
    expect(countDecimalPlaces('2.333e2')).toBe(1);
    expect(countDecimalPlaces('2.3333e2')).toBe(2);
    expect(countDecimalPlaces('2.33333e2')).toBe(3);

    expect(countDecimalPlaces('-2.333e2')).toBe(1);
    expect(countDecimalPlaces('-2.3333e2')).toBe(2);
    expect(countDecimalPlaces('-2.33333e2')).toBe(3);
  });
});
