import { DataSet, ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import { CategoryFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';

/**
 * Expand a {@param dataSet} into its {@see ExpandedDataSet}
 * for that contains all of its filters in their class-based
 * forms using the given {@param meta}.
 */
export default function expandDataSet(
  dataSet: DataSet,
  meta: FullTableMeta,
): ExpandedDataSet {
  const categoricalFilters = Object.values(meta.filters).flatMap(
    filterGroup => filterGroup.options,
  );

  const filters = dataSet.filters
    .map(filterValue => {
      const matchingFilter = categoricalFilters.find(
        filter => filter.value === filterValue,
      );

      if (!matchingFilter) {
        throw new Error(`Could not find category filter: '${filterValue}'`);
      }

      return matchingFilter;
    })
    .filter(filter => !!filter) as CategoryFilter[];

  const indicator = meta.indicators.find(
    filter => dataSet.indicator === filter.value,
  );

  if (!indicator) {
    throw new Error(`Could not find indicator: '${dataSet.indicator}'`);
  }

  const location = meta.locations.find(
    filter =>
      dataSet.location?.level === filter.level &&
      dataSet.location?.value === filter.value,
  );

  if (dataSet.location && !location) {
    throw new Error(
      `Could not find location with value: '${dataSet.location.value}', level: '${dataSet.location.level}'`,
    );
  }

  const timePeriod = meta.timePeriodRange.find(
    filter => dataSet.timePeriod === filter.value,
  );

  if (dataSet.timePeriod && !timePeriod) {
    throw new Error(`Could not find time period: '${dataSet.timePeriod}'`);
  }

  return {
    filters,
    indicator,
    location,
    timePeriod,
  };
}
