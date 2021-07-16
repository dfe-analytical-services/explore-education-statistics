import reorderMultiple from '@common/utils/reorderMultiple';

describe('reorderMultiple', () => {
  const list = ['a', 'b', 'c', 'd'];

  test('does not move any items if selected indices do not exist', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [-1, 4],
    });

    expect(newList).toEqual(['a', 'b', 'c', 'd']);
  });

  test('does not move any items if no indices selected', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [],
    });

    expect(newList).toEqual(['a', 'b', 'c', 'd']);
  });

  test('does not move item if destination index is less than 0', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: -1,
      selectedIndices: [2],
    });

    expect(newList).toEqual(['a', 'b', 'c', 'd']);
  });

  test('move item to bottom if destination index is larger than list size', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 4,
      selectedIndices: [0],
    });

    expect(newList).toEqual(['b', 'c', 'd', 'a']);
  });

  test('move item to its own position', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 1,
      selectedIndices: [1],
    });

    expect(newList).toEqual(['a', 'b', 'c', 'd']);
  });

  test('move top item to the bottom', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [0],
    });

    expect(newList).toEqual(['b', 'c', 'd', 'a']);
  });

  test('move bottom item to the top', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [3],
    });

    expect(newList).toEqual(['d', 'a', 'b', 'c']);
  });

  test('move first middle item to the top', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [1],
    });

    expect(newList).toEqual(['b', 'a', 'c', 'd']);
  });

  test('move second middle item to the top', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [2],
    });

    expect(newList).toEqual(['c', 'a', 'b', 'd']);
  });

  test('move first middle item to the bottom', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [1],
    });

    expect(newList).toEqual(['a', 'c', 'd', 'b']);
  });

  test('move second middle item to the bottom', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [2],
    });

    expect(newList).toEqual(['a', 'b', 'd', 'c']);
  });

  test('move two items from the middle to the top', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [1, 2],
    });

    expect(newList).toEqual(['b', 'c', 'a', 'd']);
  });

  test('move two items from the middle to the bottom', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [1, 2],
    });

    expect(newList).toEqual(['a', 'd', 'b', 'c']);
  });

  test('move two items that are not next to each other to the middle', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 1,
      selectedIndices: [0, 2],
    });

    expect(newList).toEqual(['b', 'a', 'c', 'd']);
  });

  test("move two items that are not next to each other to the former's previous position", () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [0, 2],
    });

    expect(newList).toEqual(['a', 'c', 'b', 'd']);
  });

  test("move two items that are not next to each other to the latter's previous position", () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 2,
      selectedIndices: [0, 2],
    });

    expect(newList).toEqual(['b', 'd', 'a', 'c']);
  });

  test('move two items that were not next to each other to the bottom', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [0, 2],
    });

    expect(newList).toEqual(['b', 'd', 'a', 'c']);
  });

  test('move two items that were not next to each other to the top', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [1, 3],
    });

    expect(newList).toEqual(['b', 'd', 'a', 'c']);
  });

  test('move two items from the top to the middle', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 1,
      selectedIndices: [0, 1],
    });

    expect(newList).toEqual(['c', 'a', 'b', 'd']);
  });

  test('move two items from the bottom to the middle', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 1,
      selectedIndices: [2, 3],
    });

    expect(newList).toEqual(['a', 'c', 'd', 'b']);
  });

  test('move three items from the bottom to the top', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 0,
      selectedIndices: [1, 2, 3],
    });

    expect(newList).toEqual(['b', 'c', 'd', 'a']);
  });

  test('move three items from the top to the bottom', () => {
    const newList = reorderMultiple({
      list,
      destinationIndex: 3,
      selectedIndices: [0, 1, 2],
    });

    expect(newList).toEqual(['d', 'a', 'b', 'c']);
  });

  test('move three items from the middle to the top', () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 0,
      selectedIndices: [1, 2, 3],
    });

    expect(newList).toEqual(['b', 'c', 'd', 'a', 'e']);
  });

  test('move three items from the middle to the bottom', () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 4,
      selectedIndices: [1, 2, 3],
    });

    expect(newList).toEqual(['a', 'e', 'b', 'c', 'd']);
  });

  test("move three items from the middle to first item's position", () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 1,
      selectedIndices: [1, 2, 3],
    });

    expect(newList).toEqual(['a', 'b', 'c', 'd', 'e']);
  });

  test("move three items from the middle to second item's position", () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 2,
      selectedIndices: [1, 2, 3],
    });

    expect(newList).toEqual(['a', 'e', 'b', 'c', 'd']);
  });

  test("move three items from the middle to third item's position", () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 3,
      selectedIndices: [1, 2, 3],
    });

    expect(newList).toEqual(['a', 'e', 'b', 'c', 'd']);
  });

  test('move three items apart from each other to the top', () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 0,
      selectedIndices: [0, 2, 4],
    });

    expect(newList).toEqual(['a', 'c', 'e', 'b', 'd']);
  });

  test('move three items apart from each other to the bottom', () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 4,
      selectedIndices: [0, 2, 4],
    });

    expect(newList).toEqual(['b', 'd', 'a', 'c', 'e']);
  });

  test('move three items apart from each other to the middle', () => {
    const newList = reorderMultiple({
      list: [...list, 'e'],
      destinationIndex: 3,
      selectedIndices: [0, 2, 4],
    });

    expect(newList).toEqual(['b', 'd', 'a', 'c', 'e']);
  });
});
