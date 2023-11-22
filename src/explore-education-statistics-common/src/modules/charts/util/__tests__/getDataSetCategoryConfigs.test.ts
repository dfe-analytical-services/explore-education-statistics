import {
  DataGroupingConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { DataSet, DataSetCategory } from '@common/modules/charts/types/dataSet';
import getDataSetCategoryConfigs, {
  DataSetCategoryConfig,
} from '@common/modules/charts/util/getDataSetCategoryConfigs';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { LegendItem } from '@common/modules/charts/types/legend';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import omit from 'lodash/omit';

describe('getDataSetCategoryConfigs', () => {
  const testFilterGroup1Item1 = new CategoryFilter({
    category: 'Filter1',
    group: 'Filter group 1',
    label: 'Filter group 1 item 1',
    value: 'filter-group-1-item-1',
  });
  const testFilterGroup1Item2 = new CategoryFilter({
    category: 'Filter1',
    group: 'Filter group 1',
    label: 'Filter group 1 item 2',
    value: 'filter-group-1-item-2',
  });
  const testFilterGroup2Item1 = new CategoryFilter({
    category: 'Filter1',
    group: 'Filter group 2',
    label: 'Filter group 2 item 1',
    value: 'filter-group-2-item-1',
  });
  const testFilterGroup2Item2 = new CategoryFilter({
    category: 'Filter1',
    group: 'Filter group 2',
    label: 'Filter group 2 item 2',
    value: 'filter-group-2-item-2',
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
        options: [
          testFilterGroup1Item1,
          testFilterGroup1Item2,
          testFilterGroup2Item1,
          testFilterGroup2Item2,
        ],
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

  const testDataSet2: DataSet = {
    filters: [testFilterGroup1Item2.id],
    indicator: testIndicator1.id,
    timePeriod: testTimePeriod1.id,
    location: {
      level: testLocation1.level,
      value: testLocation1.value,
    },
  };

  const testDataSet3: DataSet = {
    filters: [testFilterGroup2Item1.id],
    indicator: testIndicator1.id,
    timePeriod: testTimePeriod1.id,
    location: {
      level: testLocation1.level,
      value: testLocation1.value,
    },
  };

  const testDataSet4: DataSet = {
    filters: [testFilterGroup2Item2.id],
    indicator: testIndicator1.id,
    timePeriod: testTimePeriod1.id,
    location: {
      level: testLocation1.level,
      value: testLocation1.value,
    },
  };

  const testDataGrouping: DataGroupingConfig = {
    customGroups: [],
    numberOfGroups: 5,
    type: 'EqualIntervals',
  };

  test('returns configs correctly when grouped by time period', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, Location 1)',
          colour: '#F46A25',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, Location 1)',
          colour: '#801650',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1)',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns configs correctly when grouped by location', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testLocation1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testLocation1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testLocation1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testLocation1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testLocation1,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, 2020/21)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testLocation1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(generateDataSetKey(testDataSet1, testLocation1)),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, 2020/21)',
          colour: '#F46A25',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testLocation1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(generateDataSetKey(testDataSet2, testLocation1)),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, 2020/21)',
          colour: '#801650',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testLocation1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(generateDataSetKey(testDataSet3, testLocation1)),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, 2020/21)',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testLocation1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(generateDataSetKey(testDataSet4, testLocation1)),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns configs correctly when grouped by indicator', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testIndicator1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testIndicator1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testIndicator1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testIndicator1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testIndicator1,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Filter group 1 item 1, Location 1, 2020/21',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testIndicator1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testIndicator1),
        ),
      },
      {
        config: {
          label: 'Filter group 1 item 2, Location 1, 2020/21',
          colour: '#F46A25',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testIndicator1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testIndicator1),
        ),
      },
      {
        config: {
          label: 'Filter group 2 item 1, Location 1, 2020/21',
          colour: '#801650',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testIndicator1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testIndicator1),
        ),
      },
      {
        config: {
          label: 'Filter group 2 item 2, Location 1, 2020/21',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testIndicator1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testIndicator1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns configs correctly when grouped by filters without filter groups', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testFilterGroup1Item1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
        },
        filter: testFilterGroup1Item1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet2, testFilterGroup1Item2)]: {
            dataSet: testDataSet2,
            value: 70,
          },
        },
        filter: testFilterGroup1Item2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet3, testFilterGroup2Item1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
        },
        filter: testFilterGroup2Item1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet4, testFilterGroup2Item2)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testFilterGroup2Item2,
      },
    ];
    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Location 1, 2020/21)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testFilterGroup1Item1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testFilterGroup1Item1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns configs correctly when grouped by filters and filter groups', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
        },
        filter: testFilterGroup1Item1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet2)]: {
            dataSet: testDataSet2,
            value: 70,
          },
        },
        filter: testFilterGroup1Item2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet3)]: {
            dataSet: testDataSet3,
            value: 60,
          },
        },
        filter: testFilterGroup2Item1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet4)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testFilterGroup2Item2,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      groupByFilterGroups: true,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1, 2020/21)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: testDataSet1,
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, Location 1, 2020/21)',
          colour: '#F46A25',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: testDataSet2,
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, Location 1, 2020/21)',
          colour: '#801650',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: testDataSet3,
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1, 2020/21)',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: testDataSet4,
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns config from existing legend items if present', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const testLegendItems: LegendItem[] = [
      {
        colour: '#28A197',
        dataSet: omit(testDataSet1, 'timePeriod'),
        label: 'Label 1',
      },
      {
        colour: '#6BACE6',
        dataSet: omit(testDataSet2, 'timePeriod'),
        label: 'Label 2',
      },
      {
        colour: '#28A197',
        dataSet: omit(testDataSet3, 'timePeriod'),
        label: 'Label 3',
      },
      {
        colour: '#6BACE6',
        dataSet: omit(testDataSet4, 'timePeriod'),
        label: 'Label 4',
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: testLegendItems,
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Label 1',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Label 2',
          colour: '#6BACE6',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Label 3',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Label 4',
          colour: '#6BACE6',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns config from data sets if present and there is no legend config', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: {
              ...testDataSet1,
              config: {
                colour: '#28A197',
                label: 'Label 1',
              },
            },
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: {
              ...testDataSet2,
              config: {
                colour: '#6BACE6',
                label: 'Label 2',
              },
            },
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: {
              ...testDataSet3,
              config: {
                colour: '#28A197',
                label: 'Label 3',
              },
            },
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: {
              ...testDataSet4,
              config: {
                colour: '#6BACE6',
                label: 'Label 4',
              },
            },
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Label 1',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Label 2',
          colour: '#6BACE6',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Label 3',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Label 4',
          colour: '#6BACE6',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the default config is there is no legend or data set config', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, Location 1)',
          colour: '#F46A25',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, Location 1)',
          colour: '#801650',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1)',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the deprecated data grouping if present', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const testDeprecatedDataGrouping: DataGroupingConfig = {
      customGroups: [],
      numberOfGroups: 7,
      type: 'Quantiles',
    };

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      deprecatedDataClassification: 'Quantiles',
      deprecatedDataGroups: 7,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: testDeprecatedDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, Location 1)',
          colour: '#F46A25',
        },
        dataGrouping: testDeprecatedDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, Location 1)',
          colour: '#801650',
        },
        dataGrouping: testDeprecatedDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1)',
          colour: '#28A197',
        },
        dataGrouping: testDeprecatedDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the data set data grouping if present', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const testDataSetConfigs: MapDataSetConfig[] = [
      {
        dataSet: omit(testDataSet1, 'timePeriod'),
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 2,
          type: 'EqualIntervals',
        },
      },
      {
        dataSet: omit(testDataSet2, 'timePeriod'),
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 9,
          type: 'Quantiles',
        },
      },
      {
        dataSet: omit(testDataSet3, 'timePeriod'),
        dataGrouping: {
          customGroups: [{ min: 0, max: 10 }],
          numberOfGroups: 9,
          type: 'Custom',
        },
      },
      {
        dataSet: omit(testDataSet4, 'timePeriod'),
        dataGrouping: {
          customGroups: [
            { min: 0, max: 10 },
            { min: 11, max: 20 },
          ],
          numberOfGroups: 9,
          type: 'Custom',
        },
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      dataSetConfigs: testDataSetConfigs,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 2,
          type: 'EqualIntervals',
        },
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, Location 1)',
          colour: '#F46A25',
        },
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 9,
          type: 'Quantiles',
        },
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, Location 1)',
          colour: '#801650',
        },
        dataGrouping: {
          customGroups: [{ min: 0, max: 10 }],
          numberOfGroups: 9,
          type: 'Custom',
        },
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1)',
          colour: '#28A197',
        },
        dataGrouping: {
          customGroups: [
            { min: 0, max: 10 },
            { min: 11, max: 20 },
          ],
          numberOfGroups: 9,
          type: 'Custom',
        },
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });

  test('returns the default data grouping if no deprecated or data set data grouping present', () => {
    const testDataSetCategories: DataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 30,
          },
          [generateDataSetKey(testDataSet2, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 70,
          },
          [generateDataSetKey(testDataSet3, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 60,
          },
          [generateDataSetKey(testDataSet4, testTimePeriod1)]: {
            dataSet: testDataSet4,
            value: 40,
          },
        },
        filter: testTimePeriod1,
      },
    ];

    const result = getDataSetCategoryConfigs({
      dataSetCategories: testDataSetCategories,
      legendItems: [],
      meta: testSubjectMeta,
    });

    const expected: DataSetCategoryConfig[] = [
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 1, Location 1)',
          colour: '#12436D',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 1 item 2, Location 1)',
          colour: '#F46A25',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet2, testTimePeriod1),
        dataSet: expandDataSet(testDataSet2, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet2, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 1, Location 1)',
          colour: '#801650',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet3, testTimePeriod1),
        dataSet: expandDataSet(testDataSet3, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet3, testTimePeriod1),
        ),
      },
      {
        config: {
          label: 'Indicator 1 (Filter group 2 item 2, Location 1)',
          colour: '#28A197',
        },
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet4, testTimePeriod1),
        dataSet: expandDataSet(testDataSet4, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet4, testTimePeriod1),
        ),
      },
    ];

    expect(result).toEqual(expected);
  });
});
