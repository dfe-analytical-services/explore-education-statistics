import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';

describe('generateTableTitle', () => {
  const testMeta: FullTableMeta = {
    geoJsonAvailable: false,
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    footnotes: [],
    boundaryLevels: [],
    filters: {
      Characteristic: {
        name: 'characteristic',
        options: [
          new CategoryFilter({
            value: 'total',
            label: 'Total',
            group: 'Gender',
            category: 'Characteristic',
          }),
        ],
        order: 0,
      },
    },
    indicators: [
      new Indicator({
        label: 'Authorised absence rate',
        value: 'authAbsRate',
        unit: '%',
        name: 'sess_authorised_percent',
      }),
    ],
    locations: [
      new LocationFilter({
        value: 'england',
        label: 'England',
        level: 'country',
      }),
    ],
    timePeriodRange: [
      new TimePeriodFilter({
        code: 'AY',
        year: 2015,
        label: '2015/16',
        order: 0,
      }),
    ],
  };

  test('removes filters labelled "Total"', () => {
    const title = generateTableTitle({
      ...testMeta,
      filters: {
        ...testMeta.filters,
        Characteristic: {
          name: 'characteristic',
          options: [
            new CategoryFilter({
              value: 'total',
              label: 'Total',
              group: 'Gender',
              category: 'Characteristic',
            }),
          ],
          order: 0,
        },
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'total',
              label: 'Total',
              category: 'School Type',
            }),
          ],
          order: 1,
        },
      },
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in England for 2015/16",
    );
  });

  test('with no filters', () => {
    const title = generateTableTitle({
      ...testMeta,
      filters: {},
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in England for 2015/16",
    );
  });

  test('with less than 5 filters', () => {
    const title = generateTableTitle({
      ...testMeta,
      filters: {
        ...testMeta.filters,
        Characteristic: {
          ...testMeta.filters.Characteristic,
          options: [
            new CategoryFilter({
              value: 'gender_female',
              label: 'Female',
              group: 'Gender',
              category: 'Characteristic',
            }),
          ],
          order: 0,
        },
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'total',
              label: 'Total',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_primary',
              label: 'State-funded primary',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_secondary',
              label: 'State-funded secondary',
              category: 'School Type',
            }),
          ],
          order: 1,
        },
      },
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' for Female, State-funded primary and State-funded secondary in England for 2015/16",
    );
  });

  test('with 5 filters', () => {
    const title = generateTableTitle({
      ...testMeta,
      filters: {
        ...testMeta.filters,
        Characteristic: {
          ...testMeta.filters.Characteristic,
          options: [
            ...testMeta.filters.Characteristic.options,
            new CategoryFilter({
              value: 'gender_female',
              label: 'Female',
              group: 'Gender',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'gender_male',
              label: 'Male',
              group: 'Gender',
              category: 'Characteristic',
            }),
          ],
          order: 0,
        },
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'school_special',
              label: 'Special',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_primary',
              label: 'State-funded primary',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_secondary',
              label: 'State-funded secondary',
              category: 'School Type',
            }),
          ],
          order: 1,
        },
      },
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' for Female, Male, Special, State-funded primary and State-funded secondary in England for 2015/16",
    );
  });

  test('with 6 filters', () => {
    const title = generateTableTitle({
      ...testMeta,
      filters: {
        ...testMeta.filters,
        Characteristic: {
          ...testMeta.filters.Characteristic,
          options: [
            ...testMeta.filters.Characteristic.options,
            new CategoryFilter({
              value: 'gender_female',
              label: 'Female',
              group: 'Gender',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'gender_male',
              label: 'Male',
              group: 'Gender',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'ethnicity_major_asian_total',
              label: 'Ethnicity Major Asian Total',
              group: 'Ethnic group major',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'ethnicity_major_black_total',
              label: 'Ethnicity Major Black Total',
              group: 'Ethnic group major',
              category: 'Characteristic',
            }),
          ],
          order: 0,
        },
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'school_special',
              label: 'Special',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_primary',
              label: 'State-funded primary',
              category: 'School Type',
            }),
          ],
          order: 1,
        },
      },
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, Female, Male, Special and 1 other filter in England for 2015/16",
    );
  });

  test('with 7 filters', () => {
    const title = generateTableTitle({
      ...testMeta,
      filters: {
        ...testMeta.filters,
        Characteristic: {
          ...testMeta.filters.Characteristic,
          options: [
            ...testMeta.filters.Characteristic.options,
            new CategoryFilter({
              value: 'gender_female',
              label: 'Female',
              group: 'Gender',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'gender_male',
              label: 'Male',
              group: 'Gender',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'ethnicity_major_asian_total',
              label: 'Ethnicity Major Asian Total',
              group: 'Ethnic group major',
              category: 'Characteristic',
            }),
            new CategoryFilter({
              value: 'ethnicity_major_black_total',
              label: 'Ethnicity Major Black Total',
              group: 'Ethnic group major',
              category: 'Characteristic',
            }),
          ],
          order: 0,
        },
        'School Type': {
          name: 'school_type',
          options: [
            new CategoryFilter({
              value: 'school_special',
              label: 'Special',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_primary',
              label: 'State-funded primary',
              category: 'School Type',
            }),
            new CategoryFilter({
              value: 'school_secondary',
              label: 'State-funded secondary',
              category: 'School Type',
            }),
          ],
          order: 1,
        },
      },
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, Female, Male, Special and 2 other filters in England for 2015/16",
    );
  });

  test('with no time periods', () => {
    const title = generateTableTitle({
      ...testMeta,
      timePeriodRange: [],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in England",
    );
  });

  test('with more than one time period', () => {
    const title = generateTableTitle({
      ...testMeta,
      timePeriodRange: [
        ...testMeta.timePeriodRange,
        new TimePeriodFilter({
          code: 'AY',
          year: 2016,
          label: '2016/17',
          order: 1,
        }),
        new TimePeriodFilter({
          code: 'AY',
          year: 2017,
          label: '2017/18',
          order: 2,
        }),
        new TimePeriodFilter({
          code: 'AY',
          year: 2018,
          label: '2018/19',
          order: 3,
        }),
      ],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in England between 2015/16 and 2018/19",
    );
  });

  test('with no locations', () => {
    const title = generateTableTitle({
      ...testMeta,
      locations: [],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' for 2015/16",
    );
  });

  test('with less than 5 locations', () => {
    const title = generateTableTitle({
      ...testMeta,
      locations: [
        ...testMeta.locations,
        new LocationFilter({
          value: 'barking-and-dagenham',
          label: 'Barking and Dagenham',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'barnet',
          label: 'Barnet',
          level: 'localAuthority',
        }),
        new LocationFilter({
          value: 'adur',
          label: 'Adur',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'allerdale',
          label: 'Allerdale',
          level: 'localAuthorityDistrict',
        }),
      ],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in Adur, Allerdale, Barking and Dagenham, Barnet and England for 2015/16",
    );
  });

  test('with 5 locations', () => {
    const title = generateTableTitle({
      ...testMeta,
      locations: [
        new LocationFilter({
          value: 'one',
          label: 'One',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'two',
          label: 'Two',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'three',
          label: 'Three',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'four',
          label: 'Four',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'five',
          label: 'Five',
          level: 'localAuthorityDistrict',
        }),
      ],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in Five, Four, One, Three and Two for 2015/16",
    );
  });

  test('with 6 locations', () => {
    const title = generateTableTitle({
      ...testMeta,
      locations: [
        ...testMeta.locations,
        new LocationFilter({
          value: 'one',
          label: 'One',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'two',
          label: 'Two',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'three',
          label: 'Three',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'four',
          label: 'Four',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'five',
          label: 'Five',
          level: 'localAuthorityDistrict',
        }),
      ],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in England, Five, Four, One, Three and 1 other location for 2015/16",
    );
  });

  test('with 7 locations', () => {
    const title = generateTableTitle({
      ...testMeta,
      locations: [
        ...testMeta.locations,
        new LocationFilter({
          value: 'one',
          label: 'One',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'two',
          label: 'Two',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'three',
          label: 'Three',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'four',
          label: 'Four',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'five',
          label: 'Five',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          value: 'six',
          label: 'Six',
          level: 'localAuthorityDistrict',
        }),
      ],
    });

    expect(title).toBe(
      "Authorised absence rate for 'Absence by characteristic' in England, Five, Four, One, Six and 2 other locations for 2015/16",
    );
  });

  test('with multiple indicators', () => {
    const title = generateTableTitle({
      ...testMeta,
      indicators: [
        ...testMeta.indicators,
        new Indicator({
          label: 'Number of authorised absence sessions',
          value: 'authAbsSess',
          unit: '',
          name: 'sess_authorised',
        }),
      ],
    });

    expect(title).toBe("'Absence by characteristic' in England for 2015/16");
  });
});
