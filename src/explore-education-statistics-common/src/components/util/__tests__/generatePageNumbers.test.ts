import generatePageNumbers from '@common/components/util/generatePageNumbers';

describe('generatePageNumbers', () => {
  test('returns an array of all numbers when totalPages is less than maxReturnSize', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxReturnSize: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxReturnSize: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxReturnSize: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxReturnSize: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);
  });

  test('returns an array of all numbers when totalPages equals maxReturnSize', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxReturnSize: 7,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7]);
  });

  test('returns an array with pages around the current page and spacers up to the maxReturnSize if totalPages is greater than maxReturnSize', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 4, 5, 6, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 5, 6, 7, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 8,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 9,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 10,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);
  });

  test('handles a smaller than default maxReturnSize', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxReturnSize: 6,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, null, 7]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxReturnSize: 6,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, null, 7]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxReturnSize: 6,
        totalPages: 7,
      }),
    ).toEqual([1, 2, 3, 4, null, 7]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxReturnSize: 6,
        totalPages: 7,
      }),
    ).toEqual([1, null, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxReturnSize: 6,
        totalPages: 7,
      }),
    ).toEqual([1, null, 4, 5, 6, 7]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxReturnSize: 6,
        totalPages: 7,
      }),
    ).toEqual([1, null, 4, 5, 6, 7]);
  });

  test('handles a larger than default maxReturnSize', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 4, 5, 6, 7, 8, 9, 10, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 8,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 5, 6, 7, 8, 9, 10, 11, null, 15]);

    expect(
      generatePageNumbers({
        currentPage: 9,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 10,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 11,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 12,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 13,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 14,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

    expect(
      generatePageNumbers({
        currentPage: 15,
        maxReturnSize: 12,
        totalPages: 15,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
  });

  test('uses the last page as the current page when currentPage is greater than totalPages', () => {
    expect(
      generatePageNumbers({
        currentPage: 6,
        maxReturnSize: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 12,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, 9, 10]);
  });

  test('uses the first page as the current page when currentPage is 0', () => {
    expect(
      generatePageNumbers({
        currentPage: 0,
        maxReturnSize: 7,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 0,
        maxReturnSize: 7,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, 5, null, 10]);
  });

  test('uses the minimum return size if maxReturnSize below it', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxReturnSize: 3,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxReturnSize: 3,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxReturnSize: 3,
        totalPages: 6,
      }),
    ).toEqual([1, 2, 3, null, 6]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxReturnSize: 3,
        totalPages: 6,
      }),
    ).toEqual([1, null, 4, 5, 6]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxReturnSize: 3,
        totalPages: 6,
      }),
    ).toEqual([1, null, 4, 5, 6]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxReturnSize: 3,
        totalPages: 6,
      }),
    ).toEqual([1, null, 4, 5, 6]);
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
});
