import getSelectedDataSets from '@admin/pages/release/datablocks/components/chart/utils/getSelectedDataSets';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';

describe('getSelectedDataSets', () => {
  const testOneFilterMeta: FullTableMeta['filters'] = {
    'School type': {
      name: 'School type',
      options: [
        {
          value: 'secondary-id',
          label: 'Secondary',
        } as CategoryFilter,
        {
          value: 'primary-id',
          label: 'Primary',
        } as CategoryFilter,
        {
          value: 'special-id',
          label: 'Special',
        } as CategoryFilter,
      ],
      order: 0,
    },
  };

  const testTwoFilterMeta: FullTableMeta['filters'] = {
    ...testOneFilterMeta,
    'Another category1': {
      name: 'Another category1',
      options: [
        {
          value: 'another-category1-option1-id',
          label: 'another-category1-option1',
        } as CategoryFilter,
        {
          value: 'another-category1-option2-id',
          label: 'another-category1-option2',
        } as CategoryFilter,
      ],
      order: 1,
    },
  };

  const testThreeFilterMeta: FullTableMeta['filters'] = {
    ...testTwoFilterMeta,
    'Another category2': {
      name: 'Another category2',
      options: [
        {
          value: 'another-category2-option1-id',
          label: 'another-category2-option1',
        } as CategoryFilter,
        {
          value: 'another-category2-option2-id',
          label: 'another-category2-option2',
        } as CategoryFilter,
      ],
      order: 2,
    },
  };

  const testIndicatorOptions: Indicator[] = [
    {
      value: 'indicator-1-id',
      label: 'indicator 1',
    } as Indicator,
  ];

  test('returns the correct data sets when a filter, location, indicator and time period are selected', () => {
    const result = getSelectedDataSets({
      filters: testOneFilterMeta,
      indicatorOptions: testIndicatorOptions,
      values: {
        filters: {
          'School type': 'secondary-id',
        },
        indicator: 'indicator-1-id',
        location: LocationFilter.createId({
          value: 'location-id',
          level: 'localAuthority',
        }),
        timePeriod: 'time-id',
      },
    });

    const expected = [
      {
        filters: ['secondary-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the correct data sets when multiple individual filters are selected', () => {
    const result = getSelectedDataSets({
      filters: testTwoFilterMeta,
      indicatorOptions: testIndicatorOptions,
      values: {
        filters: {
          'School type': 'secondary-id',
          'Another category1': 'another-category1-option2-id',
        },
        indicator: 'indicator-1-id',
        location: LocationFilter.createId({
          value: 'location-id',
          level: 'localAuthority',
        }),
        timePeriod: 'time-id',
      },
    });

    expect(result).toEqual([
      {
        filters: ['secondary-id', 'another-category1-option2-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
    ]);
  });

  test('returns the correct data sets when "All" options are selected for a filter', () => {
    const result = getSelectedDataSets({
      filters: testOneFilterMeta,
      indicatorOptions: testIndicatorOptions,
      values: {
        filters: {
          'School type': '',
        },
        indicator: 'indicator-1-id',
        location: LocationFilter.createId({
          value: 'location-id',
          level: 'localAuthority',
        }),
        timePeriod: 'time-id',
      },
    });

    expect(result).toEqual([
      {
        filters: ['secondary-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['primary-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['special-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
    ]);
  });

  test('returns the correct data sets when "All" options are selected for two filters', () => {
    const result = getSelectedDataSets({
      filters: testTwoFilterMeta,
      indicatorOptions: testIndicatorOptions,
      values: {
        filters: {
          'School type': '',
          'Another category1': '',
        },
        indicator: 'indicator-1-id',
        location: LocationFilter.createId({
          value: 'location-id',
          level: 'localAuthority',
        }),
        timePeriod: 'time-id',
      },
    });

    expect(result).toEqual([
      {
        filters: ['secondary-id', 'another-category1-option1-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['secondary-id', 'another-category1-option2-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['primary-id', 'another-category1-option1-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['primary-id', 'another-category1-option2-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['special-id', 'another-category1-option1-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: ['special-id', 'another-category1-option2-id'],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
    ]);
  });

  test('returns the correct data sets when "All" options are selected for three filters', () => {
    const result = getSelectedDataSets({
      filters: testThreeFilterMeta,
      indicatorOptions: testIndicatorOptions,
      values: {
        filters: {
          'School type': '',
          'Another category1': '',
          'Another category2': '',
        },
        indicator: 'indicator-1-id',
        location: LocationFilter.createId({
          value: 'location-id',
          level: 'localAuthority',
        }),
        timePeriod: 'time-id',
      },
    });

    expect(result).toEqual([
      {
        filters: [
          'secondary-id',
          'another-category1-option1-id',
          'another-category2-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'secondary-id',
          'another-category1-option1-id',
          'another-category2-option2-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'secondary-id',
          'another-category1-option2-id',
          'another-category2-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'secondary-id',
          'another-category1-option2-id',
          'another-category2-option2-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'primary-id',
          'another-category1-option1-id',
          'another-category2-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'primary-id',
          'another-category1-option1-id',
          'another-category2-option2-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'primary-id',
          'another-category1-option2-id',
          'another-category2-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'primary-id',
          'another-category1-option2-id',
          'another-category2-option2-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'special-id',
          'another-category1-option1-id',
          'another-category2-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'special-id',
          'another-category1-option1-id',
          'another-category2-option2-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'special-id',
          'another-category1-option2-id',
          'another-category2-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'special-id',
          'another-category1-option2-id',
          'another-category2-option2-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
    ]);
  });

  test('returns the correct data sets when "All" option is selected for a filter and some other filters are selected', () => {
    const result = getSelectedDataSets({
      filters: testThreeFilterMeta,
      indicatorOptions: testIndicatorOptions,
      values: {
        filters: {
          'School type': '',
          'Another category1': 'another-category1-option2-id',
          'Another category2': 'another-category1-option1-id',
        },
        indicator: 'indicator-1-id',
        location: LocationFilter.createId({
          value: 'location-id',
          level: 'localAuthority',
        }),
        timePeriod: 'time-id',
      },
    });

    expect(result).toEqual([
      {
        filters: [
          'secondary-id',
          'another-category1-option2-id',
          'another-category1-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'primary-id',
          'another-category1-option2-id',
          'another-category1-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
      {
        filters: [
          'special-id',
          'another-category1-option2-id',
          'another-category1-option1-id',
        ],
        indicator: 'indicator-1-id',
        location: {
          level: 'localAuthority',
          value: 'location-id',
        },
        timePeriod: 'time-id',
      },
    ]);
  });
});
