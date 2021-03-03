import naturalOrderBy from '@common/utils/array/naturalOrderBy';

describe('naturalOrderBy', () => {
  describe('single string key', () => {
    const items = [
      { firstName: 'John' },
      { firstName: 'Aaron' },
      { firstName: 'Robert' },
      { firstName: 'Mel' },
    ];

    test('orders items in ascending order', () => {
      const expected = [
        { firstName: 'Aaron' },
        { firstName: 'John' },
        { firstName: 'Mel' },
        { firstName: 'Robert' },
      ];

      expect(naturalOrderBy(items, ['firstName'], ['asc'])).toEqual(expected);
      expect(naturalOrderBy(items, 'firstName', 'asc')).toEqual(expected);
      expect(naturalOrderBy(items, [item => item.firstName], 'asc')).toEqual(
        expected,
      );
      expect(naturalOrderBy(items, item => item.firstName, 'asc')).toEqual(
        expected,
      );
    });

    test('orders items in ascending order by default', () => {
      const expected = [
        { firstName: 'Aaron' },
        { firstName: 'John' },
        { firstName: 'Mel' },
        { firstName: 'Robert' },
      ];

      expect(naturalOrderBy(items, ['firstName'])).toEqual(expected);
      expect(naturalOrderBy(items, 'firstName')).toEqual(expected);

      expect(naturalOrderBy(items, [item => item.firstName])).toEqual(expected);
      expect(naturalOrderBy(items, item => item.firstName)).toEqual(expected);
    });

    test('orders items in descending order', () => {
      const expected = [
        { firstName: 'Robert' },
        { firstName: 'Mel' },
        { firstName: 'John' },
        { firstName: 'Aaron' },
      ];

      expect(naturalOrderBy(items, ['firstName'], ['desc'])).toEqual(expected);
      expect(naturalOrderBy(items, 'firstName', 'desc')).toEqual(expected);

      expect(naturalOrderBy(items, [item => item.firstName], ['desc'])).toEqual(
        expected,
      );
      expect(naturalOrderBy(items, item => item.firstName, 'desc')).toEqual(
        expected,
      );
    });
  });

  describe('multiple string keys', () => {
    const items = [
      { firstName: 'John', lastName: 'Smith' },
      { firstName: 'Mel', lastName: 'Gibbs' },
      { firstName: 'John', lastName: 'Acre' },
      { firstName: 'Aaron', lastName: 'White' },
    ];

    test('orders items in ascending order', () => {
      const expected = [
        { firstName: 'Aaron', lastName: 'White' },
        { firstName: 'John', lastName: 'Acre' },
        { firstName: 'John', lastName: 'Smith' },
        { firstName: 'Mel', lastName: 'Gibbs' },
      ];

      expect(
        naturalOrderBy(items, ['firstName', 'lastName'], ['asc', 'asc']),
      ).toEqual(expected);
      expect(
        naturalOrderBy(
          items,
          [item => item.firstName, item => item.lastName],
          ['asc', 'asc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in ascending order by default', () => {
      const expected = [
        { firstName: 'Aaron', lastName: 'White' },
        { firstName: 'John', lastName: 'Acre' },
        { firstName: 'John', lastName: 'Smith' },
        { firstName: 'Mel', lastName: 'Gibbs' },
      ];

      expect(naturalOrderBy(items, ['firstName', 'lastName'])).toEqual(
        expected,
      );
      expect(naturalOrderBy(items, ['firstName', 'lastName'], ['asc'])).toEqual(
        expected,
      );

      expect(
        naturalOrderBy(items, [item => item.firstName, item => item.lastName]),
      ).toEqual(expected);
      expect(
        naturalOrderBy(
          items,
          [item => item.firstName, item => item.lastName],
          ['asc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order', () => {
      const expected = [
        { firstName: 'Mel', lastName: 'Gibbs' },
        { firstName: 'John', lastName: 'Smith' },
        { firstName: 'John', lastName: 'Acre' },
        { firstName: 'Aaron', lastName: 'White' },
      ];

      expect(
        naturalOrderBy(items, ['firstName', 'lastName'], ['desc', 'desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.firstName, item => item.lastName],
          ['desc', 'desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order and tie-breaks in ascending order by default', () => {
      const expected = [
        { firstName: 'Mel', lastName: 'Gibbs' },
        { firstName: 'John', lastName: 'Acre' },
        { firstName: 'John', lastName: 'Smith' },
        { firstName: 'Aaron', lastName: 'White' },
      ];

      expect(
        naturalOrderBy(items, ['firstName', 'lastName'], ['desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.firstName, item => item.lastName],
          ['desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in ascending order and tie-breaks in descending order', () => {
      const expected = [
        { firstName: 'Aaron', lastName: 'White' },
        { firstName: 'John', lastName: 'Smith' },
        { firstName: 'John', lastName: 'Acre' },
        { firstName: 'Mel', lastName: 'Gibbs' },
      ];

      expect(
        naturalOrderBy(items, ['firstName', 'lastName'], ['asc', 'desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.firstName, item => item.lastName],
          ['asc', 'desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order and tie-breaks in ascending order', () => {
      const expected = [
        { firstName: 'Mel', lastName: 'Gibbs' },
        { firstName: 'John', lastName: 'Acre' },
        { firstName: 'John', lastName: 'Smith' },
        { firstName: 'Aaron', lastName: 'White' },
      ];

      expect(
        naturalOrderBy(items, ['firstName', 'lastName'], ['desc', 'asc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.firstName, item => item.lastName],
          ['desc', 'asc'],
        ),
      ).toEqual(expected);
    });
  });

  describe('single number key', () => {
    const items = [{ number: 4 }, { number: 3 }, { number: 1 }, { number: 2 }];

    test('orders items by key in ascending order', () => {
      const expected = [
        { number: 1 },
        { number: 2 },
        { number: 3 },
        { number: 4 },
      ];

      expect(naturalOrderBy(items, ['number'], ['asc'])).toEqual(expected);
      expect(naturalOrderBy(items, 'number', 'asc')).toEqual(expected);

      expect(naturalOrderBy(items, [item => item.number], ['asc'])).toEqual(
        expected,
      );
      expect(naturalOrderBy(items, item => item.number, 'asc')).toEqual(
        expected,
      );
    });

    test('orders items by key in descending order', () => {
      const expected = [
        { number: 4 },
        { number: 3 },
        { number: 2 },
        { number: 1 },
      ];

      expect(naturalOrderBy(items, ['number'], ['desc'])).toEqual(expected);
      expect(naturalOrderBy(items, 'number', 'desc')).toEqual(expected);

      expect(naturalOrderBy(items, [item => item.number], ['desc'])).toEqual(
        expected,
      );
      expect(naturalOrderBy(items, item => item.number, 'desc')).toEqual(
        expected,
      );
    });
  });

  describe('multiple number keys', () => {
    const items = [
      { number: 4, other: 1 },
      { number: 2, other: 4 },
      { number: 1, other: 2 },
      { number: 2, other: 3 },
    ];

    test('orders items in ascending order', () => {
      const expected = [
        { number: 1, other: 2 },
        { number: 2, other: 3 },
        { number: 2, other: 4 },
        { number: 4, other: 1 },
      ];

      expect(
        naturalOrderBy(items, ['number', 'other'], ['asc', 'asc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.other],
          ['asc', 'asc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in ascending order by default', () => {
      const expected = [
        { number: 1, other: 2 },
        { number: 2, other: 3 },
        { number: 2, other: 4 },
        { number: 4, other: 1 },
      ];

      expect(naturalOrderBy(items, ['number', 'other'])).toEqual(expected);
      expect(naturalOrderBy(items, ['number', 'other'], ['asc'])).toEqual(
        expected,
      );

      expect(
        naturalOrderBy(items, [item => item.number, item => item.other]),
      ).toEqual(expected);
      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.other],
          ['asc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order', () => {
      const expected = [
        { number: 4, other: 1 },
        { number: 2, other: 4 },
        { number: 2, other: 3 },
        { number: 1, other: 2 },
      ];

      expect(
        naturalOrderBy(items, ['number', 'other'], ['desc', 'desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.other],
          ['desc', 'desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order and tie-breaks in ascending order by default', () => {
      const expected = [
        { number: 4, other: 1 },
        { number: 2, other: 3 },
        { number: 2, other: 4 },
        { number: 1, other: 2 },
      ];

      expect(naturalOrderBy(items, ['number', 'other'], ['desc'])).toEqual(
        expected,
      );

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.other],
          ['desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in ascending order and tie-breaks in descending order', () => {
      const expected = [
        { number: 1, other: 2 },
        { number: 2, other: 4 },
        { number: 2, other: 3 },
        { number: 4, other: 1 },
      ];

      expect(
        naturalOrderBy(items, ['number', 'other'], ['asc', 'desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.other],
          ['asc', 'desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order and tie-breaks in ascending order', () => {
      const expected = [
        { number: 4, other: 1 },
        { number: 2, other: 3 },
        { number: 2, other: 4 },
        { number: 1, other: 2 },
      ];

      expect(
        naturalOrderBy(items, ['number', 'other'], ['desc', 'asc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.other],
          ['desc', 'asc'],
        ),
      ).toEqual(expected);
    });
  });

  describe('mixed keys', () => {
    const items = [
      { number: 4, firstName: 'Laura' },
      { number: 2, firstName: 'Michelle' },
      { number: 1, firstName: 'Robert' },
      { number: 2, firstName: 'Aaron' },
    ];

    test('orders items in ascending order', () => {
      const expected = [
        { number: 1, firstName: 'Robert' },
        { number: 2, firstName: 'Aaron' },
        { number: 2, firstName: 'Michelle' },
        { number: 4, firstName: 'Laura' },
      ];

      expect(
        naturalOrderBy(items, ['number', 'firstName'], ['asc', 'asc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.firstName],
          ['asc', 'asc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in ascending order by default', () => {
      const expected = [
        { number: 1, firstName: 'Robert' },
        { number: 2, firstName: 'Aaron' },
        { number: 2, firstName: 'Michelle' },
        { number: 4, firstName: 'Laura' },
      ];

      expect(naturalOrderBy(items, ['number', 'firstName'])).toEqual(expected);
      expect(naturalOrderBy(items, ['number', 'firstName'], ['asc'])).toEqual(
        expected,
      );

      expect(
        naturalOrderBy(items, [item => item.number, item => item.firstName]),
      ).toEqual(expected);
      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.firstName],
          ['asc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order', () => {
      const expected = [
        { number: 4, firstName: 'Laura' },
        { number: 2, firstName: 'Michelle' },
        { number: 2, firstName: 'Aaron' },
        { number: 1, firstName: 'Robert' },
      ];

      expect(
        naturalOrderBy(items, ['number', 'firstName'], ['desc', 'desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.firstName],
          ['desc', 'desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order and tie-breaks in ascending order by default', () => {
      const expected = [
        { number: 4, firstName: 'Laura' },
        { number: 2, firstName: 'Aaron' },
        { number: 2, firstName: 'Michelle' },
        { number: 1, firstName: 'Robert' },
      ];

      expect(naturalOrderBy(items, ['number', 'firstName'], ['desc'])).toEqual(
        expected,
      );

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.firstName],
          ['desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in ascending order and tie-breaks in descending order', () => {
      const expected = [
        { number: 1, firstName: 'Robert' },
        { number: 2, firstName: 'Michelle' },
        { number: 2, firstName: 'Aaron' },
        { number: 4, firstName: 'Laura' },
      ];

      expect(
        naturalOrderBy(items, ['number', 'firstName'], ['asc', 'desc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.firstName],
          ['asc', 'desc'],
        ),
      ).toEqual(expected);
    });

    test('orders items in descending order and tie-breaks in ascending order', () => {
      const expected = [
        { number: 4, firstName: 'Laura' },
        { number: 2, firstName: 'Aaron' },
        { number: 2, firstName: 'Michelle' },
        { number: 1, firstName: 'Robert' },
      ];

      expect(
        naturalOrderBy(items, ['number', 'firstName'], ['desc', 'asc']),
      ).toEqual(expected);

      expect(
        naturalOrderBy(
          items,
          [item => item.number, item => item.firstName],
          ['desc', 'asc'],
        ),
      ).toEqual(expected);
    });
  });

  describe('specific cases', () => {
    test('returns empty array if no items', () => {
      expect(naturalOrderBy([], [])).toEqual([]);
    });

    test('does not order items if no keys are provided', () => {
      const items = [
        { firstName: 'John' },
        { firstName: 'Aaron' },
        { firstName: 'Robert' },
        { firstName: 'Mel' },
      ];

      expect(naturalOrderBy(items, [])).toEqual(items);
    });

    test('orders items with random strings in natural order', () => {
      const items = [
        { value: '12345asd' },
        { value: '123asd' },
        { value: '19asd' },
        { value: 'asd12' },
        { value: 'asd123' },
      ];

      const expected = [
        { value: '19asd' },
        { value: '123asd' },
        { value: '12345asd' },
        { value: 'asd12' },
        { value: 'asd123' },
      ];

      expect(naturalOrderBy(items, ['value'])).toEqual(expected);
    });

    test('orders items with dates in natural order', () => {
      const items = [
        { value: '10/10/12' },
        { value: '1/10/12' },
        { value: '01/10/12' },
        { value: '11/10/12' },
      ];

      const expected = [
        { value: '1/10/12' },
        { value: '01/10/12' },
        { value: '10/10/12' },
        { value: '11/10/12' },
      ];

      expect(naturalOrderBy(items, ['value'])).toEqual(expected);
    });

    test('orders items with variety of strings in natural order', () => {
      const items = [
        { value: '3rd' },
        { value: 'Apple' },
        { value: '24th' },
        { value: '99 in the shade' },
        { value: 'Dec' },
        { value: '10000' },
        { value: '101' },
        { value: '$1.23' },
      ];

      const expected = [
        { value: '$1.23' },
        { value: '3rd' },
        { value: '24th' },
        { value: '99 in the shade' },
        { value: '101' },
        { value: '10000' },
        { value: 'Apple' },
        { value: 'Dec' },
      ];

      expect(naturalOrderBy(items, ['value'])).toEqual(expected);
    });

    test('does not order items with invalid values', () => {
      const items = [
        { value: false },
        { value: true },
        { value: undefined },
        { value: null },
        { value: {} },
        {
          value: {
            something: 'else',
          },
        },
        { value: [] },
        { value: ['test'] },
      ];

      expect(naturalOrderBy(items, ['value'] as never)).toEqual(items);
    });
  });
});
