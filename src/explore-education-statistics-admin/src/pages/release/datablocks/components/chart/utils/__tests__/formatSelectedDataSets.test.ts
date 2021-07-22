import formatSelectedDataSets from '@admin/pages/release/datablocks/components/chart/utils/formatSelectedDataSets';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';

describe('formatSelectedDataSets', () => {
  const testOneFilter = {
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
    },
  };

  const testTwoFilters = {
    ...testOneFilter,
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
    },
  };

  const testThreeFilters = {
    ...testTwoFilters,
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
    },
  };

  const testIndicatorOptions = [
    {
      value: 'indicator-1-id',
      label: 'indicator 1',
    } as Indicator,
  ];

  test('returns the correct data sets when a filter, location, indicator and time period are selected', () => {
    const testValues = {
      filters: {
        'School type': 'secondary-id',
      },
      indicator: 'indicator-1-id',
      location: LocationFilter.createId({
        value: 'location-id',
        level: 'localAuthority',
      }),
      timePeriod: 'time-id',
    };
    const result = formatSelectedDataSets({
      filters: testOneFilter,
      indicatorOptions: testIndicatorOptions,
      values: testValues,
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

  test('returns the correct data sets when multiple filters are selected', () => {
    const testValues = {
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
    };
    const result = formatSelectedDataSets({
      filters: testTwoFilters,
      indicatorOptions: testIndicatorOptions,
      values: testValues,
    });

    const expected = [
      {
        filters: ['secondary-id', 'another-category1-option2-id'],
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

  test('returns the correct data sets when "All options" are selected for a filter', () => {
    const testValues = {
      filters: {
        'School type': '',
      },
      indicator: 'indicator-1-id',
      location: LocationFilter.createId({
        value: 'location-id',
        level: 'localAuthority',
      }),
      timePeriod: 'time-id',
    };
    const result = formatSelectedDataSets({
      filters: testOneFilter,
      indicatorOptions: testIndicatorOptions,
      values: testValues,
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
    ];

    expect(result).toEqual(expected);
  });

  test('returns the correct data sets when "All options" are selected for a two filters', () => {
    const testValues = {
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
    };
    const result = formatSelectedDataSets({
      filters: testTwoFilters,
      indicatorOptions: testIndicatorOptions,
      values: testValues,
    });

    const expected = [
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
    ];

    expect(result).toEqual(expected);
  });

  test('returns the correct data sets when "All options" are selected for three filters', () => {
    const testValues = {
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
    };
    const result = formatSelectedDataSets({
      filters: testThreeFilters,
      indicatorOptions: testIndicatorOptions,
      values: testValues,
    });

    const expected = [
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
    ];

    expect(result).toEqual(expected);
  });

  test('returns the correct data sets when"All options" is selected for a filter and some other filters are selected', () => {
    const testValues = {
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
    };
    const result = formatSelectedDataSets({
      filters: testThreeFilters,
      indicatorOptions: testIndicatorOptions,
      values: testValues,
    });

    const expected = [
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
    ];

    expect(result).toEqual(expected);
  });
});
