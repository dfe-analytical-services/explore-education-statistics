import { DataSet, ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import { CategoryFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';

/**
 * Expand a {@param dataSet} into its
 * {@see ExpandedDataSet} for that contains all
 * of its filters in their class-based forms
 * using the given {@param meta}.
 */
export default function expandDataSet(
  dataSet: DataSet,
  meta: FullTableMeta,
): ExpandedDataSet {
  const categoricalFilters = Object.values(meta.filters).flat();

  const filters = dataSet.filters
    .map(filterValue =>
      categoricalFilters.find(filter => filter.value === filterValue),
    )
    .filter(filter => !!filter) as CategoryFilter[];

  const indicator = meta.indicators.find(
    filter => dataSet.indicator === filter.value,
  );

  if (!indicator) {
    throw new Error(`Could not find indicator: ${dataSet.indicator}`);
  }

  const location = meta.locations.find(
    filter =>
      dataSet.location?.level === filter.level &&
      dataSet.location?.value === filter.value,
  );

  const timePeriod = meta.timePeriodRange.find(
    filter => dataSet.timePeriod === filter.value,
  );

  return {
    filters,
    indicator,
    location,
    timePeriod,
  };
}
