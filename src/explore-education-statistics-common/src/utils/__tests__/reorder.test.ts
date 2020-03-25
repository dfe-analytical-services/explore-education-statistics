import reorder from '../reorder';

describe('reorder', () => {
  test('moving value from top to bottom of list', () => {
    const list = [1, 2, 3, 4];
    const newList = reorder(list, 0, 3);

    expect(newList).toEqual([2, 3, 4, 1]);
  });

  test('moving value from bottom to top of list', () => {
    const list = [1, 2, 3, 4];
    const newList = reorder(list, 3, 0);

    expect(newList).toEqual([4, 1, 2, 3]);
  });

  test('moving value in middle of list down one', () => {
    const list = [1, 2, 3, 4];
    const newList = reorder(list, 1, 2);

    expect(newList).toEqual([1, 3, 2, 4]);
  });

  test('moving value in middle of list up one', () => {
    const list = [1, 2, 3, 4];
    const newList = reorder(list, 2, 1);

    expect(newList).toEqual([1, 3, 2, 4]);
  });

  test('moving value at top of list down one', () => {
    const list = [1, 2, 3, 4];
    const newList = reorder(list, 0, 1);

    expect(newList).toEqual([2, 1, 3, 4]);
  });

  test('moving value from bottom of list up one', () => {
    const list = [1, 2, 3, 4];
    const newList = reorder(list, 3, 2);

    expect(newList).toEqual([1, 2, 4, 3]);
  });
});
