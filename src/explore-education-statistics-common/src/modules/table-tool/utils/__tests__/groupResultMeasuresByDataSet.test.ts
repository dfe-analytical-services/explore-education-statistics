import groupResultMeasuresByDataSet from '@common/modules/table-tool/utils/groupResultMeasuresByDataSet';
import { LocationFilter } from '@common/modules/table-tool/types/filters';

describe('groupResultMeasuresByDataSet', () => {
  test('creates different groups based on location', () => {
    const resultMeasures = groupResultMeasuresByDataSet([
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2018_AY',
        measures: {
          'indicator-1': '1000',
          'indicator-2': '2000',
        },
        filters: ['filter-1', 'filter-2'],
      },
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-2',
        timePeriod: '2018_AY',
        measures: {
          'indicator-1': '3000',
          'indicator-2': '4000',
        },
        filters: ['filter-1', 'filter-2'],
      },
    ]);

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        '2018_AY': {
          'filter-1': {
            'filter-2': {
              'indicator-1': '1000',
              'indicator-2': '2000',
            },
          },
        },
      },
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-2',
      })]: {
        '2018_AY': {
          'filter-1': {
            'filter-2': {
              'indicator-1': '3000',
              'indicator-2': '4000',
            },
          },
        },
      },
    });
  });

  test('creates different groups based on time period', () => {
    const resultMeasures = groupResultMeasuresByDataSet([
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2018_AY',
        measures: {
          'indicator-1': '1000',
          'indicator-2': '2000',
        },
        filters: ['filter-1', 'filter-2'],
      },
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2019_AY',
        measures: {
          'indicator-1': '3000',
          'indicator-2': '4000',
        },
        filters: ['filter-1', 'filter-2'],
      },
    ]);

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        '2018_AY': {
          'filter-1': {
            'filter-2': {
              'indicator-1': '1000',
              'indicator-2': '2000',
            },
          },
        },
        '2019_AY': {
          'filter-1': {
            'filter-2': {
              'indicator-1': '3000',
              'indicator-2': '4000',
            },
          },
        },
      },
    });
  });

  test('creates different groups based on filters', () => {
    const resultMeasures = groupResultMeasuresByDataSet([
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2018_AY',
        measures: {
          'indicator-1': '1000',
          'indicator-2': '2000',
        },
        filters: ['filter-1', 'filter-3'],
      },
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2018_AY',
        measures: {
          'indicator-1': '3000',
          'indicator-2': '4000',
        },
        filters: ['filter-1', 'filter-2'],
      },
    ]);

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        '2018_AY': {
          'filter-1': {
            'filter-2': {
              'indicator-1': '3000',
              'indicator-2': '4000',
            },
            'filter-3': {
              'indicator-1': '1000',
              'indicator-2': '2000',
            },
          },
        },
      },
    });
  });

  test('merges groups with same data set', () => {
    const resultMeasures = groupResultMeasuresByDataSet([
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2018_AY',
        measures: {
          'indicator-1': '1000',
          'indicator-2': '2000',
        },
        filters: ['filter-1', 'filter-2'],
      },
      {
        geographicLevel: 'localAuthority',
        locationId: 'location-1',
        timePeriod: '2018_AY',
        measures: {
          'indicator-3': '3000',
          'indicator-4': '4000',
        },
        filters: ['filter-1', 'filter-2'],
      },
    ]);

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        '2018_AY': {
          'filter-1': {
            'filter-2': {
              'indicator-1': '1000',
              'indicator-2': '2000',
              'indicator-3': '3000',
              'indicator-4': '4000',
            },
          },
        },
      },
    });
  });
});
