import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('getDefaultTableHeadersConfig', () => {
  test('returns correct config using a variety of options for every filter type', () => {
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
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Black Total',
                    value: 'ethnicity-major-black-total',
                  },
                  {
                    label: 'Ethnicity Major Asian Total',
                    value: 'ethnicity-major-asian-total',
                  },
                ],
              },
            },
          },
          SchoolType: {
            totalValue: '',
            hint: 'Filter by school type',
            legend: 'School type',
            name: 'school_type',
            options: {
              Default: {
                label: 'Default',
                options: [
                  {
                    label: 'State-funded secondary',
                    value: 'state-funded-secondary',
                  },
                  {
                    label: 'Special',
                    value: 'special',
                  },
                  {
                    label: 'State-funded primary',
                    value: 'state-funded-primary',
                  },
                ],
              },
            },
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
        locations: [
          { value: 'barnsley', label: 'Barnsley', level: 'localAuthority' },
          { value: 'barnet', label: 'Barnet', level: 'localAuthority' },
        ],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence by characteristic',
        timePeriodRange: [
          { label: '2014/15', code: 'AY', year: 2014 },
          { label: '2015/16', code: 'AY', year: 2015 },
        ],
      },
      results: [],
    };

    const testSubjectMeta = mapFullTable(testTableData);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta.subjectMeta);

    expect(columnGroups).toHaveLength(1);
    expect(columnGroups[0]).toHaveLength(2);
    expect(columnGroups[0][0].label).toBe('Barnsley');
    expect(columnGroups[0][1].label).toBe('Barnet');

    expect(columns).toHaveLength(2);
    expect(columns[0].label).toBe('2014/15');
    expect(columns[1].label).toBe('2015/16');

    expect(rowGroups).toHaveLength(2);
    expect(rowGroups[0]).toHaveLength(2);
    expect(rowGroups[0][0].label).toBe('Ethnicity Major Black Total');
    expect(rowGroups[0][1].label).toBe('Ethnicity Major Asian Total');

    expect(rowGroups[1]).toHaveLength(3);
    expect(rowGroups[1][0].label).toBe('State-funded secondary');
    expect(rowGroups[1][1].label).toBe('Special');
    expect(rowGroups[1][2].label).toBe('State-funded primary');

    expect(rows).toHaveLength(4);
    expect(rows[0].label).toBe('Number of overall absence sessions');
    expect(rows[1].label).toBe('Number of authorised absence sessions');
    expect(rows[2].label).toBe('Number of persistent absentees');
    expect(rows[3].label).toBe('Number of unauthorised absence sessions');
  });

  test('returns correct config using a larger number of time periods than indicators', () => {
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
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Black Total',
                    value: 'ethnicity-major-black-total',
                  },
                  {
                    label: 'Ethnicity Major Asian Total',
                    value: 'ethnicity-major-asian-total',
                  },
                ],
              },
            },
          },
          SchoolType: {
            totalValue: '',
            hint: 'Filter by school type',
            legend: 'School type',
            name: 'school_type',
            options: {
              Default: {
                label: 'Default',
                options: [
                  {
                    label: 'State-funded secondary',
                    value: 'state-funded-secondary',
                  },
                  {
                    label: 'Special',
                    value: 'special',
                  },
                  {
                    label: 'State-funded primary',
                    value: 'state-funded-primary',
                  },
                ],
              },
            },
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
        ],
        locations: [
          { value: 'barnsley', label: 'Barnsley', level: 'localAuthority' },
          { value: 'barnet', label: 'Barnet', level: 'localAuthority' },
        ],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence by characteristic',
        timePeriodRange: [
          { label: '2014/15', code: 'AY', year: 2014 },
          { label: '2015/16', code: 'AY', year: 2015 },
          { label: '2016/17', code: 'AY', year: 2016 },
          { label: '2017/18', code: 'AY', year: 2017 },
          { label: '2018/19', code: 'AY', year: 2018 },
        ],
      },
      results: [],
    };

    const testSubjectMeta = mapFullTable(testTableData);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta.subjectMeta);

    expect(columnGroups).toHaveLength(1);
    expect(columnGroups[0]).toHaveLength(2);
    expect(columnGroups[0][0].label).toBe('Barnsley');
    expect(columnGroups[0][1].label).toBe('Barnet');

    expect(columns).toHaveLength(5);
    expect(columns[0].label).toBe('2014/15');
    expect(columns[1].label).toBe('2015/16');
    expect(columns[2].label).toBe('2016/17');
    expect(columns[3].label).toBe('2017/18');
    expect(columns[4].label).toBe('2018/19');

    expect(rowGroups).toHaveLength(2);
    expect(rowGroups[0]).toHaveLength(2);
    expect(rowGroups[0][0].label).toBe('Ethnicity Major Black Total');
    expect(rowGroups[0][1].label).toBe('Ethnicity Major Asian Total');

    expect(rowGroups[1]).toHaveLength(3);
    expect(rowGroups[1][0].label).toBe('State-funded secondary');
    expect(rowGroups[1][1].label).toBe('Special');
    expect(rowGroups[1][2].label).toBe('State-funded primary');

    expect(rows).toHaveLength(3);
    expect(rows[0].label).toBe('Number of overall absence sessions');
    expect(rows[1].label).toBe('Number of authorised absence sessions');
    expect(rows[2].label).toBe('Number of persistent absentees');
  });

  test('returns correct config when two options for every filter type', () => {
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
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Black Total',
                    value: 'ethnicity-major-black-total',
                  },
                  {
                    label: 'Ethnicity Major Asian Total',
                    value: 'ethnicity-major-asian-total',
                  },
                ],
              },
            },
          },
          SchoolType: {
            totalValue: '',
            hint: 'Filter by school type',
            legend: 'School type',
            name: 'school_type',
            options: {
              Default: {
                label: 'Default',
                options: [
                  {
                    label: 'State-funded secondary',
                    value: 'state-funded-secondary',
                  },
                  {
                    label: 'Special',
                    value: 'special',
                  },
                ],
              },
            },
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
        ],
        locations: [
          { value: 'barnsley', label: 'Barnsley', level: 'localAuthority' },
          { value: 'barnet', label: 'Barnet', level: 'localAuthority' },
        ],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence by characteristic',
        timePeriodRange: [
          { label: '2014/15', code: 'AY', year: 2014 },
          { label: '2015/16', code: 'AY', year: 2015 },
        ],
      },
      results: [],
    };

    const testSubjectMeta = mapFullTable(testTableData);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta.subjectMeta);

    expect(columnGroups).toHaveLength(1);
    expect(columnGroups[0]).toHaveLength(2);
    expect(columnGroups[0][0].label).toBe('Barnsley');
    expect(columnGroups[0][1].label).toBe('Barnet');

    expect(columns).toHaveLength(2);
    expect(columns[0].label).toBe('2014/15');
    expect(columns[1].label).toBe('2015/16');

    expect(rowGroups).toHaveLength(2);
    expect(rowGroups[0]).toHaveLength(2);
    expect(rowGroups[0][0].label).toBe('State-funded secondary');
    expect(rowGroups[0][1].label).toBe('Special');

    expect(rowGroups[1]).toHaveLength(2);
    expect(rowGroups[1][0].label).toBe('Ethnicity Major Black Total');
    expect(rowGroups[1][1].label).toBe('Ethnicity Major Asian Total');

    expect(rows).toHaveLength(2);
    expect(rows[0].label).toBe('Number of overall absence sessions');
    expect(rows[1].label).toBe('Number of authorised absence sessions');
  });

  test('returns correct config with siblingless filters removed when one option for every filter type', () => {
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
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Black Total',
                    value: 'ethnicity-major-black-total',
                  },
                ],
              },
            },
          },
          SchoolType: {
            totalValue: '',
            hint: 'Filter by school type',
            legend: 'School type',
            name: 'school_type',
            options: {
              Default: {
                label: 'Default',
                options: [
                  {
                    label: 'State-funded secondary',
                    value: 'state-funded-secondary',
                  },
                ],
              },
            },
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
        ],
        locations: [
          { value: 'barnsley', label: 'Barnsley', level: 'localAuthority' },
        ],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence by characteristic',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
      },
      results: [],
    };

    const testSubjectMeta = mapFullTable(testTableData);

    const {
      rows,
      rowGroups,
      columns,
      columnGroups,
    } = getDefaultTableHeaderConfig(testSubjectMeta.subjectMeta);

    // Should only have indicators and time periods
    // when all other filter types are siblingless
    expect(columnGroups).toHaveLength(0);
    expect(rowGroups).toHaveLength(0);

    expect(rows).toHaveLength(1);
    expect(rows[0].label).toBe('Number of overall absence sessions');

    expect(columns).toHaveLength(1);
    expect(columns[0].label).toBe('2014/15');
  });
});
