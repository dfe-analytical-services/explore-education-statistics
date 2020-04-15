import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import generateDefaultDataSetLabel from '@common/modules/charts/util/generateDefaultDataSetLabel';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';

describe('generateDefaultDataSetLabel', () => {
  const testDataSet: ExpandedDataSet = {
    indicator: new Indicator({
      label: 'Indicator 1',
      value: 'indicator-1',
      unit: '',
    }),
    filters: [
      new CategoryFilter({
        value: 'filter-1',
        label: 'Filter 1',
        category: 'Category A',
      }),
      new CategoryFilter({
        value: 'filter-2',
        label: 'Filter 2',
        category: 'Category A',
      }),
    ],
    timePeriod: new TimePeriodFilter({
      year: 2020,
      label: '2020/21',
      code: 'AY',
    }),
    location: new LocationFilter({
      label: 'Location 1',
      level: 'country',
      value: 'location-1',
    }),
  };

  test('returns a label with all filters', () => {
    const label = generateDefaultDataSetLabel(testDataSet);

    expect(label).toBe('Indicator 1 (Filter 1, Filter 2, Location 1, 2020/21)');
  });

  test('returns a label that excludes filters from the label', () => {
    const label = generateDefaultDataSetLabel(testDataSet, 'filters');

    expect(label).toBe('Indicator 1 (Location 1, 2020/21)');
  });

  test('returns a label that excludes the location from the label', () => {
    const label = generateDefaultDataSetLabel(testDataSet, 'locations');

    expect(label).toBe('Indicator 1 (Filter 1, Filter 2, 2020/21)');
  });

  test('returns a label that excludes the time period from the label', () => {
    const label = generateDefaultDataSetLabel(testDataSet, 'timePeriod');

    expect(label).toBe('Indicator 1 (Filter 1, Filter 2, Location 1)');
  });

  test('cannot exclude indicator from the label', () => {
    const label = generateDefaultDataSetLabel(testDataSet, 'indicators');

    expect(label).toBe('Indicator 1 (Filter 1, Filter 2, Location 1, 2020/21)');
  });
});
