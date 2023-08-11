import { DataSet, ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('expandDataSet', () => {
  const testMeta: TableDataResponse = {
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
              order: 0,
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
          { id: 'barnsley-id', label: 'Barnsley', value: 'barnsley' },
          { id: 'barnet-id', label: 'Barnet', value: 'barnet' },
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

  const testFullTable = mapFullTable(testMeta);

  test('creates an expanded data set with class instances of all filters', () => {
    const dataSet: DataSet = {
      filters: ['ethnicity-major-asian-total', 'state-funded-primary'],
      indicator: 'overall-absence-sessions',
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet-id',
      },
    };

    expect(
      expandDataSet(dataSet, testFullTable.subjectMeta),
    ).toEqual<ExpandedDataSet>({
      filters: [
        new CategoryFilter({
          label: 'Ethnicity Major Asian Total',
          value: 'ethnicity-major-asian-total',
          category: 'Characteristic',
          group: 'Ethnic group major',
        }),
        new CategoryFilter({
          label: 'State-funded primary',
          value: 'state-funded-primary',
          category: 'School type',
          group: 'Default',
        }),
      ],
      indicator: new Indicator({
        label: 'Number of overall absence sessions',
        unit: '',
        value: 'overall-absence-sessions',
        name: 'sess_overall',
      }),
      timePeriod: new TimePeriodFilter({
        code: 'AY',
        label: '2014/15',
        year: 2014,
        order: 1,
      }),
      location: new LocationFilter({
        id: 'barnet-id',
        level: 'localAuthority',
        value: 'barnet',
        label: 'Barnet',
      }),
    });
  });

  test('throws error if indicator could not be found', () => {
    const dataSet: DataSet = {
      filters: ['ethnicity-major-asian-total', 'state-funded-primary'],
      indicator: 'indicator-12345',
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet-id',
      },
    };

    expect(() =>
      expandDataSet(dataSet, testFullTable.subjectMeta),
    ).toThrowError("Could not find indicator: 'indicator-12345'");
  });

  test('throws error if filter could not be found', () => {
    const dataSet: DataSet = {
      filters: ['ethnicity-major-asian-total', 'filter-12345'],
      indicator: 'overall-absence-rate',
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet-id',
      },
    };

    expect(() =>
      expandDataSet(dataSet, testFullTable.subjectMeta),
    ).toThrowError("Could not find category filter: 'filter-12345'");
  });

  test('throws error if location could not be found', () => {
    const dataSet: DataSet = {
      filters: ['ethnicity-major-asian-total', 'state-funded-primary'],
      indicator: 'authorised-absence-rate',
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: '12345',
      },
    };

    expect(() =>
      expandDataSet(dataSet, testFullTable.subjectMeta),
    ).toThrowError(
      "Could not find location with value: '12345', level: 'localAuthority'",
    );
  });
});
