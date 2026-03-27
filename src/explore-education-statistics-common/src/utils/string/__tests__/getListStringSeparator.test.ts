import getListStringSeparator from '../getListStringSeparator';

describe('commaList', () => {
  test('returns no conjunction for a null list', () => {
    expect(getListStringSeparator(null!, 0)).toBe('');
  });

  test('returns no conjunction for an empty list', () => {
    expect(getListStringSeparator([], 0)).toBe('');
  });

  test('returns no conjunction for a null index', () => {
    expect(getListStringSeparator(['cat', 'dog', 'burger'], null!)).toBe('');
  });

  test('returns no conjunction for an out of range index', () => {
    expect(getListStringSeparator(['cat', 'dog', 'burger'], 3)).toBe('');
  });

  test('returns no conjunction for the first item', () => {
    expect(getListStringSeparator(['cat', 'dog', 'burger'], 0)).toBe('');
  });

  test('returns a comma conjunction for the middle item', () => {
    expect(getListStringSeparator(['cat', 'dog', 'burger'], 1)).toBe(', ');
  });

  test('returns an "and" conjunction for the last item', () => {
    expect(getListStringSeparator(['cat', 'dog', 'burger'], 2)).toBe(' and ');
  });
});
