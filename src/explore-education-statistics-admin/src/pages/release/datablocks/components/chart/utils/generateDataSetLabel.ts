import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import compact from 'lodash/compact';

/**
 * Create a human readable label for a {@param dataSet}.
 */
export default function generateDataSetLabel(dataSet: ExpandedDataSet): string {
  const labels = compact([
    ...dataSet.filters
      .filter(filter => !filter.isTotal)
      .map(filter => filter.label),
    dataSet.location?.label ?? 'All locations',
    dataSet.timePeriod?.label ?? 'All time periods',
  ]);

  return `${dataSet.indicator.label}${
    labels.length ? ` (${labels.join(', ')})` : ''
  }`;
}
