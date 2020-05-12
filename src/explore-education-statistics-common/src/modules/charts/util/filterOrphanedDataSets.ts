import { DataSet } from '@common/modules/charts/types/dataSet';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';

/**
 * Remove data sets that are no longer applicable
 * to a given table's subject meta.
 */
export default function filterOrphanedDataSets(
  dataSets: DataSet[],
  meta: FullTableMeta,
): DataSet[] {
  return dataSets.filter(dataSet => {
    if (dataSet.location) {
      const locationId = LocationFilter.createId(dataSet.location);

      if (meta.locations.every(location => locationId !== location.id)) {
        return false;
      }
    }

    if (dataSet.timePeriod) {
      if (
        meta.timePeriodRange.every(
          timePeriod => timePeriod.id !== dataSet.timePeriod,
        )
      ) {
        return false;
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

    return hasFilters && hasIndicator;
  });
}
