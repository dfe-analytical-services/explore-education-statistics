import { AxisGroupBy } from '@common/modules/charts/types/chart';
import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import compact from 'lodash/compact';

/**
 * Create a default label for a {@param dataSet}
 * that excludes the {@param excludeGroup} filters from
 * the label (this is redundant as the label's
 * data set is already categorised).
 */
export default function generateDefaultDataSetLabel(
  dataSet: ExpandedDataSet,
  excludeGroup?: AxisGroupBy,
): string {
  const filterLabels = compact([
    ...(excludeGroup !== 'filters'
      ? dataSet.filters.filter(filter => !filter.isTotal)
      : []),
    excludeGroup !== 'locations' ? dataSet.location : undefined,
    excludeGroup !== 'timePeriod' ? dataSet.timePeriod : undefined,
  ]).map(filter => filter.label);

  return `${dataSet.indicator.label}${
    filterLabels.length ? ` (${filterLabels.join(', ')})` : ''
  }`;
}
