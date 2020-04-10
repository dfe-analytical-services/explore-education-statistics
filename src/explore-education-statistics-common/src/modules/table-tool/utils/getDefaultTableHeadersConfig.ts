import {
  CategoryFilter,
  Indicator,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import sortBy from 'lodash/sortBy';

const removeSiblinglessTotalRows = (
  filters: Dictionary<CategoryFilter[]>,
): CategoryFilter[][] => {
  return Object.values(filters).filter(filter => {
    return filter.length > 1 || !filter[0].isTotal;
  });
};

const getDefaultTableHeaderConfig = (fullTableMeta: FullTableMeta) => {
  const { indicators, filters, locations, timePeriodRange } = fullTableMeta;

  const sortedFilters = sortBy(
    [...removeSiblinglessTotalRows(filters), locations],
    [options => options.length],
  );

  const halfwayIndex = Math.floor(sortedFilters.length / 2);

  return {
    columnGroups: sortedFilters.slice(0, halfwayIndex),
    rowGroups: sortedFilters.slice(halfwayIndex),
    columns: timePeriodRange.map(
      timePeriod => new TimePeriodFilter(timePeriod),
    ),
    rows: sortBy(
      indicators.map(i => new Indicator(i)),
      o => o.label,
    ),
  };
};

export default getDefaultTableHeaderConfig;
