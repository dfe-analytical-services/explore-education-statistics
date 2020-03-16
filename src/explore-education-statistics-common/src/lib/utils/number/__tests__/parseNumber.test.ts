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

  test('changes NaNs to undefined by default', () => {
    expect(parseNumber(NaN)).toBeUndefined();
  });

  test('changes NaNs to a default value', () => {
    expect(parseNumber(NaN, 12)).toBe(12);
    expect(parseNumber(NaN, 12.34)).toBe(12.34);
  });

  test('changes invalid string values to undefined by default', () => {
    expect(parseNumber('')).toBeUndefined();
    expect(parseNumber('not a number')).toBeUndefined();
  });

  test('changes invalid string values to a default value', () => {
    expect(parseNumber('', 12)).toBe(12);
    expect(parseNumber('not a number', 12)).toBe(12);
    expect(parseNumber('not a number', 12.34)).toBe(12.34);
  });
});
