import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import { Dictionary } from '@common/types/util';
import sortBy from 'lodash/sortBy';
import { FullTableMeta } from '../types/fullTable';

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
