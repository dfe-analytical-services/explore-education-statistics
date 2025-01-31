import intersperse from '@common/utils/array/intersperse';

describe('intersperse', () => {
  test('returns no items when given an empty array', () => {
    const result = intersperse([], () => ',');

    expect(result).toEqual([]);
  });

  test('returns one item when given single item', () => {
    const result = intersperse(['item'], () => ',');

    expect(result).toEqual(['item']);
  });

  test('returns three items when given two items', () => {
    const result = intersperse(['item1', 'item2'], () => '-');

    expect(result).toEqual(['item1', '-', 'item2']);
  });

  test('returns five items when given three items', () => {
    const result = intersperse(['item1', 'item2', 'item3'], () => '-');

    expect(result).toEqual(['item1', '-', 'item2', '-', 'item3']);
  });

  test('applies `separator` function correctly', () => {
    const result = intersperse(['item1', 'item2', 'item3'], index => ({
      index,
    }));

    expect(result).toEqual([
      'item1',
      { index: 0 },
      'item2',
      { index: 1 },
      'item3',
    ]);
  });

  test('does not modify the original array', () => {
    const original = ['item1', 'item2', 'item3'];

    const result = intersperse(original, () => '-');

    expect(original).toEqual(['item1', 'item2', 'item3']);
    expect(result).not.toBe(original);
  });
});
