import commaList from '../commaList';

describe('commaList', () => {
  test('creates correct comma list for list with 3 items', () => {
    expect(commaList(['cat', 'dog', 'burger'])).toBe('cat, dog and burger');
  });

  test('creates correct comma list for list with 2 items', () => {
    expect(commaList(['cat', 'dog'])).toBe('cat and dog');
  });

  test('filters empty item from comma list with 3 items', () => {
    expect(commaList(['cat', 'dog', '', 'burger'])).toBe('cat, dog and burger');
  });

  test('filters empty item from comma list with 2 items', () => {
    expect(commaList(['cat', '', 'burger'])).toBe('cat and burger');
  });

  test('filters multiple empty items from comma list with 3 items', () => {
    expect(commaList(['cat', '   ', 'dog', '', 'burger'])).toBe(
      'cat, dog and burger',
    );
  });

  test('filters multiple empty items from comma list with 2 items', () => {
    expect(commaList(['cat', '    ', 'burger', ''])).toBe('cat and burger');
  });

  test('returns single item for single item list', () => {
    expect(commaList(['cat'])).toBe('cat');
  });

  test('returns empty string for 0 items', () => {
    expect(commaList([])).toBe('');
  });
});
