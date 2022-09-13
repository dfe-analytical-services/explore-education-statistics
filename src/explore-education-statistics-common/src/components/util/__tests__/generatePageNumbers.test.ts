import generatePageNumbers from '@common/components/util/generatePageNumbers';

describe('generatePageNumbers', () => {
  describe('small windowSize', () => {
    test('returns an array of the current page and spacers when total pages is less than 2', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 2,
          windowSize: 's',
        }),
      ).toEqual([1, 2]);

      expect(
        generatePageNumbers({
          currentPage: 2,
          totalPages: 2,
          windowSize: 's',
        }),
      ).toEqual([1, 2]);

      expect(
        generatePageNumbers({
          currentPage: 3,
          totalPages: 2,
          windowSize: 's',
        }),
      ).toEqual([1, 2]);
    });

    test('returns an array of the current page and spacers when total pages is 2', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 3,
          windowSize: 's',
        }),
      ).toEqual([1, null, 3]);

      expect(
        generatePageNumbers({
          currentPage: 2,
          totalPages: 3,
          windowSize: 's',
        }),
      ).toEqual([1, 2, 3]);

      expect(
        generatePageNumbers({
          currentPage: 3,
          totalPages: 3,
          windowSize: 's',
        }),
      ).toEqual([1, null, 3]);
    });

    test('returns an array of the current page and spacers when total pages is greater than 3', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 2,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, 2, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 3,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 3, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 4,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 4, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 5,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 5, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 6,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 6, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 7,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 7, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 8,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 8, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 9,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 9, 10]);

      expect(
        generatePageNumbers({
          currentPage: 10,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 10]);
    });

    test('uses the first page as the current page when the currentPage is 0', () => {
      expect(
        generatePageNumbers({
          currentPage: 0,
          totalPages: 4,
          windowSize: 's',
        }),
      ).toEqual([1, null, 4]);

      expect(
        generatePageNumbers({
          currentPage: 0,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 10]);
    });

    test('uses the first page as the current page when the currentPage is less than 0', () => {
      expect(
        generatePageNumbers({
          currentPage: -2,
          totalPages: 4,
          windowSize: 's',
        }),
      ).toEqual([1, null, 4]);

      expect(
        generatePageNumbers({
          currentPage: -2,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 10]);
    });

    test('uses the last page as the current page when the currentPage is greater than totalPages', () => {
      expect(
        generatePageNumbers({
          currentPage: 6,
          totalPages: 4,
          windowSize: 's',
        }),
      ).toEqual([1, null, 4]);

      expect(
        generatePageNumbers({
          currentPage: 12,
          totalPages: 10,
          windowSize: 's',
        }),
      ).toEqual([1, null, 10]);
    });

    test('returns an empty array when totalPages is 0', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 0,
          windowSize: 's',
        }),
      ).toEqual([]);
    });

    test('returns an empty array when totalPages is less than 0', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: -3,
          windowSize: 's',
        }),
      ).toEqual([]);
    });

    test('returns an empty array when totalPages is 1', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 1,
          windowSize: 's',
        }),
      ).toEqual([]);
    });
  });

  describe('medium windowSize', () => {
    test('returns an array of all numbers when totalPages is less than 7', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);

      expect(
        generatePageNumbers({
          currentPage: 2,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);

      expect(
        generatePageNumbers({
          currentPage: 3,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);

      expect(
        generatePageNumbers({
          currentPage: 4,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);
    });

    test('returns an array of all numbers when totalPages is 7', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);

      expect(
        generatePageNumbers({
          currentPage: 2,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);

      expect(
        generatePageNumbers({
          currentPage: 3,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);

      expect(
        generatePageNumbers({
          currentPage: 4,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);

      expect(
        generatePageNumbers({
          currentPage: 5,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);

      expect(
        generatePageNumbers({
          currentPage: 6,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);

      expect(
        generatePageNumbers({
          currentPage: 7,
          totalPages: 7,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, 6, 7]);
    });

    test('returns an array with pages around the current page and spacers if totalPages is greater than 7', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 2,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 3,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 4,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 5,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 4, 5, 6, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 6,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 5, 6, 7, null, 10]);

      expect(
        generatePageNumbers({
          currentPage: 7,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 6, 7, 8, 9, 10]);

      expect(
        generatePageNumbers({
          currentPage: 8,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 6, 7, 8, 9, 10]);

      expect(
        generatePageNumbers({
          currentPage: 9,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 6, 7, 8, 9, 10]);

      expect(
        generatePageNumbers({
          currentPage: 10,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 6, 7, 8, 9, 10]);
    });

    test('uses the first page as the current page when currentPage is 0', () => {
      expect(
        generatePageNumbers({
          currentPage: 0,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);

      expect(
        generatePageNumbers({
          currentPage: 0,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, null, 10]);
    });

    test('uses the first page as the current page when currentPage is less than 0', () => {
      expect(
        generatePageNumbers({
          currentPage: -2,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);

      expect(
        generatePageNumbers({
          currentPage: -2,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4, 5, null, 10]);
    });

    test('uses the last page as the current page when the currentPage is greater than totalPages', () => {
      expect(
        generatePageNumbers({
          currentPage: 6,
          totalPages: 4,
          windowSize: 'm',
        }),
      ).toEqual([1, 2, 3, 4]);

      expect(
        generatePageNumbers({
          currentPage: 12,
          totalPages: 10,
          windowSize: 'm',
        }),
      ).toEqual([1, null, 6, 7, 8, 9, 10]);
    });

    test('returns an empty array when totalPages is 0', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 0,
          windowSize: 'm',
        }),
      ).toEqual([]);
    });

    test('returns an empty array when totalPages is 1', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: 1,
          windowSize: 'm',
        }),
      ).toEqual([]);
    });

    test('returns an empty array when totalPages is less than 0', () => {
      expect(
        generatePageNumbers({
          currentPage: 1,
          totalPages: -3,
          windowSize: 'm',
        }),
      ).toEqual([]);
    });
  });
});
