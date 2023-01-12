import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '../../types/filters';
import { FullTable } from '../../types/fullTable';
import { TableHeadersConfig } from '../../types/tableHeaders';

const category1Filter1 = new CategoryFilter({
  value: 'category-1-filter-1',
  label: 'Total',
  group: 'Total',
  isTotal: true,
  category: 'Category1',
});

const category1Filter2 = new CategoryFilter({
  value: 'category-1-filter-2',
  label: 'Category 1 Filter 2',
  group: 'Group 1',
  isTotal: false,
  category: 'Category1',
});

const category1Filter3 = new CategoryFilter({
  value: 'category-1-filter-3',
  label: 'Category 1 Filter 3',
  group: 'Group 1',
  isTotal: false,
  category: 'Category1',
});

const category2Filter1 = new CategoryFilter({
  value: 'category-2-filter-1',
  label: 'Category 2 Filter 1',
  group: 'Default',
  isTotal: false,
  category: 'Category2',
});

const category2Filter2 = new CategoryFilter({
  value: 'category-2-filter-2',
  label: 'Category 2 Filter 2',
  group: 'Default',
  isTotal: false,
  category: 'Category2',
});

const indicator1 = new Indicator({
  value: 'indicator-1',
  label: 'Indicator 1',
  unit: '',
  name: 'indicator_1',
});

const indicator2 = new Indicator({
  value: 'indicator-2',
  label: 'Indicator 2',
  unit: '',
  name: 'indicator_2',
});

const location1 = new LocationFilter({
  value: 'la-1',
  label: 'LA 1',
  group: 'Region 1',
  level: 'localAuthority',
});

const location2 = new LocationFilter({
  value: 'la-2',
  label: 'LA 2',
  group: 'Region 1',
  level: 'localAuthority',
});

const location3 = new LocationFilter({
  value: 'la-3',
  label: 'LA 3',
  group: 'Region 2',
  level: 'localAuthority',
});

const location4 = new LocationFilter({
  value: 'la-4',
  label: 'LA 4',
  group: 'Region 2',
  level: 'localAuthority',
});

const timePeriod1 = new TimePeriodFilter({
  label: '2012/13',
  year: 2012,
  code: 'AY',
  order: 0,
});

const timePeriod2 = new TimePeriodFilter({
  label: '2013/14',
  year: 2013,
  code: 'AY',
  order: 1,
});

const initialTableSubjectMeta: FullTable['subjectMeta'] = {
  filters: {},
  footnotes: [],
  indicators: [],
  locations: [],
  boundaryLevels: [],
  publicationName: 'Publication name',
  subjectName: 'Subject name',
  timePeriodRange: [],
  geoJsonAvailable: true,
};

export const testTableWithTwoLevelsOfRowAndColHeaders: FullTable = {
  subjectMeta: {
    ...initialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Filter1],
        order: 0,
      },
      Category2: {
        name: 'category_1',
        options: [category2Filter1, category2Filter2],
        order: 1,
      },
    },
    indicators: [indicator1, indicator2],
    locations: [location1, location2],
    timePeriodRange: [timePeriod1, timePeriod2],
  },
  results: [
    {
      filters: ['category-2-filter-1', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '2763',
        'indicator-2': '2817',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '7697',
        'indicator-2': '3859',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '21584',
        'indicator-2': '4322',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '76',
        'indicator-2': '378',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '103464',
        'indicator-2': '26396',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '35891',
        'indicator-2': '17018',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '331',
        'indicator-2': '446',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '1327',
        'indicator-2': '2016',
      },
      timePeriod: '2013_AY',
    },
  ],
};

export const testTableWithTwoLevelsOfRowAndColHeadersConfig: TableHeadersConfig = {
  columnGroups: [[location1, location2]],
  rowGroups: [[category2Filter1, category2Filter2]],
  columns: [timePeriod1, timePeriod2],
  rows: [indicator1, indicator2],
};

export const testTableWithThreeLevelsOfRowAndColHeaders: FullTable = {
  subjectMeta: {
    ...initialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Filter2, category1Filter3],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Filter1, category2Filter2],
        order: 1,
      },
    },
    indicators: [indicator1, indicator2],
    locations: [location1, location2, location3, location4],
    timePeriodRange: [timePeriod1, timePeriod2],
  },
  results: [
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '34012',
        'indicator-2': '16024',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '95',
        'indicator-2': '194',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '20',
        'indicator-2': '14',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '7163',
        'indicator-2': '3413',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '767',
        'indicator-2': '818',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '2608',
        'indicator-2': '955',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '19243',
        'indicator-2': '7604',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '6402',
        'indicator-2': '5014',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '19340',
        'indicator-2': '3830',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '212',
        'indicator-2': '231',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '42',
        'indicator-2': '97',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '1804',
        'indicator-2': '5011',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '44',
        'indicator-2': '368',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '32',
        'indicator-2': '6',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '2362',
        'indicator-2': '1811',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '366',
        'indicator-2': '227',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '639',
        'indicator-2': '294',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '2587',
        'indicator-2': '2714',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '727',
        'indicator-2': '1059',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '4872',
        'indicator-2': '1801',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '4248',
        'indicator-2': '6712',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '734',
        'indicator-2': '286',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '99656',
        'indicator-2': '25240',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '331',
        'indicator-2': '428',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-1',
      measures: {
        'indicator-1': '2458',
        'indicator-2': '567',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '20',
        'indicator-2': '24',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '90',
        'indicator-2': '252',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-1', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '1285',
        'indicator-2': '1933',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-4',
      measures: {
        'indicator-1': '56',
        'indicator-2': '17',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '2631',
        'indicator-2': '6095',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['category-2-filter-2', 'category-1-filter-2'],
      geographicLevel: 'localAuthority',
      locationId: 'la-3',
      measures: {
        'indicator-1': '15470',
        'indicator-2': '6301',
      },
      timePeriod: '2012_AY',
    },
    {
      filters: ['category-1-filter-3', 'category-2-filter-1'],
      geographicLevel: 'localAuthority',
      locationId: 'la-2',
      measures: {
        'indicator-1': '122',
        'indicator-2': '127',
      },
      timePeriod: '2012_AY',
    },
  ],
};

export const testTableWithThreeLevelsOfRowAndColHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1, timePeriod2],
  columnGroups: [
    [category2Filter1, category2Filter2],
    [category1Filter2, category1Filter3],
  ],
  rows: [indicator1, indicator2],
  rowGroups: [[location1, location2, location3, location4]],
};
