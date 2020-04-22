import { AxisConfiguration } from '@common/modules/charts/types/chart';
import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

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
                  label: 'State-funded primary',
                  value: 'state-funded-primary',
                },
                {
                  label: 'State-funded secondary',
                  value: 'state-funded-secondary',
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
      ],
      locations: [
        { level: 'localAuthority', label: 'Barnet', value: 'barnet' },
        { level: 'localAuthority', label: 'Barnsley', value: 'barnsley' },
      ],
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
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '2613',
          'overall-absence-sessions': '3134',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '30364',
          'overall-absence-sessions': '40327',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': 'x',
          'overall-absence-sessions': 'x',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': '479',
          'overall-absence-sessions': '843',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '1939',
          'overall-absence-sessions': '2269',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '26594',
          'overall-absence-sessions': '37084',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': '19',
          'overall-absence-sessions': '35',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': '939',
          'overall-absence-sessions': '1268',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '27833',
          'overall-absence-sessions': '38130',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': '39',
          'overall-absence-sessions': '83',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '31322',
          'overall-absence-sessions': '41228',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '2652',
          'overall-absence-sessions': '3093',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnet', name: 'Barnet' },
        },
        measures: {
          'authorised-absence-sessions': '1856',
          'overall-absence-sessions': '2125',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': '745',
          'overall-absence-sessions': '1105',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
        measures: {
          'authorised-absence-sessions': '825',
          'overall-absence-sessions': '1003',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        geographicLevel: 'LocalAuthority',
        location: {
          localAuthority: { code: 'barnsley', name: 'Barnsley' },
        },
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
          config: {
            label: 'Test label 1',
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
      timePeriod: '2014_AY',
    });

    expect(dataSet1.value).toBe(2613);
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
          config: {
            label: 'Test label 1',
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
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet2.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
          config: {
            label: 'Test label 1',
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
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet2.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet3.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet4.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
          config: {
            label: 'Test label 1',
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
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
    });

    expect(JSON.parse(dataSet2Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
    });
    expect(dataSet2.value).toBe(2652);
    expect(dataSet2.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet3.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
          config: {
            label: 'Test label 1',
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
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet2.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet3.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet4.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
          config: {
            label: 'Test label 1',
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
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet2.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet3.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet4.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
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
          config: {
            label: 'Test label 1',
          },
        },
        {
          indicator: 'authorised-absence-sessions',
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          timePeriod: '2015_AY',
          location: {
            level: 'localAuthority',
            value: 'barnsley',
          },
          config: {
            label: 'Overriding test label',
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
    expect(dataSet1.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet2.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2014_AY',
      config: {
        label: 'Test label 1',
      },
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
    expect(dataSet3.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnet',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Test label 1',
      },
    });

    expect(JSON.parse(dataSet4Key)).toEqual({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      timePeriod: '2015_AY',
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
    });
    expect(dataSet4.value).toBe(19);
    expect(dataSet4.dataSet).toEqual<DataSetConfiguration>({
      indicator: 'authorised-absence-sessions',
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      location: {
        level: 'localAuthority',
        value: 'barnsley',
      },
      timePeriod: '2015_AY',
      config: {
        label: 'Overriding test label',
      },
    });
  });
});
