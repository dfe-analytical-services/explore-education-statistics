import parseNumber from '@common/utils/number/parseNumber';

describe('parseNumber', () => {
  test('parses string to numbers', () => {
    expect(parseNumber('123')).toBe(123);
    expect(parseNumber('123.44')).toBe(123.44);
  });

  test('returns number values as is', () => {
    expect(parseNumber(123)).toBe(123);
    expect(parseNumber(123.44)).toBe(123.44);
  });

  test('returns undefined if value is NaN', () => {
    expect(parseNumber(NaN)).toBeUndefined();
  });

  test('returns undefined if invalid string values', () => {
    expect(parseNumber('')).toBeUndefined();
    expect(parseNumber('   ')).toBeUndefined();
    expect(parseNumber('not a number')).toBeUndefined();
  });
});
