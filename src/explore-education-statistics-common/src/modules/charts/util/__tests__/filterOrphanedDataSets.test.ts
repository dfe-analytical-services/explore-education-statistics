import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import filterOrphanedDataSets from '@common/modules/charts/util/filterOrphanedDataSets';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('filterOrphanedDataSets', () => {
  const testTableData: TableDataResponse = {
    subjectMeta: {
      filters: {
        Characteristic: {
          totalValue: '',
          hint: 'Filter by pupil characteristic',
          legend: 'Characteristic',
          options: {
            EthnicGroupMajor: {
              label: 'Ethnic group major',
              options: [
                {
                  label: 'Ethnicity Major Chinese',
                  value: 'ethnicity-major-chinese',
                },
                {
                  label: 'Ethnicity Major Asian Total',
                  value: 'ethnicity-major-asian-total',
                },
                {
                  label: 'Ethnicity Major Black Total',
                  value: 'ethnicity-major-black-total',
                },
              ],
            },
          },
          name: 'characteristic',
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          options: {
            Default: {
              label: 'Default',
              options: [
                {
                  label: 'State-funded secondary',
                  value: 'state-funded-secondary',
                },
                {
                  label: 'State-funded primary',
                  value: 'state-funded-primary',
                },
              ],
            },
          },
          name: 'school_type',
        },
      },
      footnotes: [],
      indicators: [
        {
          label: 'Number of authorised absence sessions',
          unit: '',
          value: 'authorised-absence-sessions',
          name: 'sess_authorised',
        },
        {
          label: 'Number of overall absence sessions',
          unit: '',
          value: 'overall-absence-sessions',
          name: 'sess_overall',
        },
        {
          label: 'Authorised absence rate',
          unit: '%',
          value: 'authorised-absence-rate',
          name: 'sess_authorised_percent',
        },
      ],
      locations: [
        { level: 'localAuthority', label: 'Barnsley', value: 'E08000016' },
        { level: 'localAuthority', label: 'Barnet', value: 'E09000003' },
      ],
      boundaryLevels: [
        {
          id: 2,
          label:
            'Counties and Unitary Authorities December 2018 Boundaries EW BUC',
        },
      ],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { code: 'AY', label: '2013/14', year: 2013 },
        { code: 'AY', label: '2014/15', year: 2014 },
        { code: 'AY', label: '2015/16', year: 2015 },
      ],
      geoJsonAvailable: true,
    },
    results: [],
  };

  const testFullTable = mapFullTable(testTableData);

  test('returns data set that has matching location', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        location: {
          value: 'E08000016',
          level: 'localAuthority',
        },
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual(testDataSets);
  });

  test('removes data set where the location does not match on `value`', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        location: {
          value: 'does not exist',
          level: 'localAuthority',
        },
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual([]);
  });

  test('removes data set where the location does not match on `level`', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        location: {
          value: 'E08000016',
          level: 'localAuthorityDistrict',
        },
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual([]);
  });

  test('returns data sets that have matching time period', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2013_AY',
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual(testDataSets);
  });

  test('removes data set where the time period does not match', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: 'does not match',
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual([]);
  });

  test('returns data set where all filters match', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual(testDataSets);
  });

  test('removes data set where at least one filter does not match', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'does not match'],
        indicator: 'authorised-absence-sessions',
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual([]);
  });

  test('returns data set where indicator matches', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual(testDataSets);
  });

  test('removes data set where indicator does not match', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'does not match',
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual([]);
  });

  test('returns data set where all criteria match', () => {
    const testDataSets: DataSetConfiguration[] = [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2013_AY',
        location: {
          value: 'E08000016',
          level: 'localAuthority',
        },
        config: {
          label: 'Test label',
        },
      },
    ];

    expect(
      filterOrphanedDataSets(testDataSets, testFullTable.subjectMeta),
    ).toEqual(testDataSets);
  });
});
