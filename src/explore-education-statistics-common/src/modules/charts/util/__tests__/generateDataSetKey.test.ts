import { DataSet } from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';

describe('generateDataSetKey', () => {
  const testDataSet: DataSet = {
    indicator: 'indicator-1',
    filters: ['filter-1', 'filter-2'],
    timePeriod: '2020_AY',
    location: {
      level: 'country',
      value: 'location-1',
    },
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
    const key = generateDataSetKey(
      testDataSet,
      new CategoryFilter({
        value: 'filter-1',
        label: 'Filter 1',
        category: 'Filters',
      }),
    );

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-2'],
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
    const key = generateDataSetKey(
      testDataSet,
      new LocationFilter({
        id: 'location-1-id',
        value: 'location-1',
        level: 'country',
        label: 'Location 1',
      }),
    );

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-1', 'filter-2'],
        indicator: 'indicator-1',
        timePeriod: '2020_AY',
      }),
    );
  });

  test('providing a `groupBy` param of `timePeriod` excludes the time period from the key', () => {
    const key = generateDataSetKey(
      testDataSet,
      new TimePeriodFilter({
        order: 0,
        code: 'AY',
        year: 2020,
        label: '2020/21',
      }),
    );

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

  test('providing a `groupBy` param of `indicators` excludes the indicator from the key', () => {
    const key = generateDataSetKey(
      testDataSet,
      new Indicator({
        value: 'indicator-1',
        label: 'Indicator 1',
        unit: '',
        name: 'indicator_1',
      }),
    );

    expect(key).toBe(
      JSON.stringify({
        filters: ['filter-1', 'filter-2'],
        location: {
          level: 'country',
          value: 'location-1',
        },
        timePeriod: '2020_AY',
      }),
    );
  });
});
