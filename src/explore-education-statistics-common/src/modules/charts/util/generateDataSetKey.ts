import { DataSet } from '@common/modules/charts/types/dataSet';
import {
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';

/**
 * Create a key for the {@param dataSet} by stringifying
 * the data set so that we can create a unique identifier.
 * In Recharts, this is referred to as the `dataKey` and
 * creates a new data point in the chart.
 *
 * We can also provide {@param excludeFilter} to remove part
 * of the key. Typically, we want to remove the data set
 * category's filter from the key as we want to reduce the
 * number of unique data set keys (and consequently, unique
 * data points from the charts).
 */
export default function generateDataSetKey(
  dataSet: DataSet,
  excludeFilter?: Filter,
): string {
  const filters = !(excludeFilter instanceof Filter)
    ? [...dataSet.filters]
    : dataSet.filters.filter(
        filterValue => filterValue !== excludeFilter.value,
      );

  const indicator = !(excludeFilter instanceof Indicator)
    ? dataSet.indicator
    : undefined;

  const location =
    !(excludeFilter instanceof LocationFilter) && dataSet.location
      ? {
          level: dataSet.location.level,
          value: dataSet.location.value,
        }
      : undefined;

  const timePeriod = !(excludeFilter instanceof TimePeriodFilter)
    ? dataSet.timePeriod
    : undefined;

  return JSON.stringify({
    filters: filters.sort(),
    indicator,
    location,
    timePeriod,
  });
}
