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

const timePeriod3 = new TimePeriodFilter({
  label: '2014/15',
  year: 2014,
  code: 'AY',
  order: 2,
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

const testSubjectMeta1: FullTable['subjectMeta'] = {
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
  timePeriodRange: [timePeriod1],
};

export const testTableWithOneLevelOfRowAndColHeaders: FullTable = {
  subjectMeta: testSubjectMeta1,
  results: [
    {
      filters: [category2Filter1.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '2763',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter1.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '346',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithMissingTimePeriod: FullTable = {
  ...testTableWithOneLevelOfRowAndColHeaders,
  subjectMeta: {
    ...testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
    timePeriodRange: [timePeriod1, timePeriod2],
  },
};

export const testTableWithOneLevelOfRowAndColHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [indicator1],
  rowGroups: [[location1, location2]],
};

export const testTableWithTwoLevelsOfRowAndOneLevelOfColHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [location1, location2],
  rowGroups: [[indicator1]],
};

export const testTableWithOneLevelOfRowsAndTwoLevelsofColHeadersConfig: TableHeadersConfig = {
  columns: [location1, location2],
  columnGroups: [[timePeriod1]],
  rows: [indicator1],
  rowGroups: [],
};

export const testTableWithTwoLevelsOfRowAndColHeaders: FullTable = {
  subjectMeta: testSubjectMeta1,
  results: [
    {
      filters: [category2Filter1.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '2763',
        [indicator2.id]: '2817',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '7697',
        [indicator2.id]: '3859',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter2.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '21584',
        [indicator2.id]: '4322',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter1.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '76',
        [indicator2.id]: '378',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter2.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '103464',
        [indicator2.id]: '26396',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '35891',
        [indicator2.id]: '17018',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '331',
        [indicator2.id]: '446',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter1.id, category1Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '1327',
        [indicator2.id]: '2016',
      },
      timePeriod: timePeriod2.id,
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
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '34012',
        [indicator2.id]: '16024',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '95',
        [indicator2.id]: '194',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '20',
        [indicator2.id]: '14',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '7163',
        [indicator2.id]: '3413',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '767',
        [indicator2.id]: '818',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '2608',
        [indicator2.id]: '955',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '19243',
        [indicator2.id]: '7604',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '6402',
        [indicator2.id]: '5014',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '19340',
        [indicator2.id]: '3830',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '212',
        [indicator2.id]: '231',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '42',
        [indicator2.id]: '97',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '1804',
        [indicator2.id]: '5011',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '44',
        [indicator2.id]: '368',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '32',
        [indicator2.id]: '6',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '2362',
        [indicator2.id]: '1811',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '366',
        [indicator2.id]: '227',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '639',
        [indicator2.id]: '294',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '2587',
        [indicator2.id]: '2714',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '727',
        [indicator2.id]: '1059',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '4872',
        [indicator2.id]: '1801',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '4248',
        [indicator2.id]: '6712',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '734',
        [indicator2.id]: '286',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '99656',
        [indicator2.id]: '25240',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '331',
        [indicator2.id]: '428',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '2458',
        [indicator2.id]: '567',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '20',
        [indicator2.id]: '24',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '90',
        [indicator2.id]: '252',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter1.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '1285',
        [indicator2.id]: '1933',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location4.value,
      measures: {
        [indicator1.id]: '56',
        [indicator2.id]: '17',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '2631',
        [indicator2.id]: '6095',
      },
      timePeriod: timePeriod2.id,
    },
    {
      filters: [category2Filter2.id, category1Filter2.id],
      geographicLevel: 'localAuthority',
      locationId: location3.value,
      measures: {
        [indicator1.id]: '15470',
        [indicator2.id]: '6301',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter3.id, category2Filter1.id],
      geographicLevel: 'localAuthority',
      locationId: location2.value,
      measures: {
        [indicator1.id]: '122',
        [indicator2.id]: '127',
      },
      timePeriod: timePeriod1.id,
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
