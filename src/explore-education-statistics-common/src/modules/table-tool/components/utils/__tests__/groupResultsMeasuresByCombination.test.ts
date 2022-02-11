import groupResultMeasuresByCombination from '@common/modules/table-tool/components/utils/groupResultMeasuresByCombination';
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
    const excludedFilterIds = [
      LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      }),
    ];

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

  test('can exclude grouping by time period', () => {
    const excludedFilterIds = ['2018_AY'];

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
    const excludedFilterIds = ['filter-2'];

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
    const excludedFilterIds = ['filter-1', 'filter-2'];

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
    const excludedFilterIds = [
      LocationFilter.createId({
        level: 'localAuthority',
        value: 'location-1',
      }),
      '2018_AY',
      'filter-1',
    ];

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
