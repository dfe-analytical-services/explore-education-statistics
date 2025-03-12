import isUndefinedOrWhitespace from '../isUndefinedOrWhitespace';

describe('isUndefinedOrWhitespace', () => {
  test('undefined returns true', () => {
    expect(isUndefinedOrWhitespace(undefined)).toBe(true);
  });

  test('empty string returns true', () => {
    expect(isUndefinedOrWhitespace('')).toBe(true);
  });

  test.each([' ', '  '])('whitespace returns true', (value: string) => {
    expect(isUndefinedOrWhitespace(value)).toBe(true);
  });

  test.each(['a', '  a', '1', ' 1'])(
    'string containing non-whitespace characters returns false',
    (value: string) => {
      expect(isUndefinedOrWhitespace(value)).toBe(false);
    },
  );
});
