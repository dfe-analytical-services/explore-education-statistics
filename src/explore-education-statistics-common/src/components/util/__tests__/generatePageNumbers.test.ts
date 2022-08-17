import generatePageNumbers from '@common/components/util/generatePageNumbers';

describe('generatePageNumbers', () => {
  test('return an array of all numbers up to the total if fewer than or equal to the max page numbers', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 4,
      }),
    ).toEqual([1, 2, 3, 4]);

    expect(
      generatePageNumbers({
        currentPage: 1,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 8,
      }),
    ).toEqual([1, 2, 3, 4, 5, 6, 7, 8]);
  });

  test('returns an array with null spacers if more than the max page numbers', () => {
    expect(
      generatePageNumbers({
        currentPage: 1,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 2,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 3,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, 2, 3, 4, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 4,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 3, 4, 5, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 5,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 4, 5, 6, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 6,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 5, 6, 7, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 7,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 6, 7, 8, null, 10]);

    expect(
      generatePageNumbers({
        currentPage: 8,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 9,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 7, 8, 9, 10]);

    expect(
      generatePageNumbers({
        currentPage: 10,
        maxPageNumbers: 8,
        offset: 2,
        totalPages: 10,
      }),
    ).toEqual([1, null, 8, 9, 10]);
  });
});
