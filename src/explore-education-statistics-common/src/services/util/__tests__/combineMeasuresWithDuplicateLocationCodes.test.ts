import combineMeasuresWithDuplicateLocationCodes, {
  MeasurementsMergeStrategy,
} from '@common/services/util/combineMeasuresWithDuplicateLocationCodes';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { TableDataResult } from '@common/services/tableBuilderService';

describe('combineMeasuresWithDuplicateLocationCodes', () => {
  /**
   * This is a strategy for merging measurement values from several duplicate Locations into a single value.
   * This strategy produces a string with each Location's value for this measurement separated with forward slashes.
   *
   * This is useful to make clear in these tests the effects of the measurement merging process over and above the
   * default summing behaviour.
   *
   * ${@param measurementValues} represents the multiple values from each Location for this measurement.
   */
  const slashSeparatedStringMergeStrategy: MeasurementsMergeStrategy = (
    measurementValues: (string | undefined)[],
  ) => {
    return measurementValues
      .map(value => (typeof value === 'undefined' ? '0' : value))
      .join(' / ');
  };

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

    const deduplicatedLocations: LocationFilter[] = [];

    const results = combineMeasuresWithDuplicateLocationCodes(
      tableDataResult,
      deduplicatedLocations,
      slashSeparatedStringMergeStrategy,
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

    const availableLocations: LocationFilter[] = [];

    const results = combineMeasuresWithDuplicateLocationCodes(
      tableDataResult,
      availableLocations,
      slashSeparatedStringMergeStrategy,
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

    const deduplicatedLocations: LocationFilter[] = [
      new LocationFilter({
        value: 'duplicate-provider-code',
        label: 'Provider 1 / Provider 2',
        level: 'provider',
      }),
    ];

    const expectedMergedResults = [
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
    ];

    const results = combineMeasuresWithDuplicateLocationCodes(
      tableDataResult,
      deduplicatedLocations,
      slashSeparatedStringMergeStrategy,
    );
    expect(results).toEqual(expectedMergedResults);
  });

  test(
    'results from locations with non-overlapping measures are combined with "0" values for ' +
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

      const deduplicatedLocations: LocationFilter[] = [
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
            'indicator-1': '10 / 40 / 0',
            'indicator-2': '20 / 50 / 0',
            'indicator-3': '30 / 60 / 70',
            'indicator-4': '0 / 0 / 80',
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
        deduplicatedLocations,
        slashSeparatedStringMergeStrategy,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );

  test(
    'results from locations with non-overlapping time periods are combined with "0" values for ' +
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

      const deduplicatedLocations: LocationFilter[] = [
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
            'indicator-1': '10 / 0',
            'indicator-2': '20 / 0',
            'indicator-3': '30 / 0',
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
            'indicator-1': '0 / 40',
            'indicator-2': '0 / 50',
            'indicator-3': '0 / 60',
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
        deduplicatedLocations,
        slashSeparatedStringMergeStrategy,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );

  test(
    'results from locations with non-overlapping filters are combined with the text "0" for the location that ' +
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

      const deduplicatedLocations: LocationFilter[] = [
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
            'indicator-1': '10 / 0',
            'indicator-2': '20 / 0',
            'indicator-3': '30 / 0',
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
            'indicator-1': '0 / 40',
            'indicator-2': '0 / 50',
            'indicator-3': '0 / 60',
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
        deduplicatedLocations,
        slashSeparatedStringMergeStrategy,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );

  test(
    'default sumNumericValuesMergeStrategy measurement-combining strategy combines numeric values rather than ' +
      'separating with forward slashes.  If no numeric values exist, the first non-falsy value is displayed.',
    () => {
      const tableDataResult: TableDataResult[] = [
        {
          filters: ['filter-1', 'filter-2'],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '10',
            'indicator-2': '20',
            'indicator-3': 'Some text to be ignored',
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
            'indicator-5': '~',
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

      const deduplicatedLocations: LocationFilter[] = [
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
            'indicator-1': '50',
            'indicator-2': '70',
            'indicator-3': '130',
            'indicator-4': '80',
            'indicator-5': '~',
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
        deduplicatedLocations,
      );
      expect(results).toEqual(expectedMergedResults);
    },
  );
});
