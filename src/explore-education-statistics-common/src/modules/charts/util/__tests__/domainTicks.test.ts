import { AxisConfiguration } from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import {
  calculateMinorAxisDomainValues,
  getMajorAxisDomainTicks,
  getMinorAxisDomainTicks,
} from '@common/modules/charts/util/domainTicks';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';

describe('domainTicks', () => {
  const testKey = generateDataSetKey({
    indicator: 'indicator-1',
    filters: ['filter-1'],
    location: {
      level: 'country',
      value: 'location-1',
    },
  });

  const testChartData: ChartData[] = [
    {
      [testKey]: 50,
      name: '2014_T1',
    },
    {
      [testKey]: 32,
      name: '2015_T1',
    },
    {
      [testKey]: 28,
      name: '2016_T1',
    },
    {
      [testKey]: 48,
      name: '2017_T1',
    },
    {
      [testKey]: 54,
      name: '2018_T1',
    },
  ];

  const testAxisConfig: AxisConfiguration = {
    dataSets: [],
    referenceLines: [],
    visible: true,
    type: 'minor',
  };

  describe('calculateMinorAxisDomainValues', () => {
    describe('min', () => {
      test('returns the axis min when set', () => {
        const result = calculateMinorAxisDomainValues(testChartData, {
          ...testAxisConfig,
          min: 1.5,
        });

        expect(result.min).toBe(1.5);
      });

      test('returns 0 for the min when it is positive', () => {
        const result = calculateMinorAxisDomainValues(
          testChartData,
          testAxisConfig,
        );

        expect(result.min).toBe(0);
      });

      test('returns the correct min for a negative value that is odd when rounded down to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            ...testChartData,
            {
              [testKey]: -24.654,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.min).toBe(-40);
      });

      test('returns the correct min for a negative value that is even when rounded down to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            ...testChartData,
            {
              [testKey]: -51300,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.min).toBe(-60000);
      });

      test('returns the correct min for a negative value that starts with 5 when rounded down to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            ...testChartData,
            {
              [testKey]: -4500,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.min).toBe(-5000);
      });

      test('returns the correct min for a negative value between 0 and -1', () => {
        const result = calculateMinorAxisDomainValues(
          [
            ...testChartData,
            {
              [testKey]: -0.51,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.min).toBe(-0.6);
      });
    });

    describe('max', () => {
      test('returns the axis max when set', () => {
        const result = calculateMinorAxisDomainValues(testChartData, {
          ...testAxisConfig,
          max: 6,
        });

        expect(result.max).toBe(6);
      });

      test('returns 0 when max is 0', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: 0,
              name: '2019_T1',
            },
            {
              [testKey]: -3,
              name: '2020_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(0);
      });

      test('returns the correct max for a value that is odd when rounded up to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: 61000,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(80000);
      });

      test('returns the correct max for a value that is even when rounded up to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: 80000,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(80000);
      });

      test('returns the correct max for a value that starts with 5 when rounded up to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: 45000,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(50000);
      });

      test('returns the correct max for a value that is between 0 and 1', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: 0.56,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(0.6);
      });

      test('returns the correct max for a value that is between 0 and -1', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: -0.45,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(-0.4);
      });

      test('returns the correct max for a negative value that is even when rounded up to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: -271.5,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(-200);
      });

      test('returns the correct max for a negative value that is odd when rounded up to one significant figure', () => {
        const result = calculateMinorAxisDomainValues(
          [
            {
              [testKey]: -351.5,
              name: '2019_T1',
            },
          ],
          testAxisConfig,
        );

        expect(result.max).toBe(-200);
      });
    });
  });

  describe('getMinorAxisDomainTicks', () => {
    test('returns the correct axis domain and ticks when tickConfig is undefined', () => {
      const result = getMinorAxisDomainTicks(testChartData, testAxisConfig);
      expect(result.domain).toEqual([0, 60]);
      expect(result.ticks).toBeUndefined();
    });

    test('returns the correct axis domain and ticks when tickConfig is `default`', () => {
      const result = getMinorAxisDomainTicks(testChartData, {
        ...testAxisConfig,
        tickConfig: 'default',
      });
      expect(result.domain).toEqual([0, 60]);
      expect(result.ticks).toBeUndefined();
    });

    test('returns the correct axis domain and ticks when tickConfig is `startEnd`', () => {
      const result = getMinorAxisDomainTicks(testChartData, {
        ...testAxisConfig,
        tickConfig: 'startEnd',
      });
      expect(result.domain).toEqual([0, 60]);
      expect(result.ticks).toEqual([0, 60]);
    });

    describe('custom tick spacing', () => {
      test('returns the correct axis domain and ticks when tickConfig is `custom` and `tickSpacing is 5', () => {
        const result = getMinorAxisDomainTicks(testChartData, {
          ...testAxisConfig,
          tickConfig: 'custom',
          tickSpacing: 5,
        });
        expect(result.domain).toEqual([0, 60]);
        expect(result.ticks).toEqual([
          0,
          5,
          10,
          15,
          20,
          25,
          30,
          35,
          40,
          45,
          50,
          55,
          60,
        ]);
      });

      test('returns the correct axis domain and ticks when tickConfig is `custom` and `tickSpacing is 7', () => {
        const result = getMinorAxisDomainTicks(testChartData, {
          ...testAxisConfig,
          tickConfig: 'custom',
          tickSpacing: 7,
        });
        expect(result.domain).toEqual([0, 60]);
        expect(result.ticks).toEqual([0, 7, 14, 21, 28, 35, 42, 49, 56, 60]);
      });

      test('returns the correct axis domain and ticks when tickConfig is `custom` and `tickSpacing is 10', () => {
        const result = getMinorAxisDomainTicks(testChartData, {
          ...testAxisConfig,
          tickConfig: 'custom',
          tickSpacing: 10,
        });
        expect(result.domain).toEqual([0, 60]);
        expect(result.ticks).toEqual([0, 10, 20, 30, 40, 50, 60]);
      });
    });
  });

  describe('getMajorAxisDomainTicks', () => {
    test('returns the correct axis domain and ticks when tickConfig is undefined', () => {
      const result = getMajorAxisDomainTicks(testChartData, testAxisConfig);
      expect(result.domain).toEqual([0, 4]);
      expect(result.ticks).toBeUndefined();
    });

    test('returns the correct axis domain and ticks when tickConfig is `default`', () => {
      const result = getMajorAxisDomainTicks(testChartData, {
        ...testAxisConfig,
        tickConfig: 'default',
      });
      expect(result.domain).toEqual([0, 4]);
      expect(result.ticks).toBeUndefined();
    });

    test('returns the correct axis domain and ticks when tickConfig is `startEnd`', () => {
      const result = getMajorAxisDomainTicks(testChartData, {
        ...testAxisConfig,
        tickConfig: 'startEnd',
      });
      expect(result.domain).toEqual([0, 4]);
      expect(result.ticks).toEqual(['2014_T1', '2018_T1']);
    });

    describe('custom tick spacing', () => {
      const extendedTestChartData: ChartData[] = [
        ...testChartData,
        {
          [testKey]: 54,
          name: '2019_T1',
        },
        {
          [testKey]: 54,
          name: '2020_T1',
        },
        {
          [testKey]: 54,
          name: '2021_T1',
        },
        {
          [testKey]: 54,
          name: '2022_T1',
        },
      ];
      test('returns the correct axis domain and ticks when tickConfig is `custom` and `tickSpacing is 1', () => {
        const result = getMajorAxisDomainTicks(extendedTestChartData, {
          ...testAxisConfig,
          tickConfig: 'custom',
          tickSpacing: 1,
        });
        expect(result.domain).toEqual([0, 8]);
        expect(result.ticks).toEqual([
          '2014_T1',
          '2015_T1',
          '2016_T1',
          '2017_T1',
          '2018_T1',
          '2019_T1',
          '2020_T1',
          '2021_T1',
          '2022_T1',
        ]);
      });

      test('returns the correct axis domain and ticks when tickConfig is `custom` and `tickSpacing is 2', () => {
        const result = getMajorAxisDomainTicks(extendedTestChartData, {
          ...testAxisConfig,
          tickConfig: 'custom',
          tickSpacing: 2,
        });
        expect(result.domain).toEqual([0, 8]);
        expect(result.ticks).toEqual([
          '2014_T1',
          '2016_T1',
          '2018_T1',
          '2020_T1',
          '2022_T1',
        ]);
      });

      test('returns the correct axis domain and ticks when tickConfig is `custom` and `tickSpacing is 5', () => {
        const result = getMajorAxisDomainTicks(extendedTestChartData, {
          ...testAxisConfig,
          tickConfig: 'custom',
          tickSpacing: 5,
        });
        expect(result.domain).toEqual([0, 8]);
        expect(result.ticks).toEqual(['2014_T1', '2019_T1', '2022_T1']);
      });
    });
  });
});
