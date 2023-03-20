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
const testCategoryFilter1 = new CategoryFilter({
  value: 'category-filter-1',
  label: 'Filter 1',
  group: 'Filter Group 1',
  category: 'Category1',
});
const testCategoryFilter2 = new CategoryFilter({
  value: 'category-filter-2',
  label: 'Filter 2',
  group: 'Filter Group 1',
  category: 'Category1',
});
const testCategoryFilter3 = new CategoryFilter({
  value: 'category-filter-3',
  label: 'Filter 3',
  group: 'FilterGroup 2',
  category: 'Category2',
});
const testCategoryFilter4 = new CategoryFilter({
  value: 'category-filter-4',
  label: 'Filter 4',
  group: 'Default',
  category: 'Category3',
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

  test('adds a filter group when there are multiple subgroups and the group is not `Default`', () => {
    const testFilters: Filter[] = [testCategoryFilter1, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      [testCategoryFilter1, testCategoryFilter2, testCategoryFilter3],
      [testTimePeriod1, testTimePeriod2],
    ];
    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([
      new FilterGroup('Filter Group 1'),
      testCategoryFilter1,
      testTimePeriod1,
    ]);
  });

  test('does not add a filter group when there are multiple subgroups and the group is `Default`', () => {
    const testFilters: Filter[] = [testCategoryFilter4, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      [testCategoryFilter3, testCategoryFilter4],
      [testTimePeriod1, testTimePeriod2],
    ];
    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testCategoryFilter4, testTimePeriod1]);
  });

  test('does not add a filter group when there are multiple subgroups and the group is undefined', () => {
    const testFilters: Filter[] = [testLocationFilter5, testTimePeriod1];
    const testHeaderConfig: Filter[][] = [
      [testLocationFilter3, testLocationFilter5],
      [testTimePeriod1, testTimePeriod2],
    ];
    const result = optimizeFilters(testFilters, testHeaderConfig);
    expect(result).toEqual([testLocationFilter5, testTimePeriod1]);
  });
});
