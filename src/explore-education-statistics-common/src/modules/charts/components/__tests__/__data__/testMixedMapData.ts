import { Chart } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { testGeoJsonFeature } from './testMapBlockData';

export const testMixedDataAxisDataSets: DataSet[] = [
  {
    filters: [],
    indicator: 'numerical-indicator',
    location: {
      level: 'region',
      value: 'location-1',
    },
    timePeriod: '2024_CY',
    order: 0,
  },
  {
    filters: [],
    indicator: 'numerical-indicator',
    location: {
      level: 'region',
      value: 'location-2',
    },
    timePeriod: '2024_CY',
    order: 1,
  },
  {
    filters: [],
    indicator: 'categorical-indicator',
    location: {
      level: 'region',
      value: 'location-3',
    },
    timePeriod: '2024_CY',
    order: 2,
  },
];

export const testMixedData: TableDataResult[] = [
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-1',
    measures: {
      'categorical-indicator': 'small',
      'numerical-indicator': '400',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-2',
    measures: {
      'categorical-indicator': 'large',
      'numerical-indicator': '500',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-3',
    measures: {
      'categorical-indicator': 'large',
      'numerical-indicator': '100',
    },
    timePeriod: '2024_CY',
  },
];

export const testMixedMeta: FullTableMeta = {
  filters: {},
  footnotes: [],
  indicators: [
    new Indicator({
      value: 'numerical-indicator',
      label: 'Numerical indicator',
      unit: '',
      name: 'numerical-indicator-name',
    }),
    new Indicator({
      value: 'categorical-indicator',
      label: 'Categorical indicator',
      unit: '',
      name: 'categorical-indicator-name',
    }),
  ],
  locations: [
    new LocationFilter({
      value: 'location-1',
      label: 'Location 1',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-2',
      label: 'Location 2',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-3',
      label: 'Location 3',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
  ],
  boundaryLevels: [
    {
      id: 15,
      label: 'Regions England BUC 2022/12',
    },
    {
      id: 6,
      label: 'Regions England BUC 2017/12',
    },
  ],
  publicationName: 'Categorical data maps',
  subjectName: 'Mixed',
  timePeriodRange: [
    new TimePeriodFilter({
      year: 2024,
      code: 'CY',
      label: '2024',
      order: 0,
    }),
  ],
  geoJsonAvailable: true,
};

export const testMixedMapConfiguration: Chart = {
  type: 'map',
  boundaryLevel: 1,
  axes: {
    major: {
      type: 'major',
      groupBy: 'locations',
      sortBy: 'name',
      sortAsc: true,
      dataSets: testMixedDataAxisDataSets,
      referenceLines: [],
      visible: true,
      unit: '',
      showGrid: true,
      min: 0,
      size: 50,
      tickConfig: 'default',
      tickSpacing: 1,
    },
  },
  legend: {
    position: 'none',
    items: [
      {
        label: 'Numerical indicator (2024)',
        colour: '#12436D',
        dataSet: {
          filters: [],
          indicator: 'numerical-indicator',
          timePeriod: '2024_CY',
        },
      },
      {
        label: 'Categorical indicator (2024)',
        colour: '#801650',
        dataSet: {
          filters: [],
          indicator: 'categorical-indicator',
          timePeriod: '2024_CY',
        },
      },
    ],
  },
  title: '',
  alt: '',
  height: 600,
  map: {
    dataSetConfigs: [
      {
        dataSet: {
          filters: [],
          indicator: 'numerical-indicator',
          timePeriod: '2024_CY',
        },
        dataSetKey:
          '{"filters":[],"indicator":"numerical-indicator","timePeriod":"2024_CY"}',
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
      },
      {
        categoricalDataConfig: [
          {
            colour: '#12436D',
            value: 'large',
          },
        ],
        dataSet: {
          filters: [],
          indicator: 'categorical-indicator',
          timePeriod: '2024_CY',
        },
        dataSetKey:
          '{"filters":[],"indicator":"categorical-indicator","timePeriod":"2024_CY"}',
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
      },
    ],
  },
};
