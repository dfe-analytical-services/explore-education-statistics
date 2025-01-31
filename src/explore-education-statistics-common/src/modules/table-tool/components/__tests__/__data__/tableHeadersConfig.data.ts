import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';

export const testTimePeriodFilters: TimePeriodFilter[] = [
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
];

export const testIndicators: Indicator[] = [
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
];

export const testCategoryFilters: CategoryFilter[] = [
  new CategoryFilter({
    value: 'category-1-value',
    label: 'Category 1',
    group: 'Default',
    isTotal: false,
    category: 'Category group',
  }),
  new CategoryFilter({
    value: 'category-2-value',
    label: 'Category 2',
    group: 'Default',
    isTotal: false,
    category: 'Category group',
  }),
];

export const testLocationFilters: LocationFilter[] = [
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

export const testTableHeadersConfig: TableHeadersConfig = {
  columnGroups: [testLocationFilters, testCategoryFilters],
  columns: testTimePeriodFilters,
  rowGroups: [],
  rows: testIndicators,
};
