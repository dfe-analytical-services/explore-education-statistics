import getCsvData from '@common/modules/table-tool/components/utils/getCsvData';
import {
  WorkerCategoryFilter,
  WorkerFullTableMeta,
  WorkerIndicator,
  WorkerLocationFilter,
  WorkerTimePeriodFilter,
} from '@common/modules/table-tool/types/workerFullTable';

describe('getCsvData', () => {
  const EMPTY_CELL_TEXT = 'no data';

  const testTableMeta: WorkerFullTableMeta = {
    geoJsonAvailable: false,
    publicationName: '',
    subjectName: '',
    footnotes: [],
    boundaryLevels: [],
    filters: {
      Characteristic: {
        name: 'characteristic',
        options: [
          {
            value: 'gender_female',
            label: 'Female',
            group: 'Gender',
            category: 'Characteristic',
          } as WorkerCategoryFilter,
        ],
      },
    },
    indicators: [
      {
        label: 'Authorised absence rate',
        value: 'authAbsRate',
        unit: '%',
        name: 'sess_authorised_percent',
      } as WorkerIndicator,
      {
        label: 'Number of authorised absence sessions',
        value: 'authAbsSess',
        unit: '',
        name: 'sess_authorised',
      } as WorkerIndicator,
    ],
    locations: [
      {
        value: 'england',
        label: 'England',
        level: 'country',
      } as WorkerLocationFilter,
    ],
    timePeriodRange: [
      {
        code: 'AY',
        year: 2015,
        label: '2015/16',
        order: 0,
        value: '2015_AY',
      } as WorkerTimePeriodFilter,
    ],
  };

  test('contains full set of data', async () => {
    const data = getCsvData({
      subjectMeta: {
        ...testTableMeta,
        filters: {
          ...testTableMeta.filters,
          'School Type': {
            name: 'school_type',
            options: [
              {
                value: 'school_primary',
                label: 'State-funded primary',
                category: 'School Type',
              } as WorkerCategoryFilter,
              {
                value: 'school_secondary',
                label: 'State-funded secondary',
                category: 'School Type',
              } as WorkerCategoryFilter,
            ],
          },
        },
        locations: [
          ...testTableMeta.locations,
          {
            value: 'barnsley',
            label: 'Barnsley',
            level: 'localAuthority',
          } as WorkerLocationFilter,
        ],
      },
      results: [
        {
          filters: ['gender_female', 'school_primary'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsRate: '111',
            authAbsSess: '222',
          },
        },
        {
          filters: ['gender_female', 'school_secondary'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsRate: '333',
            authAbsSess: '444',
          },
        },
        {
          filters: ['gender_female', 'school_primary'],
          timePeriod: '2015_AY',
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: {
              code: 'barnsley',
              name: 'Barnsley',
            },
          },
          measures: {
            authAbsRate: '555',
            authAbsSess: '666',
          },
        },
        {
          filters: ['gender_female', 'school_secondary'],
          timePeriod: '2015_AY',
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: {
              code: 'barnsley',
              name: 'Barnsley',
            },
          },
          measures: {
            authAbsRate: '777',
            authAbsSess: '888',
          },
        },
      ],
    });

    expect(data).toHaveLength(5);

    expect(data[0]).toHaveLength(8);

    expect(data[1]).toHaveLength(8);
    expect(data[1][6]).toBe('111');
    expect(data[1][7]).toBe('222');

    expect(data[2]).toHaveLength(8);
    expect(data[2][6]).toBe('333');
    expect(data[2][7]).toBe('444');

    expect(data[3]).toHaveLength(8);
    expect(data[3][6]).toBe('555');
    expect(data[3][7]).toBe('666');

    expect(data[4]).toHaveLength(8);
    expect(data[4][6]).toBe('777');
    expect(data[4][7]).toBe('888');

    expect(data).toMatchSnapshot();
  });

  test(`contains ${EMPTY_CELL_TEXT} if there are only some matching results`, () => {
    const data = getCsvData({
      subjectMeta: {
        ...testTableMeta,
        filters: {
          ...testTableMeta.filters,
          'School Type': {
            name: 'school_type',
            options: [
              {
                value: 'school_primary',
                label: 'State-funded primary',
                category: 'School Type',
              } as WorkerCategoryFilter,
              {
                value: 'school_secondary',
                label: 'State-funded secondary',
                category: 'School Type',
              } as WorkerCategoryFilter,
            ],
          },
        },
      },
      results: [
        {
          filters: ['gender_female', 'school_primary'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsRate: '111',
          },
        },
        {
          filters: ['gender_female', 'school_secondary'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsSess: '222',
          },
        },
      ],
    });

    expect(data).toHaveLength(3);

    expect(data[0]).toHaveLength(8);

    expect(data[1]).toHaveLength(8);
    expect(data[1][6]).toBe('111');
    expect(data[1][7]).toBe(EMPTY_CELL_TEXT);

    expect(data[2]).toHaveLength(8);
    expect(data[2][6]).toBe(EMPTY_CELL_TEXT);
    expect(data[2][7]).toBe('222');

    expect(data).toMatchSnapshot();
  });

  test(`strips out rows with only ${EMPTY_CELL_TEXT}`, () => {
    const data = getCsvData({
      subjectMeta: {
        ...testTableMeta,
        filters: {
          ...testTableMeta.filters,
          'School Type': {
            name: 'school_type',
            options: [
              {
                value: 'school_primary',
                label: 'State-funded primary',
                category: 'School Type',
              } as WorkerCategoryFilter,
              {
                value: 'school_secondary',
                label: 'State-funded secondary',
                category: 'School Type',
              } as WorkerCategoryFilter,
            ],
          },
        },
      },
      results: [
        {
          filters: ['gender_female', 'school_primary'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsRate: '111',
            authAbsSess: '222',
          },
        },
      ],
    });

    expect(data).toHaveLength(2);

    expect(data[0]).toHaveLength(8);

    expect(data[1]).toHaveLength(8);
    expect(data[1][6]).toBe('111');
    expect(data[1][7]).toBe('222');

    expect(data).toMatchSnapshot();
  });

  test('returns only header if there are no results', () => {
    const data = getCsvData({
      subjectMeta: testTableMeta,
      results: [],
    });

    expect(data).toHaveLength(1);

    expect(data[0]).toHaveLength(7);

    expect(data).toMatchSnapshot();
  });

  test(`contains ${EMPTY_CELL_TEXT} if matching result but no matching indicator`, () => {
    const data = getCsvData({
      subjectMeta: testTableMeta,
      results: [
        {
          filters: ['gender_female'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsSess: '111',
          },
        },
      ],
    });

    expect(data).toHaveLength(2);

    expect(data[0]).toHaveLength(7);

    expect(data[1]).toHaveLength(7);
    expect(data[1][5]).toBe(EMPTY_CELL_TEXT);
    expect(data[1][6]).toBe('111');

    expect(data).toMatchSnapshot();
  });

  test('does not format values in any way', () => {
    const data = getCsvData({
      subjectMeta: testTableMeta,
      results: [
        {
          filters: ['gender_female'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsRate: '12300000',
            authAbsSess: '44255667.2356',
          },
        },
      ],
    });

    expect(data).toHaveLength(2);

    expect(data[0]).toHaveLength(7);

    expect(data[1]).toHaveLength(7);
    expect(data[1][5]).toBe('12300000');
    expect(data[1][6]).toBe('44255667.2356');

    expect(data).toMatchSnapshot();
  });

  test('can contain suppressed cells', () => {
    const data = getCsvData({
      subjectMeta: testTableMeta,
      results: [
        {
          filters: ['gender_female'],
          timePeriod: '2015_AY',
          geographicLevel: 'country',
          location: {
            country: {
              code: 'england',
              name: 'England',
            },
          },
          measures: {
            authAbsRate: '13.4',
            authAbsSess: 'x',
          },
        },
      ],
    });

    expect(data).toHaveLength(2);

    expect(data[0]).toHaveLength(7);

    expect(data[1]).toHaveLength(7);
    expect(data[1][5]).toBe('13.4');
    expect(data[1][6]).toBe('x');

    expect(data).toMatchSnapshot();
  });
});
