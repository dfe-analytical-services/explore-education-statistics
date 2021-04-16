import { DataSet } from '@common/modules/charts/types/dataSet';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';

export default function isOrphanedDataSet(
  dataSet: DataSet,
  meta: FullTableMeta,
): boolean {
  if (dataSet.location) {
    const locationId = LocationFilter.createId(dataSet.location);

    if (meta.locations.every(location => locationId !== location.id)) {
      return true;
    }
  }

  if (dataSet.timePeriod) {
    if (
      meta.timePeriodRange.every(
        timePeriod => timePeriod.id !== dataSet.timePeriod,
      )
    ) {
      return true;
    }
  }

  const filterOptions = Object.values(meta.filters).flatMap(
    filterGroup => filterGroup.options,
  );

  const hasFilters = dataSet.filters.every(
    filter => !!filterOptions.find(option => option.id === filter),
  );

  const hasIndicator = meta.indicators.find(
    indicator => indicator.id === dataSet.indicator,
  );

  return !hasFilters || hasIndicator === undefined;
}
