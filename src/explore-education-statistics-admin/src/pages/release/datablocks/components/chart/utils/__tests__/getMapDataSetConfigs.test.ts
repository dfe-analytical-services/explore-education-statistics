import getMapDataSetConfigs from '@admin/pages/release/datablocks/components/chart/utils/getMapDataSetConfigs';
import {
  AxisConfiguration,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import {
  testMixedData,
  testMixedDataAxisDataSets,
  testMixedMeta,
} from '@common/modules/charts/components/__tests__/__data__/testMixedMapData';
import { testTableData } from '../../__tests__/__data__/testTableData';
import {
  testCategoricalData,
  testCategoricalMeta,
} from '../../__tests__/__data__/testCategoricalData';

describe('getMapDataSetConfigs', () => {
  const testAxisConfiguration: AxisConfiguration = {
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

  test('returns the correct config for numerical data sets', () => {
    const fullTable = mapFullTable(testTableData);

    const result = getMapDataSetConfigs({
      axisMajor: testAxisConfiguration,
      data: fullTable.results,
      meta: fullTable.subjectMeta,
    });

    const expected: MapDataSetConfig[] = [
      {
        boundaryLevel: undefined,
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2014_AY',
        },
        dataSetKey:
          '{"filters":["ethnicity-major-chinese","state-funded-primary"],"indicator":"authorised-absence-sessions","timePeriod":"2014_AY"}',
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the correct config for categorical data sets', () => {
    const result = getMapDataSetConfigs({
      axisMajor: {
        ...testAxisConfiguration,
        dataSets: [
          {
            indicator: 'indicator-1',
            filters: [],
            timePeriod: '2024_CY',
          },
        ],
      },
      data: testCategoricalData,
      meta: testCategoricalMeta,
    });

    const expected: MapDataSetConfig[] = [
      {
        boundaryLevel: undefined,
        categoricalDataConfig: [
          {
            colour: '#12436D',
            value: 'low',
          },
          { colour: '#28A197', value: 'high' },
        ],
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
        dataSet: {
          filters: [],
          indicator: 'indicator-1',
          timePeriod: '2024_CY',
        },
        dataSetKey:
          '{"filters":[],"indicator":"indicator-1","timePeriod":"2024_CY"}',
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the correct config for mixed categorical and numerical data sets', () => {
    const result = getMapDataSetConfigs({
      axisMajor: {
        ...testAxisConfiguration,
        dataSets: testMixedDataAxisDataSets,
      },
      data: testMixedData,
      meta: testMixedMeta,
    });

    const expected: MapDataSetConfig[] = [
      {
        boundaryLevel: undefined,
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
        dataSet: {
          filters: [],
          indicator: 'numerical-indicator',
          timePeriod: '2024_CY',
        },
        dataSetKey:
          '{"filters":[],"indicator":"numerical-indicator","timePeriod":"2024_CY"}',
      },
      {
        boundaryLevel: undefined,
        categoricalDataConfig: [
          {
            colour: '#12436D',
            value: 'large',
          },
        ],
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
        dataSet: {
          filters: [],
          indicator: 'categorical-indicator',
          timePeriod: '2024_CY',
        },
        dataSetKey:
          '{"filters":[],"indicator":"categorical-indicator","timePeriod":"2024_CY"}',
      },
    ];

    expect(result).toHaveLength(2);

    expect(result).toEqual(expected);
  });
});
