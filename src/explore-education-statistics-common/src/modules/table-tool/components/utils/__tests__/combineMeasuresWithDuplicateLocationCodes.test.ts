import combineMeasuresWithDuplicateLocationCodes from '@common/modules/table-tool/components/utils/combineMeasuresWithDuplicateLocationCodes';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { TableDataResult } from '@common/services/tableBuilderService';

describe('combineMeasuresWithDuplicateLocationCodes', () => {
  test('does not affect results that do not contain locations with duplicate levels and codes', () => {
    const tableDataResult: TableDataResult[] = [
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'provider',
        timePeriod: '',
        measures: {
          'indicator-1': '10',
          'indicator-2': '20',
          'indicator-3': '30',
        },
        location: {
          provider: {
            code: 'provider-1',
            name: 'Provider 1',
          },
        },
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'provider',
        timePeriod: '',
        measures: {
          'indicator-1': '40',
          'indicator-2': '50',
          'indicator-3': '60',
        },
        location: {
          provider: {
            code: 'provider-2',
            name: 'Provider 2',
          },
        },
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'localAuthority',
        timePeriod: '',
        measures: {
          'indicator-1': '70',
          'indicator-2': '80',
          'indicator-3': '90',
        },
        location: {
          localAuthority: {
            code: 'localAuthority-1',
            name: 'LocalAuthority 1',
          },
        },
      },
    ];

    const availableLocations: LocationFilter[] = [
      new LocationFilter({
        value: 'provider-1',
        label: 'Provider 1',
        level: 'provider',
      }),
      new LocationFilter({
        value: 'provider-2',
        label: 'Provider 2',
        level: 'provider',
      }),
      new LocationFilter({
        value: 'localAuthority-1',
        label: 'LocalAuthority 1',
        level: 'localAuthority',
      }),
    ];

    const results = combineMeasuresWithDuplicateLocationCodes(
      tableDataResult,
      availableLocations,
    );
    expect(results).toEqual(tableDataResult);
  });

  test('does not affect results if 2 locations have the same code but are of different provider levels', () => {
    const tableDataResult: TableDataResult[] = [
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'provider',
        timePeriod: '',
        measures: {
          'indicator-1': '10',
          'indicator-2': '20',
          'indicator-3': '30',
        },
        location: {
          provider: {
            code: 'duplicate-location-code',
            name: 'Provider 1',
          },
        },
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'localAuthority',
        timePeriod: '',
        measures: {
          'indicator-1': '40',
          'indicator-2': '50',
          'indicator-3': '60',
        },
        location: {
          localAuthority: {
            code: 'duplicate-location-code',
            name: 'LocalAuthority 1',
          },
        },
      },
    ];

    const availableLocations: LocationFilter[] = [
      new LocationFilter({
        value: 'duplicate-location-code',
        label: 'Provider 1',
        level: 'provider',
      }),
      new LocationFilter({
        value: 'duplicate-location-code',
        label: 'LocalAuthority 1',
        level: 'localAuthority',
      }),
    ];

    const results = combineMeasuresWithDuplicateLocationCodes(
      tableDataResult,
      availableLocations,
    );
    expect(results).toEqual(tableDataResult);
  });

  test('results for locations with duplicate codes and levels are combined', () => {
    const tableDataResult: TableDataResult[] = [
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'provider',
        timePeriod: '',
        measures: {
          'indicator-1': '10',
          'indicator-2': '20',
          'indicator-3': '30',
        },
        location: {
          provider: {
            code: 'duplicate-provider-code',
            name: 'Provider 1',
          },
        },
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'provider',
        timePeriod: '',
        measures: {
          'indicator-1': '40',
          'indicator-2': '50',
          'indicator-3': '60',
        },
        location: {
          provider: {
            code: 'duplicate-provider-code',
            name: 'Provider 2',
          },
        },
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'localAuthority',
        timePeriod: '',
        measures: {
          'indicator-1': '70',
          'indicator-2': '80',
          'indicator-3': '90',
        },
        location: {
          localAuthority: {
            code: 'localAuthority-1',
            name: 'LocalAuthority 1',
          },
        },
      },
    ];

    const availableLocations: LocationFilter[] = [
      new LocationFilter({
        value: 'duplicate-provider-code',
        label: 'Provider 1 / Provider 2',
        level: 'provider',
      }),
      new LocationFilter({
        value: 'localAuthority-1',
        label: 'LocalAuthority 1',
        level: 'localAuthority',
      }),
    ];

    const expectedMergedResults = [
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'provider',
        timePeriod: '',
        measures: {
          'indicator-1': '10 / 40',
          'indicator-2': '20 / 50',
          'indicator-3': '30 / 60',
        },
        location: {
          provider: {
            code: 'duplicate-provider-code',
            name: 'Provider 1 / Provider 2',
          },
        },
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'localAuthority',
        timePeriod: '',
        measures: {
          'indicator-1': '70',
          'indicator-2': '80',
          'indicator-3': '90',
        },
        location: {
          localAuthority: {
            code: 'localAuthority-1',
            name: 'LocalAuthority 1',
          },
        },
      },
    ];

    const results = combineMeasuresWithDuplicateLocationCodes(
      tableDataResult,
      availableLocations,
    );
    expect(results).toEqual(expectedMergedResults);
  });

  test(
    'results from locations with non-overlapping measures are combined with n/a values for ' +
      'Locations that do not have data for particular measure',
    () => {
      const tableDataResult: TableDataResult[] = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '10',
            'indicator-2': '20',
            'indicator-3': '30',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1',
            },
          },
        },
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-3': '70',
            'indicator-4': '80',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 3',
            },
          },
        },
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '40',
            'indicator-3': '60',
            'indicator-2': '50',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 2',
            },
          },
        },
      ];

      const availableLocations: LocationFilter[] = [
        new LocationFilter({
          value: 'duplicate-provider-code',
          label: 'Provider 1 / Provider 2 / Provider 3',
          level: 'provider',
        }),
      ];

      const expectedMergedResults = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '10 / 40 / n/a',
            'indicator-2': '20 / 50 / n/a',
            'indicator-3': '30 / 60 / 70',
            'indicator-4': 'n/a / n/a / 80',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1 / Provider 2 / Provider 3',
            },
          },
        },
      ];

      const results = combineMeasuresWithDuplicateLocationCodes(
        tableDataResult,
        availableLocations,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );

  test(
    'results from locations with non-overlapping time periods are combined with n/a values for ' +
      'Locations that do not have data for particular time periods',
    () => {
      const tableDataResult: TableDataResult[] = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-1',
          measures: {
            'indicator-1': '10',
            'indicator-2': '20',
            'indicator-3': '30',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1',
            },
          },
        },
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-2',
          measures: {
            'indicator-1': '40',
            'indicator-2': '50',
            'indicator-3': '60',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 2',
            },
          },
        },
      ];

      const availableLocations: LocationFilter[] = [
        new LocationFilter({
          value: 'duplicate-provider-code',
          label: 'Provider 1 / Provider 2',
          level: 'provider',
        }),
      ];

      const expectedMergedResults = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-1',
          measures: {
            'indicator-1': '10 / n/a',
            'indicator-2': '20 / n/a',
            'indicator-3': '30 / n/a',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1 / Provider 2',
            },
          },
        },
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-2',
          measures: {
            'indicator-1': 'n/a / 40',
            'indicator-2': 'n/a / 50',
            'indicator-3': 'n/a / 60',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1 / Provider 2',
            },
          },
        },
      ];

      const results = combineMeasuresWithDuplicateLocationCodes(
        tableDataResult,
        availableLocations,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );

  test(
    'results from locations with non-overlapping filters are combined with n/a text for the location that ' +
      'does not have values for that filter combinations',
    () => {
      const tableDataResult: TableDataResult[] = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-1',
          measures: {
            'indicator-1': '10',
            'indicator-2': '20',
            'indicator-3': '30',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1',
            },
          },
        },
        {
          filters: ['filter-2', 'filter-3'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-2',
          measures: {
            'indicator-1': '40',
            'indicator-2': '50',
            'indicator-3': '60',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 2',
            },
          },
        },
      ];

      const availableLocations: LocationFilter[] = [
        new LocationFilter({
          value: 'duplicate-provider-code',
          label: 'Provider 1 / Provider 2',
          level: 'provider',
        }),
      ];

      const expectedMergedResults = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-1',
          measures: {
            'indicator-1': '10 / n/a',
            'indicator-2': '20 / n/a',
            'indicator-3': '30 / n/a',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1 / Provider 2',
            },
          },
        },
        {
          filters: ['filter-2', 'filter-3'],
          geographicLevel: 'provider',
          timePeriod: 'time-period-2',
          measures: {
            'indicator-1': 'n/a / 40',
            'indicator-2': 'n/a / 50',
            'indicator-3': 'n/a / 60',
          },
          location: {
            provider: {
              code: 'duplicate-provider-code',
              name: 'Provider 1 / Provider 2',
            },
          },
        },
      ];

      const results = combineMeasuresWithDuplicateLocationCodes(
        tableDataResult,
        availableLocations,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );
});
