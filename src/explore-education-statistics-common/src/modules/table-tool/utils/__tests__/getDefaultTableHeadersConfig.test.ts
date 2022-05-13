import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('getDefaultTableHeadersConfig', () => {
  const testTableData: TableDataResponse = {
    subjectMeta: {
      filters: {
        Characteristic: {
          totalValue: '',
          hint: '',
          legend: 'Characteristic',
          name: 'characteristic',
          options: {
            EthnicGroupMajor: {
              id: 'ethnic-group-major',
              label: 'Ethnic group major',
              options: [
                {
                  label: 'Ethnicity Major Asian Total',
                  value: 'ethnicity-major-asian-total',
                },
                {
                  label: 'Ethnicity Major Black Total',
                  value: 'ethnicity-major-black-total',
                },
              ],
              order: 0,
            },
          },
          order: 0,
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          name: 'school_type',
          options: {
            Default: {
              id: 'default',
              label: 'Default',
              options: [
                {
                  label: 'Special',
                  value: 'special',
                },
                {
                  label: 'State-funded primary',
                  value: 'state-funded-primary',
                },
                {
                  label: 'State-funded secondary',
                  value: 'state-funded-secondary',
                },
              ],
              order: 0,
            },
          },
          order: 1,
        },
      },
      footnotes: [],
      geoJsonAvailable: false,
      indicators: [
        {
          value: 'overall-absence-sessions',
          label: 'Number of overall absence sessions',
          unit: '',
          name: 'sess_overall',
          decimalPlaces: 2,
        },
        {
          value: 'authorised-absence-sessions',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_authorised',
          decimalPlaces: 2,
        },
        {
          value: 'persistent-absentees',
          label: 'Number of persistent absentees',
          unit: '',
          name: 'enrolments_pa_10_exact',
          decimalPlaces: 2,
        },
        {
          value: 'unauthorised-absence-sessions',
          label: 'Number of unauthorised absence sessions',
          unit: '',
          name: 'sess_unauthorised',
          decimalPlaces: 2,
        },
      ],
      locations: {
        localAuthority: [
          { id: 'barnet', value: 'barnet', label: 'Barnet' },
          { id: 'barnsley', value: 'barnsley', label: 'Barnsley' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { label: '2014/15', code: 'AY', year: 2014 },
        { label: '2015/16', code: 'AY', year: 2015 },
      ],
    },
    results: [
      {
        filters: [],
        geographicLevel: '',
        locationId: '',
        measures: {},
        timePeriod: '2014_AY',
      },
      {
        filters: [],
        geographicLevel: '',
        locationId: '',
        measures: {},
        timePeriod: '2015_AY',
      },
    ],
  };

  test('returns correct config using a variety of options for every filter type', () => {
    const testSubjectMeta = mapFullTable(testTableData);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta);

    expect(columnGroups).toHaveLength(1);
    expect(columnGroups[0]).toHaveLength(2);
    expect(columnGroups[0][0].label).toBe('Barnet');
    expect(columnGroups[0][1].label).toBe('Barnsley');

    expect(columns).toHaveLength(2);
    expect(columns[0].label).toBe('2014/15');
    expect(columns[1].label).toBe('2015/16');

    expect(rowGroups).toHaveLength(2);
    expect(rowGroups[0]).toHaveLength(2);
    expect(rowGroups[0][0].label).toBe('Ethnicity Major Asian Total');
    expect(rowGroups[0][1].label).toBe('Ethnicity Major Black Total');

    expect(rowGroups[1]).toHaveLength(3);
    expect(rowGroups[1][0].label).toBe('Special');
    expect(rowGroups[1][1].label).toBe('State-funded primary');
    expect(rowGroups[1][2].label).toBe('State-funded secondary');

    expect(rows).toHaveLength(4);
    expect(rows[0].label).toBe('Number of overall absence sessions');
    expect(rows[1].label).toBe('Number of authorised absence sessions');
    expect(rows[2].label).toBe('Number of persistent absentees');
    expect(rows[3].label).toBe('Number of unauthorised absence sessions');
  });

  test('returns correct config using a larger number of time periods than indicators', () => {
    const testTableDataTimePeriods: TableDataResponse = {
      ...testTableData,
      subjectMeta: {
        ...testTableData.subjectMeta,
        indicators: [
          {
            value: 'overall-absence-sessions',
            label: 'Number of overall absence sessions',
            unit: '',
            name: 'sess_overall',
            decimalPlaces: 2,
          },
          {
            value: 'authorised-absence-sessions',
            label: 'Number of authorised absence sessions',
            unit: '',
            name: 'sess_authorised',
            decimalPlaces: 2,
          },
          {
            value: 'persistent-absentees',
            label: 'Number of persistent absentees',
            unit: '',
            name: 'enrolments_pa_10_exact',
            decimalPlaces: 2,
          },
        ],
        timePeriodRange: [
          { label: '2014/15', code: 'AY', year: 2014 },
          { label: '2015/16', code: 'AY', year: 2015 },
          { label: '2016/17', code: 'AY', year: 2016 },
          { label: '2017/18', code: 'AY', year: 2017 },
          { label: '2018/19', code: 'AY', year: 2018 },
        ],
      },
      results: [
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2015_AY',
        },
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2016_AY',
        },
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2017_AY',
        },
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2018_AY',
        },
      ],
    };

    const testSubjectMeta = mapFullTable(testTableDataTimePeriods);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta);

    expect(columnGroups).toHaveLength(1);
    expect(columnGroups[0]).toHaveLength(2);
    expect(columnGroups[0][0].label).toBe('Barnet');
    expect(columnGroups[0][1].label).toBe('Barnsley');

    expect(columns).toHaveLength(5);
    expect(columns[0].label).toBe('2014/15');
    expect(columns[1].label).toBe('2015/16');
    expect(columns[2].label).toBe('2016/17');
    expect(columns[3].label).toBe('2017/18');
    expect(columns[4].label).toBe('2018/19');

    expect(rowGroups).toHaveLength(2);
    expect(rowGroups[0]).toHaveLength(2);
    expect(rowGroups[0][0].label).toBe('Ethnicity Major Asian Total');
    expect(rowGroups[0][1].label).toBe('Ethnicity Major Black Total');

    expect(rowGroups[1]).toHaveLength(3);
    expect(rowGroups[1][0].label).toBe('Special');
    expect(rowGroups[1][1].label).toBe('State-funded primary');
    expect(rowGroups[1][2].label).toBe('State-funded secondary');

    expect(rows).toHaveLength(3);
    expect(rows[0].label).toBe('Number of overall absence sessions');
    expect(rows[1].label).toBe('Number of authorised absence sessions');
    expect(rows[2].label).toBe('Number of persistent absentees');
  });

  test('returns correct config when two options for every filter type', () => {
    const testTableDataTwoOptions: TableDataResponse = {
      ...testTableData,
      subjectMeta: {
        ...testTableData.subjectMeta,
        filters: {
          ...testTableData.subjectMeta.filters,
          SchoolType: {
            ...testTableData.subjectMeta.filters.SchoolType,
            options: {
              Default: {
                id: 'default',
                label: 'Default',
                options: [
                  {
                    label: 'Special',
                    value: 'special',
                  },
                  {
                    label: 'State-funded secondary',
                    value: 'state-funded-secondary',
                  },
                ],
                order: 0,
              },
            },
          },
        },
        indicators: [
          {
            value: 'overall-absence-sessions',
            label: 'Number of overall absence sessions',
            unit: '',
            name: 'sess_overall',
            decimalPlaces: 2,
          },
          {
            value: 'authorised-absence-sessions',
            label: 'Number of authorised absence sessions',
            unit: '',
            name: 'sess_authorised',
            decimalPlaces: 2,
          },
        ],
      },
    };

    const testSubjectMeta = mapFullTable(testTableDataTwoOptions);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta);

    expect(columnGroups).toHaveLength(1);
    expect(columnGroups[0]).toHaveLength(2);
    expect(columnGroups[0][0].label).toBe('Barnet');
    expect(columnGroups[0][1].label).toBe('Barnsley');

    expect(columns).toHaveLength(2);
    expect(columns[0].label).toBe('2014/15');
    expect(columns[1].label).toBe('2015/16');

    expect(rowGroups).toHaveLength(2);
    expect(rowGroups[0]).toHaveLength(2);
    expect(rowGroups[0][0].label).toBe('Special');
    expect(rowGroups[0][1].label).toBe('State-funded secondary');

    expect(rowGroups[1]).toHaveLength(2);
    expect(rowGroups[1][0].label).toBe('Ethnicity Major Asian Total');
    expect(rowGroups[1][1].label).toBe('Ethnicity Major Black Total');

    expect(rows).toHaveLength(2);
    expect(rows[0].label).toBe('Number of overall absence sessions');
    expect(rows[1].label).toBe('Number of authorised absence sessions');
  });

  test('returns correct config with siblingless filters removed when one option for every filter type', () => {
    const testTableDataSiblingless: TableDataResponse = {
      ...testTableData,
      subjectMeta: {
        ...testTableData.subjectMeta,
        filters: {
          ...testTableData.subjectMeta.filters,
          Characteristic: {
            ...testTableData.subjectMeta.filters.Characteristic,
            options: {
              EthnicGroupMajor: {
                id: 'ethnic-group-major',
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Black Total',
                    value: 'ethnicity-major-black-total',
                  },
                ],
                order: 0,
              },
            },
          },
          SchoolType: {
            ...testTableData.subjectMeta.filters.SchoolType,
            options: {
              Default: {
                id: 'default',
                label: 'Default',
                options: [
                  {
                    label: 'State-funded secondary',
                    value: 'state-funded-secondary',
                  },
                ],
                order: 0,
              },
            },
          },
        },
        indicators: [
          {
            value: 'overall-absence-sessions',
            label: 'Number of overall absence sessions',
            unit: '',
            name: 'sess_overall',
            decimalPlaces: 2,
          },
        ],
        locations: {
          localAuthority: [
            { id: 'barnsley', value: 'barnsley', label: 'Barnsley' },
          ],
        },
      },
      results: [
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2014_AY',
        },
      ],
    };

    const testSubjectMeta = mapFullTable(testTableDataSiblingless);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta);

    // Should only have indicators and time periods
    // when all other filter types are siblingless
    expect(columnGroups).toHaveLength(0);
    expect(rowGroups).toHaveLength(0);

    expect(rows).toHaveLength(1);
    expect(rows[0].label).toBe('Number of overall absence sessions');

    expect(columns).toHaveLength(1);
    expect(columns[0].label).toBe('2014/15');
  });

  test('returns the correct time periods when terms are selected', () => {
    const testTableDataTerms = {
      ...testTableData,
      subjectMeta: {
        ...testTableData.subjectMeta,
        timePeriodRange: [
          { code: 'T1', label: '2017/18 Autumn Term', year: 2017 },
          { code: 'T1T2', label: '2017/18 Autumn and Spring Term', year: 2017 },
          { code: 'T2', label: '2017/18 Spring Term', year: 2017 },
          { code: 'T3', label: '2017/18 Summer Term', year: 2017 },
          { code: 'T1', label: '2018/19 Autumn Term', year: 2018 },
        ],
      },
      results: [
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2018_T1',
        },
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2017_T1',
        },
      ],
    };

    const testSubjectMeta = mapFullTable(testTableDataTerms);

    const { columns } = getDefaultTableHeaderConfig(testSubjectMeta);

    expect(columns).toHaveLength(2);
    expect(columns[0].label).toBe('2017/18 Autumn Term');
    expect(columns[1].label).toBe('2018/19 Autumn Term');
  });
});
