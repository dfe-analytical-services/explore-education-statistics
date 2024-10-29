import { DataGroupingConfig } from '@common/modules/charts/types/chart';
import { DataSet, DataSetCategory } from '@common/modules/charts/types/dataSet';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import getMapDataSetCategoryConfigs, {
  defaultDataGrouping,
  MapDataSetCategoryConfig,
} from '../getMapDataSetCategoryConfigs';

describe('getMapDataSetCategoryConfigs', () => {
  const testFilterGroup1Item1 = new CategoryFilter({
    category: 'Filter1',
    group: 'Filter group 1',
    label: 'Filter group 1 item 1',
    value: 'filter-group-1-item-1',
  });
  const testIndicator1 = new Indicator({
    label: 'Indicator 1',
    name: 'indicator-1-name',
    unit: '',
    value: 'indicator-1',
  });
  const testLocation1 = new LocationFilter({
    id: 'location-1-id',
    label: 'Location 1',
    level: 'country',
    value: 'location-1',
  });
  const testTimePeriod1 = new TimePeriodFilter({
    year: 2020,
    code: 'AY',
    label: '2020/21',
    order: 0,
  });

  const testSubjectMeta: FullTableMeta = {
    boundaryLevels: [],
    filters: {
      Filter1: {
        name: 'filter-1',
        options: [testFilterGroup1Item1],
        order: 0,
      },
    },
    footnotes: [],
    geoJsonAvailable: false,
    indicators: [testIndicator1],
    locations: [testLocation1],
    publicationName: 'Publication 1',
    subjectName: 'Subject 1',
    timePeriodRange: [testTimePeriod1],
  };

  const testDataSet1: DataSet = {
    filters: [testFilterGroup1Item1.id],
    indicator: testIndicator1.id,
    timePeriod: testTimePeriod1.id,
    location: {
      level: testLocation1.level,
      value: testLocation1.value,
    },
  };

  const testDataGrouping: DataGroupingConfig = {
    customGroups: [{ min: 0, max: 999 }],
    numberOfGroups: 6,
    type: 'Quantiles',
  };

  test("boundaryLevel and dataGrouping's defaulted when generated with no pre-existing datasets", () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const result = getMapDataSetCategoryConfigs({
      dataSetConfigs: [],
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: MapDataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: defaultDataGrouping,
        dataKey: generateDataSetKey(testDataSet1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: testDataSet1,
        boundaryLevel: undefined,
      },
    ];

    expect(result).toEqual(expected);
  });
  test('boundaryLevel and dataGrouping are set when generated with pre-existing dataSets', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const result = getMapDataSetCategoryConfigs({
      dataSetConfigs: [
        {
          dataSet: testDataSet1,
          dataGrouping: testDataGrouping,
          boundaryLevel: 15,
        },
      ],
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: MapDataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: testDataSet1,
        boundaryLevel: 15,
      },
    ];

    expect(result).toEqual(expected);
  });
});
