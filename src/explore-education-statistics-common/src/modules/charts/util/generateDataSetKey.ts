import { AxisGroupBy } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';

/**
 * Create a key for the {@param dataSet} by stringifying
 * the data set so that we can create a unique identifier.
 * In Recharts, this is referred to as the `dataKey`.
 *
 * We can also provide {@param excludeGroup} to remove part
 * of the key e.g. time period/location. These parts of the
 * data set are optional, but we can still group by them, so
 * we need to filter these out to avoid having too many
 * data points in the chart.
 */
export default function generateDataSetKey(
  dataSet: DataSet,
  excludeGroup?: AxisGroupBy,
): string {
  const filters =
    excludeGroup !== 'filters' ? [...dataSet.filters].sort() : undefined;

  const indicator =
    excludeGroup !== 'indicators' ? dataSet.indicator : undefined;

  const location =
    excludeGroup !== 'locations' && dataSet.location
      ? {
          level: dataSet.location.level,
          value: dataSet.location.value,
        }
      : undefined;

  const timePeriod =
    excludeGroup !== 'timePeriod' ? dataSet.timePeriod : undefined;

  return JSON.stringify({
    filters,
    indicator,
    location,
    timePeriod,
  });
}
