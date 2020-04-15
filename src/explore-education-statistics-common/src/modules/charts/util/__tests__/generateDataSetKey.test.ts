import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';

describe('generateDataSetKey', () => {
  const testDataSet: ExpandedDataSet = {
    indicator: new Indicator({
      label: 'Indicator 1',
      value: 'indicator-1',
      unit: '',
    }),
    filters: [
      new CategoryFilter({
        value: 'filter-1',
        label: 'Filter 1',
        category: 'Category A',
      }),
      new CategoryFilter({
        value: 'filter-2',
        label: 'Filter 2',
        category: 'Category A',
      }),
    ],
    timePeriod: new TimePeriodFilter({
      year: 2020,
      label: '2020/21',
      code: 'AY',
    }),
    location: new LocationFilter({
      label: 'Location 1',
      level: 'country',
      value: 'location-1',
    }),
  };

  test('returns a stringified version of the data set for the key', () => {
    const key = generateDataSetKey(testDataSet);

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-1', 'filter-2'],
        indicator: 'indicator-1',
        location: {
          level: 'country',
          value: 'location-1',
        },
        timePeriod: '2020_AY',
      }),
    );
  });

  test('providing a `groupBy` param of `filters` excludes the filters from the key', () => {
    const key = generateDataSetKey(testDataSet, 'filters');

    expect(key).toBe(
      JSON.stringify({
        filters: [],
        indicator: 'indicator-1',
        location: {
          level: 'country',
          value: 'location-1',
        },
        timePeriod: '2020_AY',
      }),
    );
  });

  test('providing a `groupBy` param of `locations` excludes the location from the key', () => {
    const key = generateDataSetKey(testDataSet, 'locations');

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-1', 'filter-2'],
        indicator: 'indicator-1',
        timePeriod: '2020_AY',
      }),
    );
  });

  test('providing a `groupBy` param of `timePeriod` excdlues the time period from the key', () => {
    const key = generateDataSetKey(testDataSet, 'timePeriod');

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-1', 'filter-2'],
        indicator: 'indicator-1',
        location: {
          level: 'country',
          value: 'location-1',
        },
      }),
    );
  });

  test('providing a `groupBy` param of `indicators` does not exclude it from the key', () => {
    const key = generateDataSetKey(testDataSet, 'indicators');

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-1', 'filter-2'],
        indicator: 'indicator-1',
        location: {
          level: 'country',
          value: 'location-1',
        },
        timePeriod: '2020_AY',
      }),
    );
  });
});
