import { DataSet } from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';

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

  test('providing a `groupBy` param of `filters` does not exclude the filters from the key', () => {
    const key = generateDataSetKey(testDataSet, 'filters');

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
