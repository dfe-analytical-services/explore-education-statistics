import { DataSet } from '@common/modules/charts/types/dataSet';
import isOrphanedDataSet from '@common/modules/charts/util/isOrphanedDataSet';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('isOrphanedDataSet', () => {
  const testTableData: TableDataResponse = {
    subjectMeta: {
      filters: {
        Characteristic: {
          totalValue: '',
          hint: 'Filter by pupil characteristic',
          legend: 'Characteristic',
          options: {
            EthnicGroupMajor: {
              id: 'ethnic-group-major',
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
              order: 0,
            },
          },
          order: 0,
          name: 'characteristic',
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          options: {
            Default: {
              id: 'default',
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
              order: 1,
            },
          },
          order: 1,
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
      locations: {
        localAuthority: [
          { id: 'barnsley', label: 'Barnsley', value: 'barnsley' },
          { id: 'barnet', label: 'Barnet', value: 'barnet' },
        ],
      },
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

  test('is false for a data set that has matching location', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      location: {
        value: 'barnsley',
        level: 'localAuthority',
      },
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      false,
    );
  });

  test('is true for a data set where the location does not match on value', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      location: {
        value: 'does not exist',
        level: 'localAuthority',
      },
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      true,
    );
  });

  test('is true for a data set where the location does not match on level', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      location: {
        value: 'barnsley',
        level: 'localAuthorityDistrict',
      },
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      true,
    );
  });

  test('is false for a data set that has matching time period', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      timePeriod: '2013_AY',
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      false,
    );
  });

  test('is true for a data set where the time period does not match', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      timePeriod: 'does not match',
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      true,
    );
  });

  test('is false for a data set where all filters match', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      false,
    );
  });

  test('is true for a data set where at least one filter does not match', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'does not match'],
      indicator: 'authorised-absence-sessions',
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      true,
    );
  });

  test('is false for a data set where indicator matches', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      false,
    );
  });

  test('is true for a data set where indicator does not match', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'does not match',
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      true,
    );
  });

  test('is false for a data set where all criteria match', () => {
    const testDataSet: DataSet = {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      timePeriod: '2013_AY',
      location: {
        value: 'barnsley',
        level: 'localAuthority',
      },
    };

    expect(isOrphanedDataSet(testDataSet, testFullTable.subjectMeta)).toBe(
      false,
    );
  });
});
