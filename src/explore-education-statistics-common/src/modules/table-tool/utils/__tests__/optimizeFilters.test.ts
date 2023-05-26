import {
  LocationFilter,
  Filter,
  TimePeriodFilter,
  CategoryFilter,
} from '@common/modules/table-tool/types/filters';
import optimizeFilters, {
  FilterGroup,
} from '@common/modules/table-tool/utils/optimizeFilters';

const testLocationFilter1 = new LocationFilter({
  value: 'loc-1',
  label: 'Darlington',
  group: 'North East',
  level: 'localAuthority',
});
const testLocationFilter2 = new LocationFilter({
  value: 'loc-2',
  label: 'Durham',
  group: 'North East',
  level: 'localAuthority',
});
const testLocationFilter3 = new LocationFilter({
  value: 'loc-4',
  label: 'England',
  group: 'England',
  level: 'country',
});
const testLocationFilter5 = new LocationFilter({
  value: 'loc-5',
  label: 'Wales',
  group: undefined,
  level: 'country',
});
const testTimePeriod1 = new TimePeriodFilter({
  label: '2012/13',
  year: 2012,
  code: 'AY',
  order: 0,
});
const testTimePeriod2 = new TimePeriodFilter({
  label: '2013/14',
  year: 2013,
  code: 'AY',
  order: 1,
});
const testCategory1Group1Filter1 = new CategoryFilter({
  value: 'filter-1',
  label: 'Filter 1',
  group: 'Filter Group 1',
  category: 'Category 1',
});
const testCategory1Group1Filter2 = new CategoryFilter({
  value: 'filter-2',
  label: 'Filter 2',
  group: 'Filter Group 1',
  category: 'Category 1',
});
const testCategory1Group2Filter3 = new CategoryFilter({
  value: 'filter-3',
  label: 'Filter 3',
  group: 'Filter Group 2',
  category: 'Category 1',
});
const testCategory2GroupDefaultFilter4 = new CategoryFilter({
  value: 'filter-4',
  label: 'Filter 4',
  group: 'Default',
  category: 'Category 2',
});
const testCategory3Group1Filter5 = new CategoryFilter({
  value: 'filter-5',
  label: 'Filter 5',
  group: 'Filter Group 1',
  category: 'Category 3',
});
const testCategory3Group2Filter6 = new CategoryFilter({
  value: 'filter-6',
  label: 'Filter 6',
  group: 'Filter Group 2',
  category: 'Category 3',
});
const testCategory3Group2Filter7 = new CategoryFilter({
  value: 'filter-7',
  label: 'Filter 7',
  group: 'Filter Group 2',
  category: 'Category 3',
});

describe('optimizeFilters', () => {
  test('returns the filters unchanged when there are no groups to add or single headers to remove', () => {
    const testFilters: Filter[] = [testLocationFilter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      [testLocationFilter1, testLocationFilter2],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual(testFilters);
  });

  test('removes the last filter when there are zero filters in the last header array', () => {
    const testFilters: Filter[] = [testLocationFilter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      [testLocationFilter1, testLocationFilter2],
      [],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testLocationFilter1]);
  });

  test('removes the last filter when there is only one filter in the last header array', () => {
    const testFilters: Filter[] = [testLocationFilter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      [testLocationFilter1, testLocationFilter2],
      [testTimePeriod1],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testLocationFilter1]);
  });

  test('adds FilterGroup when groups with different labels in 1 level are not `Default`', () => {
    const testFilters: Filter[] = [testCategory1Group1Filter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      // Belong to different groups
      [
        testCategory1Group1Filter1,
        testCategory1Group1Filter2,
        testCategory1Group2Filter3,
      ],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      new FilterGroup('Filter Group 1', 0),
      testCategory1Group1Filter1,
      testTimePeriod1,
    ]);
  });

  test('adds FilterGroups for groups across 2 non-adjacent levels with different labels that are not `Default`', () => {
    const testFilters: Filter[] = [
      testCategory1Group1Filter1,
      testTimePeriod1,
      testCategory3Group1Filter5,
    ];
    const testHeaderConfig: Filter[][] = [
      // Belong to different groups
      [
        testCategory1Group1Filter1,
        testCategory1Group1Filter2,
        testCategory1Group2Filter3,
      ],
      [testTimePeriod1, testTimePeriod2],
      // Belong to different groups
      [testCategory3Group1Filter5, testCategory3Group2Filter6],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      new FilterGroup('Filter Group 1', 0),
      testCategory1Group1Filter1,
      testTimePeriod1,
      new FilterGroup('Filter Group 1', 2),
      testCategory3Group1Filter5,
    ]);
  });

  test('adds FilterGroups for groups across 2 adjacent levels with different labels that are not `Default`', () => {
    const testFilters: Filter[] = [
      testCategory1Group1Filter2,
      testCategory3Group2Filter6,
      testTimePeriod2,
    ];
    const testHeaderConfig: Filter[][] = [
      // Belong to different groups
      [
        testCategory1Group1Filter1,
        testCategory1Group1Filter2,
        testCategory1Group2Filter3,
      ],
      // Belong to different groups
      [testCategory3Group1Filter5, testCategory3Group2Filter6],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      new FilterGroup('Filter Group 1', 0),
      testCategory1Group1Filter2,
      new FilterGroup('Filter Group 2', 1),
      testCategory3Group2Filter6,
      testTimePeriod2,
    ]);
  });

  test('only adds FilterGroup for group that is not `Default`', () => {
    const testFilters: Filter[] = [
      testCategory1Group1Filter1,
      testTimePeriod1,
      testCategory2GroupDefaultFilter4,
    ];
    const testHeaderConfig: Filter[][] = [
      // Belong to different groups
      [
        testCategory1Group1Filter1,
        testCategory1Group1Filter2,
        testCategory1Group2Filter3,
      ],
      [testTimePeriod1, testTimePeriod2],
      // A filter belongs to Default group, so no FilterGroup added
      [testCategory2GroupDefaultFilter4, testCategory3Group1Filter5],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      new FilterGroup('Filter Group 1', 0),
      testCategory1Group1Filter1,
      testTimePeriod1,
      testCategory2GroupDefaultFilter4,
    ]);
  });

  test('does not add a FilterGroup when group is `Default`', () => {
    const testFilters: Filter[] = [
      testCategory2GroupDefaultFilter4,
      testTimePeriod1,
    ];
    const testHeaderConfig: Filter[][] = [
      // Belong to different groups, but filter
      // being optimized belongs to Default group
      [testCategory2GroupDefaultFilter4, testCategory1Group2Filter3],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testCategory2GroupDefaultFilter4, testTimePeriod1]);
  });

  test('does not add a FilterGroup when one group that is not `Default`', () => {
    const testFilters: Filter[] = [testCategory1Group1Filter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      // Belong to same group
      [testCategory1Group1Filter1, testCategory1Group1Filter2],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testCategory1Group1Filter1, testTimePeriod1]);
  });

  test('does not add FilterGroups when groups across 1 level have same labels that are not `Default`', () => {
    const testFilters: Filter[] = [testCategory1Group1Filter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      // Belong to same group
      [testCategory1Group1Filter1, testCategory1Group1Filter2],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testCategory1Group1Filter1, testTimePeriod1]);
  });

  test('does not add FilterGroups when groups across 2 adjacent levels have same labels that are not `Default`', () => {
    const testFilters: Filter[] = [
      testCategory1Group1Filter2,
      testCategory3Group2Filter7,
      testTimePeriod2,
    ];
    const testHeaderConfig: Filter[][] = [
      // Belong to same group
      [testCategory1Group1Filter1, testCategory1Group1Filter2],
      // Belong to same group
      [testCategory3Group2Filter6, testCategory3Group2Filter7],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      testCategory1Group1Filter2,
      testCategory3Group2Filter7,
      testTimePeriod2,
    ]);
  });

  test('does not add FilterGroups when groups across 2 non-adjacent levels have same labels that are not `Default`', () => {
    const testFilters: Filter[] = [
      testCategory1Group1Filter1,
      testTimePeriod1,
      testCategory3Group1Filter5,
    ];
    const testHeaderConfig: Filter[][] = [
      // Belong to same group
      [testCategory1Group1Filter1, testCategory1Group1Filter2],
      [testTimePeriod1, testTimePeriod2],
      // Belong to same group
      [testCategory3Group2Filter6, testCategory3Group2Filter7],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      testCategory1Group1Filter1,
      testTimePeriod1,
      testCategory3Group1Filter5,
    ]);
  });

  test('does not add a FilterGroup when there are no groups', () => {
    const testFilters: Filter[] = [testLocationFilter5, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      // None of these are grouped
      [testLocationFilter3, testLocationFilter5],
      [testTimePeriod1, testTimePeriod2],
    ];

    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testLocationFilter5, testTimePeriod1]);
  });
});
