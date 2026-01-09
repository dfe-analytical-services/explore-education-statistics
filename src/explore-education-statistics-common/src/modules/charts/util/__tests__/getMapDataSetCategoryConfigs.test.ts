import {
  DataGroupingConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
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
} from '../getMapDataSetCategoryConfigs';

describe('getMapDataSetCategoryConfigs', () => {
  const testFilterGroupItem = new CategoryFilter({
    category: 'Filter1',
    group: 'Filter group 1',
    label: 'Filter group 1 item 1',
    value: 'filter-group-1-item-1',
  });
  const testFilterGroupItem2 = new CategoryFilter({
    category: 'Filter2',
    group: 'Filter group 2',
    label: 'Filter group 2 item 2',
    value: 'filter-group-2-item-2',
  });
  const testIndicator = new Indicator({
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
  const testTimePeriod = new TimePeriodFilter({
    year: 2020,
    code: 'AY',
    label: '2020/21',
    order: 0,
  });

  const testSubjectMeta: FullTableMeta = {
    boundaryLevels: [],
    filters: {
      Filter1: {
        legend: 'Filter 1',
        name: 'filter-1',
        options: [testFilterGroupItem],
        order: 0,
      },
      Filter2: {
        legend: 'Filter 2',
        name: 'filter-2',
        options: [testFilterGroupItem2],
        order: 0,
      },
    },
    footnotes: [],
    geoJsonAvailable: false,
    indicators: [testIndicator],
    locations: [testLocation1],
    publicationName: 'Publication 1',
    subjectName: 'Subject 1',
    timePeriodRange: [testTimePeriod],
  };

  const testDataSets: DataSet[] = [
    {
      filters: [testFilterGroupItem.id],
      indicator: testIndicator.id,
      timePeriod: testTimePeriod.id,
      location: {
        level: testLocation1.level,
        value: testLocation1.value,
      },
    },
    {
      filters: [testFilterGroupItem2.id],
      indicator: testIndicator.id,
      timePeriod: testTimePeriod.id,
      location: {
        level: testLocation1.level,
        value: testLocation1.value,
      },
    },
  ];

  const testDataGrouping: DataGroupingConfig = {
    customGroups: [{ min: 0, max: 999 }],
    numberOfGroups: 6,
    type: 'Quantiles',
  };

  test('boundaryLevel and dataGrouping are set to defaults when no dataSetConfigs provided', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSets[0])]: {
            dataSet: testDataSets[0],
            value: 30,
          },
          [generateDataSetKey(testDataSets[1])]: {
            dataSet: testDataSets[1],
            value: 20,
          },
        },
        filter: testTimePeriod,
      },
    ];

    const result = getMapDataSetCategoryConfigs({
      dataSetConfigs: [],
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    expect(result).toEqual([
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: defaultDataGrouping,
        dataKey: generateDataSetKey(testDataSets[0]),
        dataSet: expandDataSet(testDataSets[0], testSubjectMeta),
        rawDataSet: testDataSets[0],
        boundaryLevel: undefined,
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1)',
          colour: '#F46A25',
        },
        dataGrouping: defaultDataGrouping,
        dataKey: generateDataSetKey(testDataSets[1]),
        dataSet: expandDataSet(testDataSets[1], testSubjectMeta),
        rawDataSet: testDataSets[1],
        boundaryLevel: undefined,
      },
    ]);
  });

  test('boundaryLevel and dataGrouping are set when dataSetConfigs are provided', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSets[0])]: {
            dataSet: testDataSets[0],
            value: 30,
          },
          [generateDataSetKey(testDataSets[1])]: {
            dataSet: testDataSets[1],
            value: 90,
          },
        },
        filter: testTimePeriod,
      },
    ];
    const customDataGrouping: MapDataSetConfig['dataGrouping'] = {
      customGroups: [
        { min: 0, max: 59 },
        { min: 60, max: 119 },
      ],
      type: 'Custom',
      numberOfGroups: 2,
    };

    const result = getMapDataSetCategoryConfigs({
      dataSetConfigs: [
        {
          dataSet: testDataSets[0],
          dataSetKey: 'dataSetKey1',
          dataGrouping: testDataGrouping,
          boundaryLevel: 15,
        },
        {
          dataSet: testDataSets[1],
          dataSetKey: 'dataSetKey2',
          dataGrouping: customDataGrouping,
          boundaryLevel: 6,
        },
      ],
      dataSetCategories: testDataSetCategories,
      legendItems: [
        { colour: '#aa0000', dataSet: testDataSets[1], label: 'custom-label' },
      ],
      meta: testSubjectMeta,
    });

    expect(result).toEqual([
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSets[0]),
        dataSet: expandDataSet(testDataSets[0], testSubjectMeta),
        rawDataSet: testDataSets[0],
        boundaryLevel: 15,
      },
      {
        config: {
          label: 'custom-label',
          colour: '#aa0000',
        },
        dataGrouping: customDataGrouping,
        dataKey: generateDataSetKey(testDataSets[1]),
        dataSet: expandDataSet(testDataSets[1], testSubjectMeta),
        rawDataSet: testDataSets[1],
        boundaryLevel: 6,
      },
    ]);
  });
});
