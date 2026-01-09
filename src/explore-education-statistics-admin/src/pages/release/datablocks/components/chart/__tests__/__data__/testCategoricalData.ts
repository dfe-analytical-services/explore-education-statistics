import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { testGeoJsonFeature } from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';

export const testCategoricalMeta: FullTableMeta = {
  filters: {},
  footnotes: [],
  indicators: [
    new Indicator({
      label: 'Indicator 1',
      name: 'indicator-1-name',
      unit: '',
      value: 'indicator-1',
    }),
    new Indicator({
      label: 'Indicator 2',
      name: 'indicator-2-name',
      unit: '',
      value: 'indicator-2',
    }),
  ],
  locations: [
    new LocationFilter({
      value: 'location-1-value',
      label: 'Location 1',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-2-value',
      label: 'Location 2',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-3-value',
      label: 'Location 3',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-4-value',
      label: 'Location 4',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),

    new LocationFilter({
      value: 'location-5-value',
      label: 'Location 5',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),

    new LocationFilter({
      value: 'location-6-value',
      label: 'Location 6',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-7-value',
      label: 'Location 7',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-8-value',
      label: 'Location 8',
      level: 'region',
      geoJson: testGeoJsonFeature,
    }),
    new LocationFilter({
      value: 'location-9-value',
      label: 'Location 9',
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
  publicationName: 'Publication 1',
  subjectName: 'Subject 1',
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

export const testCategoricalData: TableDataResult[] = [
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-1',
    measures: {
      'indicator-2': 'large',
      'indicator-1': 'high',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-6-value',
    measures: {
      'indicator-2': 'medium',
      'indicator-1': 'low',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-7-value',
    measures: {
      'indicator-2': 'small',
      'indicator-1': 'low',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-1-value',
    measures: {
      'indicator-2': 'small',
      'indicator-1': 'low',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-2-value',
    measures: {
      'indicator-2': 'large',
      'indicator-1': 'high',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-8-value',
    measures: {
      'indicator-2': 'small',
      'indicator-1': 'high',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-9-value',
    measures: {
      'indicator-2': 'medium',
      'indicator-1': 'high',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-5-value',
    measures: {
      'indicator-2': 'medium',
      'indicator-1': 'low',
    },
    timePeriod: '2024_CY',
  },
  {
    filters: [],
    geographicLevel: 'region',
    locationId: 'location-3-value',
    measures: {
      'indicator-2': 'large',
      'indicator-1': 'high',
    },
    timePeriod: '2024_CY',
  },
];

export const testMapDataSetConfigs: MapDataSetConfig[] = [
  {
    categoricalDataConfig: [
      {
        value: 'high',
        colour: '#28A197',
      },
      {
        value: 'low',
        colour: '#12436D',
      },
    ],
    dataSet: {
      filters: [],
      indicator: 'indicator-1',
      timePeriod: '2024_CY',
    },
    dataSetKey:
      '{"filters":[],"indicator":"indicator-1","timePeriod":"2024_CY"}',
    dataGrouping: {
      type: 'EqualIntervals',
      numberOfGroups: 5,
      customGroups: [],
    },
  },
  {
    categoricalDataConfig: [
      {
        value: 'large',
        colour: '#28A197',
      },
      {
        value: 'medium',
        colour: '#801650',
      },
      {
        value: 'small',
        colour: '#12436D',
      },
    ],
    dataSet: {
      filters: [],
      indicator: 'indicator-2',
      timePeriod: '2024_CY',
    },
    dataSetKey:
      '{"filters":[],"indicator":"indicator-2","timePeriod":"2024_CY"}',
    dataGrouping: {
      type: 'EqualIntervals',
      numberOfGroups: 5,
      customGroups: [],
    },
  },
];
