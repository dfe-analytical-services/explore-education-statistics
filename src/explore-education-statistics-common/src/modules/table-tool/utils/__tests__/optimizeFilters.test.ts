import {
  CategoryFilter,
  LocationFilter,
  Indicator,
  Filter,
} from '../../types/filters';
import optimizeFilters from '../optimizeFilters';

export const testCategoryFilter1 = new CategoryFilter({
  category: 'Duration of fixed period exclusions',
  group: 'Default',
  isTotal: false,
  label: '3',
  value: '7ec5fc48-5bb5-4896-985e-7d9921bd7e60',
});

export const testCategoryFilter2 = new CategoryFilter({
  value: '8fe677c9-f116-4874-90bd-edacf3460d75',
  label: '1',
  group: 'Default',
  isTotal: false,
  category: 'Duration of fixed period exclusions',
});
export const testCategoryFilter3 = new CategoryFilter({
  value: '6b942ff1-c845-4df1-956d-49ba1c339a5e',
  label: '2',
  group: 'Default',
  isTotal: false,
  category: 'Duration of fixed period exclusions',
});

export const testCategoryFilter4 = new CategoryFilter({
  value: '6d9e22db-fec9-43cf-856e-cd9f5a4886dd',
  label: 'Total',
  group: 'Default',
  isTotal: true,
  category: 'School type',
});
export const testCategoryFilter5 = new CategoryFilter({
  value: '65845b5d-57a2-451c-a947-a4328e5ee3d8',
  label: 'Special',
  group: 'Default',
  isTotal: false,
  category: 'School type',
});
export const testCategoryFilter6 = new CategoryFilter({
  value: '1889f1a2-2114-44c2-bf0a-4a9b4a36f462',
  label: 'State-funded primary',
  group: 'Default',
  isTotal: false,
  category: 'School type',
});
export const testCategoryFilter7 = new CategoryFilter({
  value: 'afcfcc14-27cb-48ee-90da-0aabd3f3af3b',
  label: 'State-funded secondary',
  group: 'Default',
  isTotal: false,
  category: 'School type',
});

export const testCategoryFilter8 = new CategoryFilter({
  value: 'c8b5b431-c308-4e09-9db5-11844e108407',
  label: 'Total',
  group: 'Total',
  isTotal: true,
  category: 'Characteristic',
});

export const testLocationFilter1 = new LocationFilter({
  value: 'd3dd04a9-ecf5-4e87-d270-08dae1c60ac3',
  label: 'Bexley',
  group: '',
  level: 'localAuthority',
});

export const testIndicator1 = new Indicator({
  value: '1b09726d-00d0-42ee-81a5-59d07c7dd9ba',
  label: 'Number of fixed period exclusions',
  unit: '',
  name: 'num_fixed_excl',
});

export const testIndicator2 = new Indicator({
  value: '39510778-d87a-4233-8767-539777ca6036',
  label: 'Number of pupil enrolments',
  unit: '',
  name: 'headcount',
});

export const testIndicator3 = new Indicator({
  value: '2835ebeb-fdb8-499a-9601-5d2d97f53936',
  label: 'Number of pupils',
  unit: '',
  name: 'headcount',
});

export const testIndicator4 = new Indicator({
  value: 'e790f56b-61ad-43a7-d427-08dae1c60ac3',
  label: 'Number of pupil enrolments',
  unit: '',
  decimalPlaces: 0,
  name: 'enrolments',
});

export const testFilters1: Filter[] = [
  testCategoryFilter1,
  testCategoryFilter1,
];

export const testFilters2: Filter[] = [testCategoryFilter4, testIndicator2];

export const testFilters3: Filter[] = [testCategoryFilter8, testIndicator3];

export const testFilters4: Filter[] = [testLocationFilter1, testIndicator4];

export const testHeaderConfig1: Filter[][] = [
  [testCategoryFilter1, testCategoryFilter1, testCategoryFilter1],
  [testIndicator1],
];

export const testHeaderConfig2: Filter[][] = [
  [
    testCategoryFilter4,
    testCategoryFilter5,
    testCategoryFilter6,
    testCategoryFilter7,
  ],
  [testIndicator2],
];

export const testHeaderConfig3: Filter[][] = [
  [testCategoryFilter8],
  [testIndicator3],
];

export const testHeaderConfig4: Filter[][] = [
  [testLocationFilter1],
  [testIndicator4],
];

describe('optimizeFilters', () => {
  test('reduces three identical category filters down to one', () => {
    expect(optimizeFilters(testFilters1, testHeaderConfig1)).toEqual([
      testCategoryFilter1,
    ]);
  });

  test('adds additional filter subgroups if required', () => {
    expect(optimizeFilters(testFilters2, testHeaderConfig2)).toEqual([
      testCategoryFilter4,
    ]);
  });

  test("doesn't show a single subgroup as this adds groups to a potentially crowded table", () => {
    expect(optimizeFilters(testFilters3, testHeaderConfig3)).toEqual([
      testCategoryFilter8,
    ]);
  });

  test('adds an empty header if there is missing location group data', () => {
    expect(optimizeFilters(testFilters4, testHeaderConfig4)).toEqual([
      testLocationFilter1,
    ]);
  });
});
