import { AxisConfiguration } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';
import produce from 'immer';

describe('createDataSetCategories', () => {
  const testTable: TableDataResponse = {
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
      ],
      locations: {
        localAuthority: [
          { id: 'barnet', label: 'Barnet', value: 'barnet' },
          { id: 'barnsley', label: 'Barnsley', value: 'barnsley' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { code: 'AY', label: '2014/15', year: 2014 },
        { code: 'AY', label: '2015/16', year: 2015 },
      ],
      geoJsonAvailable: true,
    },
    results: [
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '2613',
          'overall-absence-sessions': '3134',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '30364',
          'overall-absence-sessions': '40327',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': 'x',
          'overall-absence-sessions': 'x',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '479',
          'overall-absence-sessions': '843',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '1939',
          'overall-absence-sessions': '2269',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '26594',
          'overall-absence-sessions': '37084',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '19',
          'overall-absence-sessions': '35',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '939',
          'overall-absence-sessions': '1268',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '27833',
          'overall-absence-sessions': '38130',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '39',
          'overall-absence-sessions': '83',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '31322',
          'overall-absence-sessions': '41228',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '2652',
          'overall-absence-sessions': '3093',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '1856',
          'overall-absence-sessions': '2125',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '745',
          'overall-absence-sessions': '1105',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '825',
          'overall-absence-sessions': '1003',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '4',
          'overall-absence-sessions': '4',
        },
        timePeriod: '2015_AY',
      },
    ],
  };

  test('returns single data set when a complete data set is configured', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2014_AY',
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe('2014/15');

    const dataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(dataSets).toHaveLength(1);

    const [[dataSet1Key, dataSet1]] = dataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });

    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });
  });

  test('returns expanded data sets when data set is missing time period', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('2014/15');

    const category1DataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(category1DataSets).toHaveLength(1);

    const [[dataSet1Key, dataSet1]] = category1DataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });

    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });

    expect(dataSetCategories[1].filter.label).toBe('2015/16');

    const category2DataSets = Object.entries(dataSetCategories[1].dataSets);

    expect(category2DataSets).toHaveLength(1);

    const [[dataSet2Key, dataSet2]] = category2DataSets;

    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });

    expect(dataSet2.value).toBe(2652);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
    });
  });

  test('returns expanded data sets when data set is missing time period and location', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('2014/15');

    const category1DataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(category1DataSets).toHaveLength(2);

    const [
      [dataSet1Key, dataSet1],
      [dataSet2Key, dataSet2],
    ] = category1DataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet2.value).toBe(39);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
    });

    expect(dataSetCategories[1].filter.label).toBe('2015/16');

    const category2DataSets = Object.entries(dataSetCategories[1].dataSets);

    expect(category2DataSets).toHaveLength(2);

    const [
      [dataSet3Key, dataSet3],
      [dataSet4Key, dataSet4],
    ] = category2DataSets;

    expect(JSON.parse(dataSet3Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet3.value).toBe(2652);
    expect(dataSet3.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  test('returns expanded data sets categorised by locations when data is missing location and time period', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'locations',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('Barnet');

    const category1DataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(category1DataSets).toHaveLength(2);

    const [
      [dataSet1Key, dataSet1],
      [dataSet2Key, dataSet2],
    ] = category1DataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2014_AY',
    });
    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
    });
    expect(dataSet2.value).toBe(2652);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
    });

    expect(dataSetCategories[1].filter.label).toBe('Barnsley');

    const category2DataSets = Object.entries(dataSetCategories[1].dataSets);

    expect(category2DataSets).toHaveLength(2);

    const [
      [dataSet3Key, dataSet3],
      [dataSet4Key, dataSet4],
    ] = category2DataSets;

    expect(JSON.parse(dataSet3Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2014_AY',
    });
    expect(dataSet3.value).toBe(39);
    expect(dataSet3.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  test('returns expanded chart data sets categorised by indicators when data is missing location and time period', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'indicators',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe(
      'Number of authorised absence sessions',
    );

    const category1DataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(category1DataSets).toHaveLength(4);

    const [
      [dataSet1Key, dataSet1],
      [dataSet2Key, dataSet2],
      [dataSet3Key, dataSet3],
      [dataSet4Key, dataSet4],
    ] = category1DataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet2Key)).toEqual({
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet2.value).toBe(2652);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
    });

    expect(JSON.parse(dataSet3Key)).toEqual({
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet3.value).toBe(39);
    expect(dataSet3.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  test('returns expanded data sets categorised by filters when data set is missing location and time period', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'filters',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('Ethnicity Major Chinese');

    const category1DataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(category1DataSets).toHaveLength(4);

    const [
      [dataSet1Key, dataSet1],
      [dataSet2Key, dataSet2],
      [dataSet3Key, dataSet3],
      [dataSet4Key, dataSet4],
    ] = category1DataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['state-funded-primary'],
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['state-funded-primary'],
      timePeriod: '2015_AY',
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet2.value).toBe(2652);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
    });

    expect(JSON.parse(dataSet3Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['state-funded-primary'],
      timePeriod: '2014_AY',
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet3.value).toBe(39);
    expect(dataSet3.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['state-funded-primary'],
      timePeriod: '2015_AY',
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  test('uses most specific configuration for expanded data sets', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          timePeriod: '2015_AY',
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('2014/15');

    const category1DataSets = Object.entries(dataSetCategories[0].dataSets);

    expect(category1DataSets).toHaveLength(2);

    const [
      [dataSet1Key, dataSet1],
      [dataSet2Key, dataSet2],
    ] = category1DataSets;

    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
    });

    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet2.value).toBe(39);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
    });

    expect(dataSetCategories[1].filter.label).toBe('2015/16');

    const category2DataSets = Object.entries(dataSetCategories[1].dataSets);

    expect(category2DataSets).toHaveLength(2);

    const [
      [dataSet3Key, dataSet3],
      [dataSet4Key, dataSet4],
    ] = category2DataSets;

    expect(JSON.parse(dataSet3Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
    });
    expect(dataSet3.value).toBe(2652);
    expect(dataSet3.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  test('does not categorise by siblingless filters', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'filters',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const updatedTestTable = produce(testTable, draft => {
      draft.subjectMeta.filters.SchoolType.options.Default.options = [
        {
          label: 'State-funded primary',
          value: 'state-funded-primary',
        },
      ];
    });
    const fullTable = mapFullTable(updatedTestTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe('Ethnicity Major Chinese');
  });

  test('falls back to categorising by siblingless filters if there would be no categories otherwise', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'filters',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const updatedTestTable = produce(testTable, draft => {
      draft.subjectMeta.filters.Characteristic.options.EthnicGroupMajor.options = [
        {
          label: 'Ethnicity Major Chinese',
          value: 'ethnicity-major-chinese',
        },
      ];
      draft.subjectMeta.filters.SchoolType.options.Default.options = [
        {
          label: 'State-funded primary',
          value: 'state-funded-primary',
        },
      ];
    });
    const fullTable = mapFullTable(updatedTestTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('Ethnicity Major Chinese');
    expect(dataSetCategories[1].filter.label).toBe('State-funded primary');
  });

  test('matches results correctly when table has multiple location levels', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['state-funded-primary'],
          location: {
            level: 'country',
            value: 'england',
          },
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['state-funded-secondary'],
          location: {
            level: 'country',
            value: 'england',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable({
      subjectMeta: {
        filters: {
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
            order: 0,
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
        ],
        locations: {
          country: [{ id: 'england', label: 'England', value: 'england' }],
          region: [
            { id: 'north-east', label: 'North East', value: 'north-east' },
          ],
        },
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence by geographic level',
        timePeriodRange: [{ code: 'AY', label: '2015/16', year: 2015 }],
        geoJsonAvailable: true,
      },
      results: [
        {
          filters: ['state-funded-primary'],
          geographicLevel: 'region',
          locationId: 'north-east',
          measures: { 'authorised-absence-sessions': '1892595' },
          timePeriod: '2015_AY',
        },
        {
          filters: ['state-funded-primary'],
          geographicLevel: 'country',
          locationId: 'england',
          measures: { 'authorised-absence-sessions': '42219483' },
          timePeriod: '2015_AY',
        },
        {
          filters: ['state-funded-secondary'],
          geographicLevel: 'region',
          locationId: 'north-east',
          measures: { 'authorised-absence-sessions': '1856777' },
          timePeriod: '2015_AY',
        },
        {
          filters: ['state-funded-secondary'],
          geographicLevel: 'country',
          locationId: 'england',
          measures: { 'authorised-absence-sessions': '37997247' },
          timePeriod: '2015_AY',
        },
      ],
    });

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe('2015/16');

    const dataSets = Object.values(dataSetCategories[0].dataSets);

    expect(dataSets).toHaveLength(2);

    expect(dataSets[0].value).toBe(42219483);
    expect(dataSets[0].dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['state-funded-primary'],
      location: {
        level: 'country',
        value: 'england',
      },
      timePeriod: '2015_AY',
    });

    expect(dataSets[1].value).toBe(37997247);
    expect(dataSets[1].dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['state-funded-secondary'],
      location: {
        level: 'country',
        value: 'england',
      },
      timePeriod: '2015_AY',
    });
  });

  test('group by specific filter category', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'filters',
      groupByFilter: 'school_type',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const updatedTestTable = produce(testTable, draft => {
      draft.subjectMeta.filters.Characteristic.options.EthnicGroupMajor.options = [
        {
          label: 'Ethnicity Major Chinese',
          value: 'ethnicity-major-chinese',
        },
        {
          label: 'Ethnicity another',
          value: 'ethnicity-another',
        },
      ];
      draft.subjectMeta.filters.SchoolType.options.Default.options = [
        {
          label: 'State-funded primary',
          value: 'state-funded-primary',
        },
        {
          label: 'State-funded secondary',
          value: 'state-funded-secondary',
        },
      ];
    });
    const fullTable = mapFullTable(updatedTestTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(2);
    expect(dataSetCategories[0].filter.label).toBe('State-funded primary');
    expect(dataSetCategories[1].filter.label).toBe('State-funded secondary');
  });

  test('includes datasets with NaN values when includeNonNumericData is true', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2014_AY',
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2015_AY',
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
      true,
    );

    expect(dataSetCategories).toHaveLength(2);

    expect(dataSetCategories[0].filter.label).toBe('2014/15');

    const dataSets1 = Object.entries(dataSetCategories[0].dataSets);
    expect(dataSets1).toHaveLength(1);

    const [[dataSet1Key, dataSet1]] = dataSets1;
    expect(JSON.parse(dataSet1Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet1.value).toBe(null);
    expect(dataSet1.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
    });

    expect(dataSetCategories[1].filter.label).toBe('2015/16');

    const dataSets2 = Object.entries(dataSetCategories[1].dataSets);
    expect(dataSets2).toHaveLength(1);

    const [[dataSet2Key, dataSet2]] = dataSets2;
    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet2.value).toBe(4);
    expect(dataSet2.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  test('excludes datasets with NaN values when includeNonNumericData is false', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2014_AY',
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2015_AY',
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe('2015/16');

    const dataSets = Object.entries(dataSetCategories[0].dataSets);
    expect(dataSets).toHaveLength(1);

    const [[dataSetKey, dataSet]] = dataSets;
    expect(JSON.parse(dataSetKey)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet.value).toBe(4);
    expect(dataSet.dataSet).toEqual<DataSet>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
    });
  });

  describe('ordering data sets with `order` property', () => {
    const testOrderedAxisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2015_AY',
          order: 0,
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2015_AY',
          order: 1,
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2014_AY',
          order: 2,
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2014_AY',
          order: 3,
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    test('grouped by time period', () => {
      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        testOrderedAxisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe('2015/16');
      expect(dataSetCategories[1].filter.label).toBe('2014/15');
    });

    test('grouped by filter', () => {
      const axisConfiguration: AxisConfiguration = {
        ...testOrderedAxisConfiguration,
        groupBy: 'filters',
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.value).toBe('ethnicity-major-chinese');
      expect(dataSetCategories[1].filter.value).toBe('state-funded-primary');
    });

    test('grouped by indicator', () => {
      const indicatorDataSets = [
        {
          indicator: 'overall-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2015_AY',
          order: 0,
        },
        {
          indicator: 'overall-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2015_AY',
          order: 1,
        },
      ];

      const axisConfiguration: AxisConfiguration = {
        ...testOrderedAxisConfiguration,
        groupBy: 'indicators',
        dataSets: [
          ...indicatorDataSets,
          ...testOrderedAxisConfiguration.dataSets,
        ],
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe(
        'Number of overall absence sessions',
      );
      expect(dataSetCategories[1].filter.label).toBe(
        'Number of authorised absence sessions',
      );
    });

    test('grouped by location', () => {
      const axisConfiguration: AxisConfiguration = {
        ...testOrderedAxisConfiguration,
        groupBy: 'locations',
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.value).toBe('barnet');
      expect(dataSetCategories[1].filter.value).toBe('barnsley');
    });

    test('reversed when `sortAsc` = false', () => {
      const axisConfiguration: AxisConfiguration = {
        ...testOrderedAxisConfiguration,
        groupBy: 'filters',
        sortAsc: false,
      };
      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe('State-funded primary');
      expect(dataSetCategories[1].filter.label).toBe('Ethnicity Major Chinese');
    });
  });

  describe('ordering for older charts with no `order` property', () => {
    const testUnorderedAxisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'authorised-absence-sessions',
          filters: ['state-funded-primary', 'ethnicity-major-chinese'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2015_AY',
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['state-funded-primary', 'ethnicity-major-chinese'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2015_AY',
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['state-funded-primary', 'ethnicity-major-chinese'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2014_AY',
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['state-funded-primary', 'ethnicity-major-chinese'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2014_AY',
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    test('orders by `label` when grouped by filters', () => {
      const axisConfiguration: AxisConfiguration = {
        ...testUnorderedAxisConfiguration,
        groupBy: 'filters',
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe('Ethnicity Major Chinese');
      expect(dataSetCategories[1].filter.label).toBe('State-funded primary');
    });

    test('orders by `label` when grouped by indicator', () => {
      const indicatorDataSets = [
        {
          indicator: 'overall-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          timePeriod: '2015_AY',
        },
        {
          indicator: 'overall-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          location: {
            level: 'localAuthority',
            value: 'barnet',
          },
          timePeriod: '2015_AY',
        },
      ];

      const axisConfiguration: AxisConfiguration = {
        ...testUnorderedAxisConfiguration,
        groupBy: 'indicators',
        dataSets: [
          ...indicatorDataSets,
          ...testUnorderedAxisConfiguration.dataSets,
        ],
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe(
        'Number of authorised absence sessions',
      );
      expect(dataSetCategories[1].filter.label).toBe(
        'Number of overall absence sessions',
      );
    });

    test('orders by `label` when grouped by location', () => {
      const axisConfiguration: AxisConfiguration = {
        ...testUnorderedAxisConfiguration,
        groupBy: 'locations',
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe('Barnet');
      expect(dataSetCategories[1].filter.label).toBe('Barnsley');
    });

    test('orders by `order` when grouped by time period', () => {
      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        testUnorderedAxisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe('2014/15');
      expect(dataSetCategories[1].filter.label).toBe('2015/16');
    });

    test('reversed when `sortAsc` = false', () => {
      const axisConfiguration: AxisConfiguration = {
        ...testUnorderedAxisConfiguration,
        groupBy: 'filters',
        sortAsc: false,
      };

      const fullTable = mapFullTable(testTable);
      const dataSetCategories = createDataSetCategories(
        axisConfiguration,
        fullTable.results,
        fullTable.subjectMeta,
      );

      expect(dataSetCategories).toHaveLength(2);
      expect(dataSetCategories[0].filter.label).toBe('State-funded primary');
      expect(dataSetCategories[1].filter.label).toBe('Ethnicity Major Chinese');
    });
  });
});
