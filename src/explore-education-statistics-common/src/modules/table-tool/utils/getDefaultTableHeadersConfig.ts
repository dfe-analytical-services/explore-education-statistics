import {
  CategoryFilter,
  Indicator,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import sortBy from 'lodash/sortBy';

const removeSiblinglessTotalRows = (
  filters: FullTableMeta['filters'],
): CategoryFilter[][] => {
  return Object.values(filters)
    .map(filterGroup => filterGroup.options)
    .filter(filterGroup => {
      return filterGroup.length > 1 || !filterGroup[0].isTotal;
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
