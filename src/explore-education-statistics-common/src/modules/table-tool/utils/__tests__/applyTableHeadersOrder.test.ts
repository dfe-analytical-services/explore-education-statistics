import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import applyTableHeadersOrder from '@common/modules/table-tool/utils/applyTableHeadersOrder';

const testCategoryFilters1 = [
  new CategoryFilter({
    value: 'category-1-value',
    label: 'Category 1',
    group: 'Default',
    isTotal: false,
    category: 'Category group 1',
  }),
  new CategoryFilter({
    value: 'category-2-value',
    label: 'Category 2',
    group: 'Default',
    isTotal: false,
    category: 'Category group 1',
  }),
  new CategoryFilter({
    value: 'category-3-value',
    label: 'Category 3',
    group: 'Default',
    isTotal: false,
    category: 'Category group 1',
  }),
  new CategoryFilter({
    value: 'category-4-value',
    label: 'Category 4',
    group: 'Default',
    isTotal: false,
    category: 'Category group 1',
  }),
  new CategoryFilter({
    value: 'category-5-value',
    label: 'Category 5',
    group: 'Default',
    isTotal: false,
    category: 'Category group 1',
  }),
];
const testCategoryFilters2 = [
  new CategoryFilter({
    value: 'category-1-value',
    label: 'Category 1',
    group: 'Default',
    isTotal: false,
    category: 'Category group 2',
  }),
  new CategoryFilter({
    value: 'category-2-value',
    label: 'Category 2',
    group: 'Default',
    isTotal: false,
    category: 'Category group 2',
  }),
  new CategoryFilter({
    value: 'category-3-value',
    label: 'Category 3',
    group: 'Default',
    isTotal: false,
    category: 'Category group 2',
  }),
  new CategoryFilter({
    value: 'category-4-value',
    label: 'Category 4',
    group: 'Default',
    isTotal: false,
    category: 'Category group 2',
  }),
  new CategoryFilter({
    value: 'category-5-value',
    label: 'Category 5',
    group: 'Default',
    isTotal: false,
    category: 'Category group 2',
  }),
];
const testCategoryFilters3 = [
  new CategoryFilter({
    value: 'category-1-value',
    label: 'Category 1',
    group: 'Default',
    isTotal: false,
    category: 'Category group 3',
  }),
  new CategoryFilter({
    value: 'category-2-value',
    label: 'Category 2',
    group: 'Default',
    isTotal: false,
    category: 'Category group 3',
  }),
  new CategoryFilter({
    value: 'category-3-value',
    label: 'Category 3',
    group: 'Default',
    isTotal: false,
    category: 'Category group 3',
  }),
  new CategoryFilter({
    value: 'category-4-value',
    label: 'Category 4',
    group: 'Default',
    isTotal: false,
    category: 'Category group 3',
  }),
  new CategoryFilter({
    value: 'category-5-value',
    label: 'Category 5',
    group: 'Default',
    isTotal: false,
    category: 'Category group 3',
  }),
];
const testCategoryFilters4 = [
  new CategoryFilter({
    value: 'category-1-value',
    label: 'Category 1',
    group: 'Default',
    isTotal: false,
    category: 'Category group 4',
  }),
  new CategoryFilter({
    value: 'category-2-value',
    label: 'Category 2',
    group: 'Default',
    isTotal: false,
    category: 'Category group 4',
  }),
  new CategoryFilter({
    value: 'category-3-value',
    label: 'Category 3',
    group: 'Default',
    isTotal: false,
    category: 'Category group 4',
  }),
  new CategoryFilter({
    value: 'category-4-value',
    label: 'Category 4',
    group: 'Default',
    isTotal: false,
    category: 'Category group 4',
  }),
  new CategoryFilter({
    value: 'category-5-value',
    label: 'Category 5',
    group: 'Default',
    isTotal: false,
    category: 'Category group 4',
  }),
];
const testCategoryFilters5 = [
  new CategoryFilter({
    value: 'category-1-value',
    label: 'Category 1',
    group: 'Default',
    isTotal: false,
    category: 'Category group 5',
  }),
  new CategoryFilter({
    value: 'category-2-value',
    label: 'Category 2',
    group: 'Default',
    isTotal: false,
    category: 'Category group 5',
  }),
  new CategoryFilter({
    value: 'category-3-value',
    label: 'Category 3',
    group: 'Default',
    isTotal: false,
    category: 'Category group 5',
  }),
  new CategoryFilter({
    value: 'category-4-value',
    label: 'Category 4',
    group: 'Default',
    isTotal: false,
    category: 'Category group 5',
  }),
  new CategoryFilter({
    value: 'category-5-value',
    label: 'Category 5',
    group: 'Default',
    isTotal: false,
    category: 'Category group 5',
  }),
];
const testLocationFilters = [
  new LocationFilter({
    value: 'location-1-value',
    label: 'Location 1',
    level: '',
  }),
  new LocationFilter({
    value: 'location-2-value',
    label: 'Location 2',
    level: '',
  }),
  new LocationFilter({
    value: 'location-3-value',
    label: 'Location 3',
    level: '',
  }),
  new LocationFilter({
    value: 'location-4-value',
    label: 'Location 4',
    level: '',
  }),
  new LocationFilter({
    value: 'location-5-value',
    label: 'Location 5',
    level: '',
  }),
];
const testTimePeriodFilters = [
  new TimePeriodFilter({
    year: 2010,
    code: 'T1',
    label: 'Time period 1',
    order: 0,
  }),
  new TimePeriodFilter({
    year: 2011,
    code: 'T1',
    label: 'Time period 2',
    order: 1,
  }),
  new TimePeriodFilter({
    year: 2012,
    code: 'T1',
    label: 'Time period 3',
    order: 2,
  }),
  new TimePeriodFilter({
    year: 2013,
    code: 'T1',
    label: 'Time period 4',
    order: 3,
  }),
  new TimePeriodFilter({
    year: 2014,
    code: 'T1',
    label: 'Time period 5',
    order: 4,
  }),
];
const testIndicators = [
  new Indicator({
    value: 'indicator-1-value',
    label: 'Indicator 1',
    unit: '',
    name: 'Indicator 1 name',
  }),
  new Indicator({
    value: 'indicator-2-value',
    label: 'Indicator 2',
    unit: '',
    name: 'Indicator 2 name',
  }),
  new Indicator({
    value: 'indicator-3-value',
    label: 'Indicator 3',
    unit: '',
    name: 'Indicator 3 name',
  }),
  new Indicator({
    value: 'indicator-4-value',
    label: 'Indicator 4',
    unit: '',
    name: 'Indicator 4 name',
  }),
  new Indicator({
    value: 'indicator-5-value',
    label: 'Indicator 5',
    unit: '',
    name: 'Indicator 5 name',
  }),
];
const testDefaultBaseHeaders: TableHeadersConfig = {
  columnGroups: [],
  columns: testTimePeriodFilters,
  rowGroups: [],
  rows: testIndicators,
};

describe('applyTableHeadersOrder', () => {
  test('does not change the ordering when the table has not been reordered', () => {
    const testHeadersUnorderedHeaders: TableHeadersConfig = {
      ...testDefaultBaseHeaders,
      columnGroups: [testLocationFilters, testCategoryFilters1],
      rowGroups: [testCategoryFilters2, testCategoryFilters2],
    };
    const result = applyTableHeadersOrder({
      reorderedTableHeaders: testHeadersUnorderedHeaders,
      defaultTableHeaders: testHeadersUnorderedHeaders,
    });
    expect(result).toEqual(testHeadersUnorderedHeaders);
  });

  describe('rows', () => {
    test('retains the order of reordered rows', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[1], testIndicators[0], testIndicators[2]],
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[0], testIndicators[1], testIndicators[2]],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order when rows have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[1], testIndicators[0], testIndicators[2]],
      };
      // testIndicators[2] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[0], testIndicators[1]],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[1], testIndicators[0]],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('adds new rows after reordered rows', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[3], testIndicators[1], testIndicators[4]],
      };
      // testIndicators[0] and testIndicators[2] added
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [
          testIndicators[0],
          testIndicators[1],
          testIndicators[2],
          testIndicators[3],
          testIndicators[4],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [
          testIndicators[3],
          testIndicators[1],
          testIndicators[4],
          testIndicators[0],
          testIndicators[2],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when rows have been added and removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[1], testIndicators[0], testIndicators[2]],
      };
      // testIndicators[3] and testIndicators[4] added
      // testIndicators[2] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [
          testIndicators[0],
          testIndicators[1],
          testIndicators[3],
          testIndicators[4],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [
          testIndicators[1],
          testIndicators[0],
          testIndicators[3],
          testIndicators[4],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('if rows have not been reordered new rows are added in their default positions', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [testIndicators[2], testIndicators[3]],
      };
      // testIndicators[0], testIndicators[1] and testIndicators[4] added
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rows: [
          testIndicators[0],
          testIndicators[1],
          testIndicators[2],
          testIndicators[3],
          testIndicators[4],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testDefaultTableHeaders);
    });

    test('populate rows when the only row group has been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [
          testTimePeriodFilters,
          testCategoryFilters2,
          testLocationFilters,
        ],
        columns: testIndicators,
        rowGroups: [],
        rows: testCategoryFilters1,
      };
      // testCategoryFilters1 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testIndicators,
        rowGroups: [testCategoryFilters2],
        rows: testTimePeriodFilters,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testLocationFilters],
        columns: testIndicators,
        rowGroups: [],
        rows: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });
  });

  describe('rows in rowGroups', () => {
    test('retains the order of rows in rowGroups when rows have not been added or removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[0],
            testCategoryFilters1[1],
            testCategoryFilters1[2],
          ],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order of rows in rowGroups when rows have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      //  testCategoryFilters1[1] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [[testCategoryFilters1[0], testCategoryFilters1[2]]],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [[testCategoryFilters1[2], testCategoryFilters1[0]]],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of rows in rowGroups when rows have been added', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      // testCategoryFilters1[3] and testCategoryFilters1[4] added
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[0],
            testCategoryFilters1[1],
            testCategoryFilters1[2],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of rows in rowGroups when rows have been added and removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      // testCategoryFilters1[3] and testCategoryFilters1[4] added
      // testCategoryFilters1[1] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[0],
            testCategoryFilters1[2],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });
  });

  describe('rowGroups', () => {
    test('retains the order of reordered rowGroups', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        rows: testIndicators,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testLocationFilters,
        ],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order when rowGroups have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          testLocationFilters,
          testCategoryFilters1,
          testCategoryFilters2,
        ],
        rows: testIndicators,
      };
      // testCategoryFilters1 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [testCategoryFilters2, testLocationFilters],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [testLocationFilters, testCategoryFilters2],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });

      expect(result).toEqual(expected);
    });

    test('handles when all rowGroups have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [testCategoryFilters1, testCategoryFilters2, testIndicators],
        rows: testLocationFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [],
        rows: testCategoryFilters2,
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [],
        rows: testCategoryFilters2,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });

      expect(result).toEqual(expected);
    });

    test('retains the order of rowGroups and adds new groups distributed between rows and columns', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        rows: testIndicators,
      };
      // testCategoryFilters3 and testCategoryFilters4 added
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters3,
          testCategoryFilters4,
        ],
        rows: testIndicators,
      };
      // From a user's perspective there isn't a distinction between rows and rowGroups,
      // rows is an internal term for the last of the row groups (same for columns).
      // This means that adding new groups causes rows to change as well as rowGroups,
      // as the group previously in rows will be shifted into rowGroups
      const expected: TableHeadersConfig = {
        columnGroups: [testTimePeriodFilters, testCategoryFilters3],
        columns: testCategoryFilters4,
        rowGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of rowGroups and rows when groups have been added and removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        rows: testIndicators,
      };
      // testCategoryFilters3 and testCategoryFilters4 added
      // testCategoryFilters2 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [
          testCategoryFilters1,
          testLocationFilters,
          testCategoryFilters3,
          testCategoryFilters4,
        ],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testTimePeriodFilters, testCategoryFilters3],
        columns: testCategoryFilters4,
        rowGroups: [testLocationFilters, testCategoryFilters1],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when the rows group has been moved into rowGroups', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [testCategoryFilters1, testCategoryFilters2, testIndicators],
        rows: testLocationFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testLocationFilters,
        ],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains order when when the rows group has been moved into rowGroups and a new group has been added', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters2, testIndicators],
        rows: testCategoryFilters1,
      };
      // testCategoryFilters3 added
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters1, testIndicators, testCategoryFilters2],
        rows: testCategoryFilters3,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testTimePeriodFilters],
        columns: testCategoryFilters3,
        rowGroups: [testCategoryFilters2, testIndicators],
        rows: testCategoryFilters1,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('if rowGroups have not been reordered new rowGroups are added in their default positions', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          testCategoryFilters3,
          testCategoryFilters4,
          testLocationFilters,
        ],
        rows: testIndicators,
      };
      // testCategoryFilters1 and testCategoryFilters2 added
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testCategoryFilters3,
          testCategoryFilters4,
          testLocationFilters,
        ],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testDefaultTableHeaders);
    });
  });

  describe('columns', () => {
    test('retains the order of columns when no options have changed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[1],
          testTimePeriodFilters[0],
          testTimePeriodFilters[2],
        ],
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[0],
          testTimePeriodFilters[1],
          testTimePeriodFilters[2],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order of columns when columns have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[1],
          testTimePeriodFilters[0],
          testTimePeriodFilters[2],
        ],
      };
      // testTimePeriodFilters[2] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [testTimePeriodFilters[0], testTimePeriodFilters[1]],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [testTimePeriodFilters[1], testTimePeriodFilters[0]],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('adds new columns after reordered columns', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[1],
          testTimePeriodFilters[0],
          testTimePeriodFilters[2],
        ],
      };
      // testTimePeriodFilters[3] and testTimePeriodFilters[4] added
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[0],
          testTimePeriodFilters[1],
          testTimePeriodFilters[2],
          testTimePeriodFilters[3],
          testTimePeriodFilters[4],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[1],
          testTimePeriodFilters[0],
          testTimePeriodFilters[2],
          testTimePeriodFilters[3],
          testTimePeriodFilters[4],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of columns when columns have been added and removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[1],
          testTimePeriodFilters[0],
          testTimePeriodFilters[2],
        ],
      };
      // testTimePeriodFilters[3] and testTimePeriodFilters[4] added
      // testTimePeriodFilters[2] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[0],
          testTimePeriodFilters[1],
          testTimePeriodFilters[3],
          testTimePeriodFilters[4],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columns: [
          testTimePeriodFilters[1],
          testTimePeriodFilters[0],
          testTimePeriodFilters[3],
          testTimePeriodFilters[4],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('populate columns when the only column group has been removed', () => {
      const testReorderedTableHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testCategoryFilters1,
        rowGroups: [
          testTimePeriodFilters,
          testCategoryFilters2,
          testLocationFilters,
        ],
        rows: testIndicators,
      };
      // testCategoryFilters1 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters2, testLocationFilters],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedTableHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });
  });

  describe('columns in columnGroups', () => {
    test('retains the order of columns in columnGroups when no options have changed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[0],
            testCategoryFilters1[1],
            testCategoryFilters1[2],
          ],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order of columns in columnGroups when columns have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      // testCategoryFilters1[1] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [[testCategoryFilters1[0], testCategoryFilters1[2]]],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [[testCategoryFilters1[2], testCategoryFilters1[0]]],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of columns in columnGroups when columns have been added', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      // testCategoryFilters1[3] and testCategoryFilters1[4] added
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[0],
            testCategoryFilters1[1],
            testCategoryFilters1[2],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of columns in columnGroups when columns have been added and removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[1],
          ],
        ],
      };
      // testCategoryFilters1[3] and testCategoryFilters1[4] added
      // testCategoryFilters1[1] removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[0],
            testCategoryFilters1[2],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          [
            testCategoryFilters1[2],
            testCategoryFilters1[0],
            testCategoryFilters1[3],
            testCategoryFilters1[4],
          ],
        ],
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });
  });

  describe('columnGroups', () => {
    test('retains the order of columnGroups when no options have changed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        columns: testTimePeriodFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testLocationFilters,
        ],
        columns: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order of columnGroups when columnGroups have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          testLocationFilters,
          testCategoryFilters1,
          testCategoryFilters2,
        ],
        columns: testTimePeriodFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [testCategoryFilters2, testLocationFilters],
        columns: testTimePeriodFilters,
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [testLocationFilters, testCategoryFilters2],
        columns: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('handles when all columnGroups have been removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          testLocationFilters,
          testCategoryFilters1,
          testTimePeriodFilters,
        ],
        columns: testCategoryFilters2,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [],
        columns: testTimePeriodFilters,
      };
      const expected: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [],
        columns: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });

      expect(result).toEqual(expected);
    });

    test('retains the order of columnGroups and adds new groups distributed between rows and columns', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters5],
        rows: testIndicators,
      };
      // testCategoryFilters3 and testCategoryFilters4 added
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters3,
          testCategoryFilters4,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters5],
        rows: testIndicators,
      };
      // From a user's perspective there isn't a distinction between columns and columnGroups,
      // columns is an internal term for the last of the column groups (same for rows).
      // This means that adding new groups causes columns to change as well as columnGroups,
      // as the group previously in columns will be shifted into columnGroups
      const expected: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters5, testIndicators, testCategoryFilters3],
        rows: testCategoryFilters4,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order of columnGroups and columns when groups have been added and removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters2,
          testLocationFilters,
          testCategoryFilters1,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters5],
        rows: testIndicators,
      };
      // testCategoryFilters3 and testCategoryFilters4 added
      // testCategoryFilters2 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters1,
          testLocationFilters,
          testCategoryFilters3,
          testCategoryFilters4,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters5],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testLocationFilters, testCategoryFilters1],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters5, testIndicators, testCategoryFilters3],
        rows: testCategoryFilters4,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when the columns group has been moved into columnGroups and nothing has been added or removed', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testTimePeriodFilters,
        ],
        columns: testLocationFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        ...testDefaultBaseHeaders,
        columnGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testLocationFilters,
        ],
        columns: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains order when when the columns group has been moved into columnGroups and a new group has been added', () => {
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testTimePeriodFilters],
        columns: testCategoryFilters1,
        rowGroups: [testCategoryFilters5],
        rows: testIndicators,
      };
      // testCategoryFilters3 added
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters1,
          testTimePeriodFilters,
          testCategoryFilters2,
        ],
        columns: testCategoryFilters3,
        rowGroups: [testCategoryFilters5],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testTimePeriodFilters],
        columns: testCategoryFilters1,
        rowGroups: [testCategoryFilters5, testIndicators],
        rows: testCategoryFilters3,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });
  });

  describe('mixing up rows and columns', () => {
    test('retains the order when a rowGroup has been moved to columnGroups and no options have changed', () => {
      // testCategoryFilters3 moved from rowGroups to columnGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters3,
          testCategoryFilters1,
          testCategoryFilters2,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters],
        rows: testIndicators,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters1, testCategoryFilters2],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters3, testLocationFilters],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order when a rowGroup has been moved to columnGroups and options have been added and removed', () => {
      // testCategoryFilters3 moved from rowGroups to columnGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters3,
          testCategoryFilters1,
          testCategoryFilters2,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters],
        rows: testIndicators,
      };
      // testCategoryFilters4 added
      // testCategoryFilters2 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters1],
        columns: testTimePeriodFilters,
        rowGroups: [
          testCategoryFilters3,
          testLocationFilters,
          testCategoryFilters4,
        ],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testCategoryFilters1],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters, testIndicators],
        rows: testCategoryFilters4,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when rows has been moved to columnGroups and options have been added and removed', () => {
      // testIndicators moved from rows to columnGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [
          testIndicators,
          testCategoryFilters1,
          testCategoryFilters2,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters, testCategoryFilters5],
        rows: testCategoryFilters3,
      };
      // testCategoryFilters4 added
      // testCategoryFilters3 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testTimePeriodFilters,
        ],
        columns: testCategoryFilters4,
        rowGroups: [testLocationFilters, testCategoryFilters5],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [
          testIndicators,
          testCategoryFilters1,
          testCategoryFilters2,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters, testCategoryFilters5],
        rows: testCategoryFilters4,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when rows has been moved to columns and options have not changed', () => {
      // testIndicators moved from rows to columns
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testCategoryFilters1],
        columns: testIndicators,
        rowGroups: [testCategoryFilters3, testLocationFilters],
        rows: testTimePeriodFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters1, testCategoryFilters2],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters3, testLocationFilters],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testCategoryFilters1],
        columns: testIndicators,
        rowGroups: [testCategoryFilters3, testLocationFilters],
        rows: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when rows has been moved to columns and options have been added and removed', () => {
      // testIndicators moved from rows to columns
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testCategoryFilters1],
        columns: testIndicators,
        rowGroups: [testCategoryFilters3, testLocationFilters],
        rows: testTimePeriodFilters,
      };
      // testCategoryFilters4 added
      // testCategoryFilters3 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testCategoryFilters4,
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters],
        rows: testIndicators,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testCategoryFilters2, testCategoryFilters1],
        columns: testIndicators,
        rowGroups: [testLocationFilters, testTimePeriodFilters],
        rows: testCategoryFilters4,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when a columnGroup has been moved to rowGroups and no options have changed', () => {
      // testCategoryFilters3 moved from columnGroups to rowGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testIndicators,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters3,
          testCategoryFilters2,
        ],
        rows: testTimePeriodFilters,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testLocationFilters],
        columns: testIndicators,
        rowGroups: [testCategoryFilters1, testCategoryFilters2],
        rows: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });

    test('retains the order when a columnGroup has been moved to rowGroups and options have been added and removed', () => {
      // testCategoryFilters3 moved from columnGroups to rowGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testIndicators,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters3,
          testCategoryFilters2,
        ],
        rows: testTimePeriodFilters,
      };
      // testCategoryFilters4 added
      // testCategoryFilters2 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testLocationFilters],
        columns: testIndicators,
        rowGroups: [testCategoryFilters1, testCategoryFilters4],
        rows: testTimePeriodFilters,
      };

      const expected: TableHeadersConfig = {
        columnGroups: [testLocationFilters, testIndicators],
        columns: testCategoryFilters4,
        rowGroups: [testCategoryFilters1, testCategoryFilters3],
        rows: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when columns has been moved to rowGroups and options have been added and removed', () => {
      // testIndicators moved from columns to rowGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testCategoryFilters3,
        rowGroups: [testIndicators, testCategoryFilters1, testCategoryFilters2],
        rows: testTimePeriodFilters,
      };
      // testCategoryFilters4 added
      // testCategoryFilters3 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testIndicators,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testTimePeriodFilters,
        ],
        rows: testCategoryFilters4,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testCategoryFilters4,
        rowGroups: [testIndicators, testCategoryFilters1, testCategoryFilters2],
        rows: testTimePeriodFilters,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when columns has been moved to rows and options have not changed', () => {
      // testIndicators moved from columns to rows
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testLocationFilters],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters2, testCategoryFilters1],
        rows: testIndicators,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testLocationFilters],
        columns: testIndicators,
        rowGroups: [testCategoryFilters1, testCategoryFilters2],
        rows: testTimePeriodFilters,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testLocationFilters],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters2, testCategoryFilters1],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains the order when columns has been moved to rows and options have been added and removed', () => {
      // testIndicators moved from columns to rows
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [testCategoryFilters3, testLocationFilters],
        columns: testTimePeriodFilters,
        rowGroups: [testCategoryFilters2, testCategoryFilters1],
        rows: testIndicators,
      };
      // testCategoryFilters4 added
      // testCategoryFilters3 removed
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [testLocationFilters],
        columns: testIndicators,
        rowGroups: [
          testCategoryFilters1,
          testCategoryFilters2,
          testCategoryFilters4,
        ],
        rows: testTimePeriodFilters,
      };
      const expected: TableHeadersConfig = {
        columnGroups: [testLocationFilters, testTimePeriodFilters],
        columns: testCategoryFilters4,
        rowGroups: [testCategoryFilters2, testCategoryFilters1],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(expected);
    });

    test('retains order when a reordered group has moved to the other axis', () => {
      // testCategoryFilters2 moved from columnGroups to rowGroups
      const testReorderedHeaders: TableHeadersConfig = {
        columnGroups: [],
        columns: testTimePeriodFilters,
        rowGroups: [
          [
            testCategoryFilters2[3],
            testCategoryFilters2[0],
            testCategoryFilters2[1],
          ],
          testLocationFilters,
        ],
        rows: testIndicators,
      };
      const testDefaultTableHeaders: TableHeadersConfig = {
        columnGroups: [
          [
            testCategoryFilters2[0],
            testCategoryFilters2[1],
            testCategoryFilters2[3],
          ],
        ],
        columns: testTimePeriodFilters,
        rowGroups: [testLocationFilters],
        rows: testIndicators,
      };
      const result = applyTableHeadersOrder({
        reorderedTableHeaders: testReorderedHeaders,
        defaultTableHeaders: testDefaultTableHeaders,
      });
      expect(result).toEqual(testReorderedHeaders);
    });
  });
});
