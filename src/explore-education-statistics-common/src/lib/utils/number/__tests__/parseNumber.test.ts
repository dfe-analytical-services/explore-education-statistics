import parseNumber from '@common/lib/utils/number/parseNumber';

describe('parseNumber', () => {
  test('parses string to numbers', () => {
    expect(parseNumber('123')).toBe(123);
    expect(parseNumber('123.44')).toBe(123.44);
  });

  test('returns number values as is', () => {
    expect(parseNumber(123)).toBe(123);
    expect(parseNumber(123.44)).toBe(123.44);
  });

  test('changes NaNs to undefined', () => {
    expect(parseNumber(NaN)).toBeUndefined();
  });

  test('changes invalid string values to undefined by default', () => {
    expect(parseNumber('')).toBeUndefined();
    expect(parseNumber('not a number')).toBeUndefined();
  });
});
