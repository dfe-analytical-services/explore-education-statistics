import generatePageNumbers from '@common/components/util/generatePageNumbers';

describe('generatePageNumbers', () => {
  test('returns an array of all numbers when totalPages is less than maxItems', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);
  });

  test('returns an array of all numbers when totalPages equals maxItems', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxItems: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);
  });

  test('returns an array with pages around the current page and spacers up to the maxItems if totalPages is greater than maxItems', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 4, 5, 6, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 5, 6, 7, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 8,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 9,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 10,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);
  });

  test('handles a smaller than default maxItems', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 6,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, null, 7]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxItems: 6,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, null, 7]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxItems: 6,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, null, 7]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxItems: 6,
        totalPages: 7,
      }),
    ).toEqual([1, null, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxItems: 6,
        totalPages: 7,
      }),
    ).toEqual([1, null, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxItems: 6,
        totalPages: 7,
      }),
    ).toEqual([1, null, 4, 5, 6, 7]);
  });

  test('handles a larger than default maxItems', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 8,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 5, 6, 7, 8, 9, 10, 11, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 9,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 10,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 11,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 12,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 13,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 14,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 15,
        maxItems: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
  });

  test('uses the last page as the current page when currentPage is greater than totalPages', () => {
    expect(
      generatePageNumbers({
        currentPage: 6,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 12,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);
  });

  test('uses the first page as the current page when currentPage is 0', () => {
    expect(
      generatePageNumbers({
        currentPage: 0,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 0,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);
  });

  test('uses the first page as the current page when currentPage is less than 0', () => {
    expect(
      generatePageNumbers({
        currentPage: -2,
        maxItems: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: -2,
        maxItems: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);
  });

  test('uses the minimum return size if maxItems is less than it', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 3,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxItems: 3,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxItems: 3,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxItems: 3,
        totalPages: 6,
      }),
    ).toEqual([1, null, 4, 5, 6]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxItems: 3,
        totalPages: 6,
      }),
    ).toEqual([1, null, 4, 5, 6]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxItems: 3,
        totalPages: 6,
      }),
    ).toEqual([1, null, 4, 5, 6]);
  });

  test('uses the minimum return size if maxItems is 0', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: 0,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);
  });

  test('uses the minimum return size if maxItems is less than 0', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxItems: -2,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);
  });

  test('returns an empty array when totalPages is 0', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        totalPages: 0,
      }),
    ).toEqual([]);
  });

  test('returns an empty array when totalPages is 1', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        totalPages: 1,
      }),
    ).toEqual([]);
  });

  test('returns an empty array when totalPages is less than 0', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        totalPages: -3,
      }),
    ).toEqual([]);
  });
});
