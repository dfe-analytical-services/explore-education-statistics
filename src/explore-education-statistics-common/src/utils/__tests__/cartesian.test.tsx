import cartesian from '../cartesian';

describe('cartesian', () => {
  test('creates lists for every item of a single input list', () => {
    expect(cartesian(['a', 'b'])).toEqual([['a'], ['b']]);
  });

  test('creates empty list for no input lists', () => {
    expect(cartesian()).toEqual([]);
  });

  test('creates empty list for empty input list', () => {
    expect(cartesian([])).toEqual([]);
  });

  test('creates empty list for multiple empty lists', () => {
    expect(cartesian([], [], [])).toEqual([]);
  });

  test('creates combined lists for every item of two input lists', () => {
    expect(cartesian(['a', 'b'], ['c', 'd'])).toEqual([
      ['a', 'c'],
      ['a', 'd'],
      ['b', 'c'],
      ['b', 'd'],
    ]);
  });

  test('creates combined lists for every item of three input lists', () => {
    expect(cartesian(['a', 'b'], ['c', 'd'], ['e', 'f'])).toEqual([
      ['a', 'c', 'e'],
      ['a', 'c', 'f'],
      ['a', 'd', 'e'],
      ['a', 'd', 'f'],
      ['b', 'c', 'e'],
      ['b', 'c', 'f'],
      ['b', 'd', 'e'],
      ['b', 'd', 'f'],
    ]);
  });

  test('creates empty list when at least one list is empty', () => {
    expect(cartesian(['a', 'b'], [], ['e', 'f'])).toEqual([]);
  });
});
