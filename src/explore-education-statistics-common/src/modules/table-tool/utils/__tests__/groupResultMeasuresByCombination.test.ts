import groupResultMeasuresByCombination from '@common/modules/table-tool/utils/groupResultMeasuresByCombination';
import { LocationFilter } from '@common/modules/table-tool/types/filters';

describe('groupResultMeasuresByCombination', () => {
  test('creates different groups based on location', () => {
    const resultMeasures = groupResultMeasuresByCombination([
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

  // Test for Permalinks created prior to the switchover to using location id's.
  test('creates different groups based on location codes', () => {
    const resultMeasures = groupResultMeasuresByCombination(
      // Results contain a 'location' object with codes rather than a 'locationId'
      [
        {
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: {
              code: 'location-1',
              name: 'LA 1',
            },
          },
          timePeriod: '2018_AY',
          measures: {
            'indicator-1': '1000',
            'indicator-2': '2000',
          },
          filters: ['filter-1', 'filter-2'],
        },
        {
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: {
              code: 'location-2',
              name: 'LA 2',
            },
          },
          timePeriod: '2018_AY',
          measures: {
            'indicator-1': '3000',
            'indicator-2': '4000',
          },
          filters: ['filter-1', 'filter-2'],
        },
      ],
    );

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
    const resultMeasures = groupResultMeasuresByCombination([
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
    const resultMeasures = groupResultMeasuresByCombination([
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
    const resultMeasures = groupResultMeasuresByCombination([
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

  test('can exclude grouping by location', () => {
    const excludedFilterIds = new Set([
      LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      }),
    ]);

    const resultMeasures = groupResultMeasuresByCombination(
      [
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
      ],
      excludedFilterIds,
    );

    expect(resultMeasures).toEqual({
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
    });
  });

  // Test for Permalinks created prior to the switchover to using location id's.
  test('can exclude grouping by location code', () => {
    const excludedFilterIds = new Set([
      LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      }),
    ]);

    const resultMeasures = groupResultMeasuresByCombination(
      // Results contain a 'location' object with codes rather than a 'locationId'
      [
        {
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: {
              code: 'location-1',
              name: 'LA 1',
            },
          },
          timePeriod: '2018_AY',
          measures: {
            'indicator-1': '1000',
            'indicator-2': '2000',
          },
          filters: ['filter-1', 'filter-2'],
        },
        {
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: {
              code: 'location-1',
              name: 'LA 1',
            },
          },
          timePeriod: '2018_AY',
          measures: {
            'indicator-3': '3000',
            'indicator-4': '4000',
          },
          filters: ['filter-1', 'filter-2'],
        },
      ],
      excludedFilterIds,
    );

    expect(resultMeasures).toEqual({
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
    });
  });

  test('can exclude grouping by time period', () => {
    const excludedFilterIds = new Set(['2018_AY']);

    const resultMeasures = groupResultMeasuresByCombination(
      [
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
      ],
      excludedFilterIds,
    );

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        'filter-1': {
          'filter-2': {
            'indicator-1': '1000',
            'indicator-2': '2000',
            'indicator-3': '3000',
            'indicator-4': '4000',
          },
        },
      },
    });
  });

  test('can exclude grouping by single filter', () => {
    const excludedFilterIds = new Set(['filter-2']);

    const resultMeasures = groupResultMeasuresByCombination(
      [
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
      ],
      excludedFilterIds,
    );

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        '2018_AY': {
          'filter-1': {
            'indicator-1': '1000',
            'indicator-2': '2000',
            'indicator-3': '3000',
            'indicator-4': '4000',
          },
        },
      },
    });
  });

  test('can exclude grouping by multiple filters', () => {
    const excludedFilterIds = new Set(['filter-1', 'filter-2']);

    const resultMeasures = groupResultMeasuresByCombination(
      [
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
      ],
      excludedFilterIds,
    );

    expect(resultMeasures).toEqual({
      [LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      })]: {
        '2018_AY': {
          'indicator-1': '1000',
          'indicator-2': '2000',
          'indicator-3': '3000',
          'indicator-4': '4000',
        },
      },
    });
  });

  test('can exclude grouping of different filter types', () => {
    const excludedFilterIds = new Set([
      LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      }),
      '2018_AY',
      'filter-1',
    ]);

    const resultMeasures = groupResultMeasuresByCombination(
      [
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
      ],
      excludedFilterIds,
    );

    expect(resultMeasures).toEqual({
      'filter-2': {
        'indicator-1': '1000',
        'indicator-2': '2000',
        'indicator-3': '3000',
        'indicator-4': '4000',
      },
    });
  });
});
