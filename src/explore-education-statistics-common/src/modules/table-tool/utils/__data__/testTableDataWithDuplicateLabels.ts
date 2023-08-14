import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { testInitialTableSubjectMeta } from '@common/modules/table-tool/utils/__data__/testTableData';

const indicator1 = new Indicator({
  value: 'indicator-1',
  label: 'Indicator 1',
  unit: '',
  name: 'indicator_1',
});

const location1 = new LocationFilter({
  value: 'location-1',
  label: 'Location 1',
  level: 'country',
});

const timePeriod1 = new TimePeriodFilter({
  label: '2012/13',
  year: 2012,
  code: 'AY',
  order: 0,
});

const category1Filter1 = new CategoryFilter({
  value: 'category1_filter1',
  label: 'Filter 1',
  group: 'Default',
  isTotal: false,
  category: 'Category 1',
});

const category1Filter2 = new CategoryFilter({
  value: 'category1_filter2',
  label: 'Filter 2',
  group: 'Default',
  isTotal: false,
  category: 'Category 1',
});

// Category 2 filters have the same labels as Category 1
const category2Filter1 = new CategoryFilter({
  value: 'category2_filter1',
  label: 'Filter 1',
  group: 'Default',
  isTotal: false,
  category: 'Category 2',
});

const category2Filter2 = new CategoryFilter({
  value: 'category2_filter2',
  label: 'Filter 2',
  group: 'Default',
  isTotal: false,
  category: 'Category 2',
});

// Category 3 and 4 both contain the same group labels (Group 1 and Group 2)
const category3Group1Filter1 = new CategoryFilter({
  value: 'category3_group1_filter1',
  label: 'Category 3 Group 1 Filter 1',
  group: 'Group 1',
  isTotal: false,
  category: 'Category 3',
});

const category3Group2Filter1 = new CategoryFilter({
  value: 'category3_group2_filter1',
  label: 'Category 3 Group 2 Filter 1',
  group: 'Group 2',
  isTotal: false,
  category: 'Category 3',
});

const category4Group1Filter1 = new CategoryFilter({
  value: 'category4_group1_filter1',
  label: 'Category 4 Group 1 Filter 1',
  group: 'Group 1',
  isTotal: false,
  category: 'Category 4',
});

const category4Group2Filter1 = new CategoryFilter({
  value: 'category4_group2_filter1',
  label: 'Category 4 Group 2 Filter 1',
  group: 'Group 2',
  isTotal: false,
  category: 'Category 4',
});

// Filter 1 and 2 in Category 1 have the same labels as Filter 1 and 2 in Category 2
export const testTableWithDuplicateFilterLabelsInColumnHeadersConfig: TableHeadersConfig =
  {
    columns: [category2Filter1, category2Filter2],
    columnGroups: [[indicator1], [category1Filter1, category1Filter2]],
    rows: [timePeriod1],
    rowGroups: [],
  };

export const testTableWithDuplicateFilterLabelsInRowHeadersConfig: TableHeadersConfig =
  {
    columns: [timePeriod1],
    columnGroups: [],
    rows: [category2Filter1, category2Filter2],
    rowGroups: [[indicator1], [category1Filter1, category1Filter2]],
  };

export const testTableWithDuplicateFilterLabels: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Filter1, category1Filter2],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Filter1, category2Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Filter1.id, category2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter1.id, category2Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '88',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter2.id, category2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '89',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter2.id, category2Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '90',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

// Only Filter 1 in Category 1 and Filter 2 in Category 2 have data. This causes
// columns / rows to be excluded for Category 1 - Filter 2 and Category 2 Filter 1.
export const testTableWithDuplicateFilterLabelsAndMissingData: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Filter1, category1Filter2],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Filter1, category2Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Filter1.id, category2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Filter2.id, category2Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '90',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithMultipleGroupsWithSameLabelsInColumnHeadersConfig: TableHeadersConfig =
  {
    columnGroups: [
      [category3Group1Filter1, category3Group2Filter1],
      [category4Group1Filter1, category4Group2Filter1],
    ],
    columns: [timePeriod1],
    rowGroups: [],
    rows: [indicator1],
  };

export const testTableWithMultipleGroupsWithSameLabelsInRowHeadersConfig: TableHeadersConfig =
  {
    columnGroups: [],
    columns: [indicator1],
    rowGroups: [
      [category3Group1Filter1, category3Group2Filter1],
      [category4Group1Filter1, category4Group2Filter1],
    ],
    rows: [timePeriod1],
  };

export const testTableWithMultipleGroupsWithSameLabels: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category3: {
        name: 'category_3',
        options: [category3Group1Filter1, category3Group2Filter1],
        order: 1,
      },
      Category4: {
        name: 'category_4',
        options: [category4Group1Filter1, category4Group2Filter1],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category4Group1Filter1.id, category3Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '20',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category4Group1Filter1.id, category3Group2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '71',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category4Group2Filter1.id, category3Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '44',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category4Group2Filter1.id, category3Group2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '32',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};
