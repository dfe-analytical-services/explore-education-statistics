import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import compact from 'lodash/compact';

/**
 * Create a default label for a {@param dataSet}
 * that excludes the {@param excludeFilter} from
 * the label (this is redundant as the label's
 * data set is already categorised).
 */
export default function generateDefaultDataSetLabel(
  dataSet: ExpandedDataSet,
  excludeFilter?: Filter,
): string {
  const filterLabels = compact([
    ...(!(excludeFilter instanceof CategoryFilter)
      ? dataSet.filters.filter(filter => !filter.isTotal)
      : dataSet.filters.filter(
          filter => filter.value !== excludeFilter.value && !filter.isTotal,
        )),
    !(excludeFilter instanceof LocationFilter) ? dataSet.location : undefined,
    !(excludeFilter instanceof TimePeriodFilter)
      ? dataSet.timePeriod
      : undefined,
  ]).map(filter => filter.label);

  if (excludeFilter instanceof Indicator) {
    return filterLabels.join(', ');
  }

  return `${dataSet.indicator.label}${
    filterLabels.length ? ` (${filterLabels.join(', ')})` : ''
  }`;
}
