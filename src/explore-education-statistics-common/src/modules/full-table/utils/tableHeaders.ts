import sortBy from 'lodash/sortBy';
import { Dictionary } from '@common/types/util';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import {
  CategoryFilter,
  Indicator,
  TimePeriodFilter,
  LocationFilter,
} from '@common/modules/full-table/types/filters';
import { FullTableMeta } from '../types/fullTable';
import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/table-tool/components/utils/tableToolHelpers';

export interface TableHeadersConfig {
  columns: (Indicator | TimePeriodFilter)[];
  columnGroups: (LocationFilter | CategoryFilter)[][];
  rows: (Indicator | TimePeriodFilter)[];
  rowGroups: (LocationFilter | CategoryFilter)[][];
}

const removeSiblinglessTotalRows = (
  categoryFilters: Dictionary<CategoryFilter[]>,
): CategoryFilter[][] => {
  return Object.values(categoryFilters).filter(filter => {
    return filter.length > 1 || !filter[0].isTotal;
  });
};
/*
export const transformTableMetaFiltersToCategoryFilters = (
  filters: FullTableMeta['filters'],
): Dictionary<CategoryFilter[]> => {
  return mapValuesWithKeys(filters, (filterKey, filterValue) =>
    Object.values(filterValue.options)
      .flatMap(options => options.options)
      .map(
        filter =>
          new CategoryFilter(filter, filter.value === filterValue.totalValue),
      ),
  );
};

 */

const getDefaultTableHeaderConfig = (fullTableMeta: FullTableMeta) => {
  const { indicators, filters, locations, timePeriodRange } = fullTableMeta;

  const sortedFilters = sortBy(
    [
      ...removeSiblinglessTotalRows(
        transformTableMetaFiltersToCategoryFilters(filters),
      ),
      locations,
    ],
    [options => options.length],
  );

  const halfwayIndex = Math.floor(sortedFilters.length / 2);

  return {
    columnGroups: sortedFilters.slice(0, halfwayIndex),
    rowGroups: sortedFilters.slice(halfwayIndex),
    columns: timePeriodRange.map(
      timePeriod => new TimePeriodFilter(timePeriod),
    ),
    rows: indicators.map(indicator => new Indicator(indicator)),
  };
};

export default getDefaultTableHeaderConfig;
