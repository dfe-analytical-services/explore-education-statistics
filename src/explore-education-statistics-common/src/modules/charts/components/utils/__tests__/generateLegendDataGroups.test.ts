import generateLegendDataGroups, {
  LegendDataGroup,
} from '@common/modules/charts/components/utils/generateLegendDataGroups';

describe('generateLegendDataGroups', () => {
  test('returns an empty array when there are no values', () => {
    const dataClasses = generateLegendDataGroups({
      colour: 'rgba(0, 0, 0, 1)',
      groups: 5,
      values: [],
    });

    expect(dataClasses).toHaveLength(0);
  });

  test('returns group min/max values with custom `unit` option', () => {
    const dataClasses = generateLegendDataGroups({
      colour: 'rgba(0, 0, 0, 1)',
      groups: 1,
      values: [1, 2],
      unit: '£',
    });

    expect(dataClasses).toEqual<LegendDataGroup[]>([
      {
        colour: 'rgba(0, 0, 0, 1)',
        decimalPlaces: 0,
        max: '£2',
        maxRaw: 2,
        min: '£1',
        minRaw: 1,
      },
    ]);
  });

  test('returns group min/max values with specific d.p. when using `decimalPlaces` option', () => {
    const dataClasses = generateLegendDataGroups({
      colour: 'rgba(0, 0, 0, 1)',
      decimalPlaces: 3,
      groups: 1,
      values: [1.0111, 2.38911],
    });

    expect(dataClasses).toEqual<LegendDataGroup[]>([
      {
        colour: 'rgba(0, 0, 0, 1)',
        decimalPlaces: 3,
        max: '2.389',
        maxRaw: 2.389,
        min: '1.011',
        minRaw: 1.011,
      },
    ]);
  });

  describe('equal intervals', () => {
    test('creates 1 group when min/max are the same', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        // Doesn't matter how many groups are specified,
        // will still only generate one group.
        groups: 5,
        values: [1, 1],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '1',
          maxRaw: 1,
          min: '1',
          minRaw: 1,
        },
      ]);
    });

    test('creates 1 group where min/max values do not exceed 2 d.p implicitly', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 1,
        values: [1.0111, 2.38911],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '2.39',
          maxRaw: 2.39,
          min: '1.01',
          minRaw: 1.01,
        },
      ]);
    });

    test('creates 2 groups with equally sized integer ranges using integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 2,
        values: [1, 4],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
      ]);
    });

    test('creates 2 groups with unequally sized integer ranges using integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 2,
        values: [1, 5],
      });

      // Not sure what the best way to fix this. Ideally, the first group
      // should be bigger than the second group, but it's the other way
      // round due to a quirk in the algorithm under these conditions.
      // For the sake of time, we've intentionally left this as it's
      // unlikely to cause a real problem for end users.
      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '5',
          maxRaw: 5,
          min: '3',
          minRaw: 3,
        },
      ]);
    });

    test('creates 2 groups with equally sized non-integer ranges using non-integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 2,
        values: [2.2, 6.6],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 1,
          max: '4.3',
          maxRaw: 4.3,
          min: '2.2',
          minRaw: 2.2,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '6.6',
          maxRaw: 6.6,
          min: '4.4',
          minRaw: 4.4,
        },
      ]);
    });

    test('creates 2 groups with equally sized non-integer ranges with 2 d.p when using non-integer values with 1 d.p and a range of 0.1', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 2,
        values: [3.2, 3.3],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 2,
          max: '3.24',
          maxRaw: 3.24,
          min: '3.20',
          minRaw: 3.2,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '3.30',
          maxRaw: 3.3,
          min: '3.25',
          minRaw: 3.25,
        },
      ]);
    });

    test('creates 3 groups with equally sized integer ranges using integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 3,
        values: [1, 3, 6],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '6',
          maxRaw: 6,
          min: '5',
          minRaw: 5,
        },
      ]);
    });

    test('creates 3 groups with unequally sized integer ranges using integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 3,
        values: [1, 3, 5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '5',
          maxRaw: 5,
          min: '5',
          minRaw: 5,
        },
      ]);
    });

    test('creates 3 groups with equally sized non-integer ranges using non-integer values with 1 d.p', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 3,
        values: [2.5, 5.1, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 1,
          max: '4.9',
          maxRaw: 4.9,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 1,
          max: '7.4',
          maxRaw: 7.4,
          min: '5.0',
          minRaw: 5,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '10.0',
          maxRaw: 10,
          min: '7.5',
          minRaw: 7.5,
        },
      ]);
    });

    test('creates 3 groups with equally sized non-integer ranges using non-integer values with 2 d.p', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 3,
        values: [3.33, 9.99, 13.32],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 2,
          max: '6.65',
          maxRaw: 6.65,
          min: '3.33',
          minRaw: 3.33,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 2,
          max: '9.98',
          maxRaw: 9.98,
          min: '6.66',
          minRaw: 6.66,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '13.32',
          maxRaw: 13.32,
          min: '9.99',
          minRaw: 9.99,
        },
      ]);
    });

    test('creates 5 groups with equally sized integer ranges using integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 5,
        values: [5, 1, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 0,
          max: '6',
          maxRaw: 6,
          min: '5',
          minRaw: 5,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 0,
          max: '8',
          maxRaw: 8,
          min: '7',
          minRaw: 7,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '10',
          maxRaw: 10,
          min: '9',
          minRaw: 9,
        },
      ]);
    });

    test('creates 5 groups with unequally sized integer ranges using integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 5,
        values: [5, 1, 9],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 0,
          max: '6',
          maxRaw: 6,
          min: '5',
          minRaw: 5,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 0,
          max: '8',
          maxRaw: 8,
          min: '7',
          minRaw: 7,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '9',
          maxRaw: 9,
          min: '9',
          minRaw: 9,
        },
      ]);
    });

    test('creates 5 groups with equally sized non-integer ranges using non-integer values with 1 d.p', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 5,
        values: [2.5, 10.3, 12.5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 1,
          max: '4.4',
          maxRaw: 4.4,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 1,
          max: '6.4',
          maxRaw: 6.4,
          min: '4.5',
          minRaw: 4.5,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 1,
          max: '8.4',
          maxRaw: 8.4,
          min: '6.5',
          minRaw: 6.5,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 1,
          max: '10.4',
          maxRaw: 10.4,
          min: '8.5',
          minRaw: 8.5,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '12.5',
          maxRaw: 12.5,
          min: '10.5',
          minRaw: 10.5,
        },
      ]);
    });

    test('creates 5 groups with equally sized non-integer ranges using non-integer values with 2 d.p', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'EqualIntervals',
        groups: 5,
        values: [2.45, 10.32, 25.45],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 2,
          max: '7.04',
          maxRaw: 7.04,
          min: '2.45',
          minRaw: 2.45,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 2,
          max: '11.64',
          maxRaw: 11.64,
          min: '7.05',
          minRaw: 7.05,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 2,
          max: '16.24',
          maxRaw: 16.24,
          min: '11.65',
          minRaw: 11.65,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 2,
          max: '20.84',
          maxRaw: 20.84,
          min: '16.25',
          minRaw: 16.25,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '25.45',
          maxRaw: 25.45,
          min: '20.85',
          minRaw: 20.85,
        },
      ]);
    });
  });

  describe('quantiles', () => {
    test('creates 1 group when min/max are the same', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        // Doesn't matter how many groups are specified,
        // will still only generate one group.
        groups: 5,
        values: [1, 1],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '1',
          maxRaw: 1,
          min: '1',
          minRaw: 1,
        },
      ]);
    });

    test('creates 1 group where min/max values do not exceed 2 d.p implicitly', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 1,
        values: [1.0111, 2.38911],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '2.39',
          maxRaw: 2.39,
          min: '1.01',
          minRaw: 1.01,
        },
      ]);
    });

    test('creates 2 groups with integer ranges using integer values with even distribution and odd range', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 2,
        values: [1, 4],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '3',
          maxRaw: 3,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '4',
          minRaw: 4,
        },
      ]);
    });

    test('creates 2 groups with integer ranges using integer values with even distribution and even range', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 2,
        values: [1, 5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '3',
          maxRaw: 3,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '5',
          maxRaw: 5,
          min: '4',
          minRaw: 4,
        },
      ]);
    });

    test('creates 2 groups with non-integer ranges using non-integer values with even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 2,
        values: [2.2, 6.6],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 1,
          max: '4.4',
          maxRaw: 4.4,
          min: '2.2',
          minRaw: 2.2,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '6.6',
          maxRaw: 6.6,
          min: '4.5',
          minRaw: 4.5,
        },
      ]);
    });

    test('creates 2 groups with integer ranges using integer values with uneven distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 2,
        values: [1, 2, 3, 3, 8, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '3',
          maxRaw: 3,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '10',
          maxRaw: 10,
          min: '4',
          minRaw: 4,
        },
      ]);
    });

    test('creates 2 groups with non-integer ranges using non-integer values with uneven distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 2,
        values: [2.2, 3.3, 3.3, 6.6],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 1,
          max: '3.3',
          maxRaw: 3.3,
          min: '2.2',
          minRaw: 2.2,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '6.6',
          maxRaw: 6.6,
          min: '3.4',
          minRaw: 3.4,
        },
      ]);
    });

    test('creates 2 groups with non-integer ranges with 2 d.p using non-integer values with 1 d.p and a range of 0.1', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 2,
        values: [2.2, 2.3],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 2,
          max: '2.25',
          maxRaw: 2.25,
          min: '2.20',
          minRaw: 2.2,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '2.30',
          maxRaw: 2.3,
          min: '2.26',
          minRaw: 2.26,
        },
      ]);
    });

    test('creates 3 groups with integer ranges using integer values with even distribution and odd range', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 3,
        values: [1, 3, 6],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '6',
          maxRaw: 6,
          min: '5',
          minRaw: 5,
        },
      ]);
    });

    test('creates 3 groups with integer ranges using integer values with even distribution and even range', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 3,
        values: [1, 3, 5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '5',
          maxRaw: 5,
          min: '5',
          minRaw: 5,
        },
      ]);
    });

    test('creates 3 groups with non-integer ranges using non-integer values with 1 d.p and even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 3,
        values: [2.5, 5.1, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 1,
          max: '4.2',
          maxRaw: 4.2,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 1,
          max: '6.7',
          maxRaw: 6.7,
          min: '4.3',
          minRaw: 4.3,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '10.0',
          maxRaw: 10,
          min: '6.8',
          minRaw: 6.8,
        },
      ]);
    });

    test('creates 3 groups with non-integer ranges using non-integer values with 2 d.p and even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 3,
        values: [3.33, 9.99, 13.32],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 2,
          max: '7.77',
          maxRaw: 7.77,
          min: '3.33',
          minRaw: 3.33,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 2,
          max: '11.10',
          maxRaw: 11.1,
          min: '7.78',
          minRaw: 7.78,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '13.32',
          maxRaw: 13.32,
          min: '11.11',
          minRaw: 11.11,
        },
      ]);
    });

    test('creates 3 groups with integer ranges using integer values with uneven distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 3,
        values: [1, 2, 2, 3, 5, 6, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 0,
          max: '2',
          maxRaw: 2,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 0,
          max: '5',
          maxRaw: 5,
          min: '3',
          minRaw: 3,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '10',
          maxRaw: 10,
          min: '6',
          minRaw: 6,
        },
      ]);
    });

    test('creates 3 groups with non-integer ranges using non-integer values with uneven distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 3,
        values: [2.5, 2.7, 2.7, 3.2, 5.1, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(170, 170, 170, 1)',
          decimalPlaces: 1,
          max: '2.7',
          maxRaw: 2.7,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(85, 85, 85, 1)',
          decimalPlaces: 1,
          max: '3.8',
          maxRaw: 3.8,
          min: '2.8',
          minRaw: 2.8,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '10.0',
          maxRaw: 10,
          min: '3.9',
          minRaw: 3.9,
        },
      ]);
    });

    test('creates 5 groups with integer ranges using 5 integer values with even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 5,
        values: [1, 3, 5, 7, 10],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 0,
          max: '3',
          maxRaw: 3,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '4',
          minRaw: 4,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 0,
          max: '6',
          maxRaw: 6,
          min: '5',
          minRaw: 5,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 0,
          max: '8',
          maxRaw: 8,
          min: '7',
          minRaw: 7,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '10',
          maxRaw: 10,
          min: '9',
          minRaw: 9,
        },
      ]);
    });

    test('creates 5 groups with integer ranges using 3 integer values with even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 5,
        values: [5, 1, 9],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          decimalPlaces: 0,
          colour: 'rgba(204, 204, 204, 1)',
          max: '3',
          maxRaw: 3,
          min: '1',
          minRaw: 1,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 0,
          max: '4',
          maxRaw: 4,
          min: '4',
          minRaw: 4,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 0,
          max: '6',
          maxRaw: 6,
          min: '5',
          minRaw: 5,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 0,
          max: '7',
          maxRaw: 7,
          min: '7',
          minRaw: 7,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '9',
          maxRaw: 9,
          min: '8',
          minRaw: 8,
        },
      ]);
    });

    test('creates 5 groups with non-integer ranges using 3 non-integer values with 1 d.p and even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 5,
        values: [2.5, 10.4, 12.5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 1,
          max: '5.7',
          maxRaw: 5.7,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 1,
          max: '8.8',
          maxRaw: 8.8,
          min: '5.8',
          minRaw: 5.8,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 1,
          max: '10.8',
          maxRaw: 10.8,
          min: '8.9',
          minRaw: 8.9,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 1,
          max: '11.7',
          maxRaw: 11.7,
          min: '10.9',
          minRaw: 10.9,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '12.5',
          maxRaw: 12.5,
          min: '11.8',
          minRaw: 11.8,
        },
      ]);
    });

    test('creates 5 groups with non-integer ranges using 5 non-integer values with 1 d.p with even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 5,
        values: [2.5, 5.2, 7.4, 9.8, 12.5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 1,
          max: '4.7',
          maxRaw: 4.7,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 1,
          max: '6.5',
          maxRaw: 6.5,
          min: '4.8',
          minRaw: 4.8,
        },
        {
          decimalPlaces: 1,
          colour: 'rgba(102, 102, 102, 1)',
          max: '8.4',
          maxRaw: 8.4,
          min: '6.6',
          minRaw: 6.6,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 1,
          max: '10.3',
          maxRaw: 10.3,
          min: '8.5',
          minRaw: 8.5,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '12.5',
          maxRaw: 12.5,
          min: '10.4',
          minRaw: 10.4,
        },
      ]);
    });

    test('creates 5 groups with non-integer ranges using 3 non-integer values with 2 d.p and even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 5,
        values: [2.45, 10.32, 25.45],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 2,
          max: '5.60',
          maxRaw: 5.6,
          min: '2.45',
          minRaw: 2.45,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 2,
          max: '8.75',
          maxRaw: 8.75,
          min: '5.61',
          minRaw: 5.61,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 2,
          max: '13.35',
          maxRaw: 13.35,
          min: '8.76',
          minRaw: 8.76,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 2,
          max: '19.40',
          maxRaw: 19.4,
          min: '13.36',
          minRaw: 13.36,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 2,
          max: '25.45',
          maxRaw: 25.45,
          min: '19.41',
          minRaw: 19.41,
        },
      ]);
    });

    test('creates 5 groups with non-integer ranges using non-integer values with even distribution', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Quantiles',
        groups: 5,
        values: [2.5, 4.9, 5.2, 6.9, 7.4, 8.9, 9.8, 12.5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(204, 204, 204, 1)',
          decimalPlaces: 1,
          max: '5.0',
          maxRaw: 5,
          min: '2.5',
          minRaw: 2.5,
        },
        {
          colour: 'rgba(153, 153, 153, 1)',
          decimalPlaces: 1,
          max: '6.6',
          maxRaw: 6.6,
          min: '5.1',
          minRaw: 5.1,
        },
        {
          colour: 'rgba(102, 102, 102, 1)',
          decimalPlaces: 1,
          max: '7.7',
          maxRaw: 7.7,
          min: '6.7',
          minRaw: 6.7,
        },
        {
          colour: 'rgba(51, 51, 51, 1)',
          decimalPlaces: 1,
          max: '9.4',
          maxRaw: 9.4,
          min: '7.8',
          minRaw: 7.8,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '12.5',
          maxRaw: 12.5,
          min: '9.5',
          minRaw: 9.5,
        },
      ]);
    });
  });

  describe('custom', () => {
    test('returns custom groups for integer values and groups', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Custom',
        customDataGroups: [
          { min: 1, max: 45 },
          {
            min: 46,
            max: 100,
          },
        ],
        groups: 5,
        values: [1, 24, 35, 45, 111],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '45',
          maxRaw: 45,
          min: '1',
          minRaw: 1,
        },
        {
          decimalPlaces: 0,
          colour: 'rgba(0, 0, 0, 1)',
          max: '100',
          maxRaw: 100,
          min: '46',
          minRaw: 46,
        },
      ]);
    });

    test('returns groups for integer custom groups and non-integer values', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Custom',
        customDataGroups: [
          { min: 0, max: 5 },
          {
            min: 6,
            max: 10,
          },
        ],
        groups: 5,
        values: [2.5, 4.9, 5.2, 6.9, 7.4, 8.9, 9.8, 12.5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 0,
          max: '5',
          maxRaw: 5.4,
          min: '0',
          minRaw: -0.5,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 0,
          max: '10',
          maxRaw: 10.4,
          min: '6',
          minRaw: 5.5,
        },
      ]);
    });

    test('returns groups for non-integer custom groups and non-integer values with the same number of decimal places', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Custom',
        customDataGroups: [
          { min: 2.2, max: 5.5 },
          {
            min: 5.6,
            max: 7.3,
          },
        ],
        groups: 5,
        values: [2.5, 4.9, 5.2, 6.9, 7.4, 8.9, 9.8, 12.5],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 1,
          max: '5.5',
          maxRaw: 5.5,
          min: '2.2',
          minRaw: 2.2,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '7.3',
          maxRaw: 7.3,
          min: '5.6',
          minRaw: 5.6,
        },
      ]);
    });

    test('returns groups for non-integer custom groups and non-integer values with a different number of decimal places', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Custom',
        customDataGroups: [
          { min: 2, max: 5.5 },
          {
            min: 5.6,
            max: 7.3,
          },
        ],
        groups: 5,
        values: [1.89, 2.55, 4.91, 5.52, 6.93, 7.28, 8.39, 9.83, 12.53],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 1,
          max: '5.5',
          maxRaw: 5.54,
          min: '2',
          minRaw: 1.95,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 1,
          max: '7.3',
          maxRaw: 7.34,
          min: '5.6',
          minRaw: 5.55,
        },
      ]);
    });

    test('handles more decimal places for non-integer custom groups and non-integer values with a different number of decimal places', () => {
      const dataClasses = generateLegendDataGroups({
        colour: 'rgba(0, 0, 0, 1)',
        classification: 'Custom',
        customDataGroups: [
          { min: 1.123, max: 4.336 },
          {
            min: 6.988,
            max: 10.569,
          },
        ],
        decimalPlaces: 3,
        groups: 5,
        values: [1.12345, 4.33642, 6.98765, 10.56935],
      });

      expect(dataClasses).toEqual<LegendDataGroup[]>([
        {
          colour: 'rgba(128, 128, 128, 1)',
          decimalPlaces: 3,
          max: '4.336',
          maxRaw: 4.3364,
          min: '1.123',
          minRaw: 1.1225,
        },
        {
          colour: 'rgba(0, 0, 0, 1)',
          decimalPlaces: 3,
          max: '10.569',
          maxRaw: 10.5694,
          min: '6.988',
          minRaw: 6.9875,
        },
      ]);
    });
  });
});
