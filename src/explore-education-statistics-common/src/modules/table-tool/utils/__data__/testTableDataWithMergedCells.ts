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

// Label and group are the same
const category1Group1Filter1 = new CategoryFilter({
  value: 'category1_group1_filter1',
  label: 'Category 1 Group 1',
  group: 'Category 1 Group 1',
  isTotal: false,
  category: 'Category 1',
});

const category1Group2Filter1 = new CategoryFilter({
  value: 'category1_group2_filter1',
  label: 'Category 1 Group 2 Filter 1',
  group: 'Category 1 Group 2',
  isTotal: false,
  category: 'Category 1',
});

// Label and group are the same
const category1Group2Filter2 = new CategoryFilter({
  value: 'category1_group2_filter2',
  label: 'Category 1 Group 2',
  group: 'Category 1 Group 2',
  isTotal: false,
  category: 'Category 1',
});

const category2Group1Filter1 = new CategoryFilter({
  value: 'category2_group1_filter1',
  label: 'Category 2 Group 1 Filter 1',
  group: 'Group 1',
  isTotal: false,
  category: 'Category 2',
});

const category2Group1Filter2 = new CategoryFilter({
  value: 'category2_group1_filter2',
  label: 'Category 2 Group 1 Filter 2',
  group: 'Group 1',
  isTotal: false,
  category: 'Category 2',
});

export const testTableWithOnlyMergedCellsInColumnHeadersConfig: TableHeadersConfig = {
  columns: [category1Group1Filter1, category1Group2Filter2],
  columnGroups: [],
  rows: [timePeriod1],
  rowGroups: [[indicator1]],
};

export const testTableWithOnlyMergedCellsInRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [indicator1],
  rowGroups: [[category1Group1Filter1, category1Group2Filter2]],
};

export const testTableWithOnlyMergedCellsInHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group2Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '74',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithMergedAndUnMergedCellsInColumnHeadersConfig: TableHeadersConfig = {
  columns: [category1Group1Filter1, category1Group2Filter1],
  columnGroups: [],
  rows: [timePeriod1],
  rowGroups: [[indicator1]],
};

export const testTableWithMergedAndUnmergedCellsInRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [indicator1],
  rowGroups: [[category1Group1Filter1, category1Group2Filter1]],
};

export const testTableWithMergedAndUnMergedCellsInHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter1],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '95',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithOnlyMergedCellsInFirstLevelOfColumnHeadersConfig: TableHeadersConfig = {
  columns: [category2Group1Filter1, category2Group1Filter2],
  columnGroups: [[category1Group1Filter1, category1Group2Filter2]],
  rows: [indicator1],
  rowGroups: [[timePeriod1]],
};

export const testTableWithOnlyMergedCellsInFirstLevelOfRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [indicator1],
  rowGroups: [
    [category1Group1Filter1, category1Group2Filter2],
    [category2Group1Filter1, category2Group1Filter2],
  ],
};

export const testTableWithOnlyMergedCellsInFirstLevelOfHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter2],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Group1Filter1, category2Group1Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group1Filter1.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '74',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group1Filter1.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter2.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '92',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter2.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '87',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithMergedAndUnmergedCellsInFirstLevelOfColumnHeadersConfig: TableHeadersConfig = {
  columns: [category2Group1Filter1, category2Group1Filter2],
  columnGroups: [[category1Group1Filter1, category1Group2Filter1]],
  rows: [indicator1],
  rowGroups: [[timePeriod1]],
};

export const testTableWithMergedAndUnmergedCellsInFirstLevelOfRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [indicator1],
  rowGroups: [
    [category1Group1Filter1, category1Group2Filter1],
    [category2Group1Filter1, category2Group1Filter2],
  ],
};

export const testTableWithMergedAndUnmergedCellsInFirstLevelOfHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter1],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Group1Filter1, category2Group1Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group1Filter1.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '74',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group1Filter1.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter1.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '92',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter1.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '87',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithOnlyMergedCellsInMiddleLevelOfColumnHeadersConfig: TableHeadersConfig = {
  columns: [category2Group1Filter1, category2Group1Filter2],
  columnGroups: [
    [indicator1],
    [category1Group1Filter1, category1Group2Filter2],
  ],
  rows: [timePeriod1],
  rowGroups: [],
};

export const testTableWithOnlyMergedCellsInMiddleLevelOfRowsConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [category2Group1Filter1, category2Group1Filter2],
  rowGroups: [[indicator1], [category1Group1Filter1, category1Group2Filter2]],
};

export const testTableWithOnlyMergedCellsInMiddleLevelOfHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter2],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Group1Filter1, category2Group1Filter2],
        order: 1,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group2Filter2.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '73',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter2.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '66',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Group1Filter1.id, category1Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '75',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Group1Filter2.id, category1Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '70',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithMergedAndUnmergedCellsInMiddleLevelOfColumnHeadersConfig: TableHeadersConfig = {
  columns: [category2Group1Filter1, category2Group1Filter2],
  columnGroups: [
    [indicator1],
    [category1Group1Filter1, category1Group2Filter1, category1Group2Filter2],
  ],
  rows: [timePeriod1],
  rowGroups: [],
};

export const testTableWithMergedAndUnmergedCellsInMiddleLevelOfRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [category2Group1Filter1, category2Group1Filter2],
  rowGroups: [[indicator1], [category1Group1Filter1, category1Group2Filter1]],
};

export const testTableWithMergedAndUnmergedCellsInMiddleLevelOfHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [
          category1Group1Filter1,
          category1Group2Filter1,
          category1Group2Filter2,
        ],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Group1Filter1, category2Group1Filter2],
        order: 1,
      },
    },

    indicators: [indicator1],
    locations: [location1],

    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group2Filter2.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '79',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter2.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '74',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Group1Filter1.id, category1Group2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '87',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Group1Filter2.id, category1Group2Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Group1Filter1.id, category1Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '88',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category2Group1Filter2.id, category1Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithOnlyMergedCellsInLastLevelOfColumnHeadersConfig: TableHeadersConfig = {
  columns: [category1Group1Filter1, category1Group2Filter2],
  columnGroups: [
    [indicator1],
    [category2Group1Filter1, category2Group1Filter2],
  ],
  rows: [timePeriod1],
  rowGroups: [],
};

export const testTableWithOnlyMergedCellsInLastLevelOfRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [category1Group1Filter1, category1Group2Filter2],
  rowGroups: [[indicator1], [category2Group1Filter1, category2Group1Filter2]],
};

export const testTableWithOnlyMergedCellsInLastLevelOfHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter2],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Group1Filter1, category2Group1Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group1Filter1.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '74',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group1Filter1.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter2.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '92',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter2.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '87',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};

export const testTableWithMergedAndUnmergedCellsInLastLevelOfColumnHeadersConfig: TableHeadersConfig = {
  columns: [category1Group1Filter1, category1Group2Filter1],
  columnGroups: [
    [indicator1],
    [category2Group1Filter1, category2Group1Filter2],
  ],
  rows: [timePeriod1],
  rowGroups: [],
};

export const testTableWithMergedAndUnmergedCellsInLastLevelOfRowHeadersConfig: TableHeadersConfig = {
  columns: [timePeriod1],
  columnGroups: [],
  rows: [category1Group1Filter1, category1Group2Filter1],
  rowGroups: [[indicator1], [category2Group1Filter1, category2Group1Filter2]],
};

export const testTableWithMergedAndUnmergedCellsInLastLevelOfHeaders: FullTable = {
  subjectMeta: {
    ...testInitialTableSubjectMeta,
    filters: {
      Category1: {
        name: 'category_1',
        options: [category1Group1Filter1, category1Group2Filter1],
        order: 0,
      },
      Category2: {
        name: 'category_2',
        options: [category2Group1Filter1, category2Group1Filter2],
        order: 0,
      },
    },
    indicators: [indicator1],
    locations: [location1],
    timePeriodRange: [timePeriod1],
  },
  results: [
    {
      filters: [category1Group1Filter1.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '74',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group1Filter1.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '85',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter1.id, category2Group1Filter1.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '92',
      },
      timePeriod: timePeriod1.id,
    },
    {
      filters: [category1Group2Filter1.id, category2Group1Filter2.id],
      geographicLevel: 'country',
      locationId: location1.value,
      measures: {
        [indicator1.id]: '87',
      },
      timePeriod: timePeriod1.id,
    },
  ],
};
