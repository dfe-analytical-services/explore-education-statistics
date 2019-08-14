import sortBy from 'lodash/sortBy';
import { Dictionary } from '@common/types/util';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';

const removeSiblinglessTotalRows = (
  categoryFilters: Dictionary<CategoryFilter[]>,
): CategoryFilter[][] => {
  return Object.values(categoryFilters).filter(filter => {
    return filter.length > 1 || !filter[0].isTotal;
  });
};

const getDefaultTableHeaderConfig = (
  indicators: Indicator[],
  filters: Dictionary<CategoryFilter[]>,
  timePeriods: TimePeriod[],
  locations: LocationFilter[],
) => {
  const sortedFilters = sortBy(
    [...removeSiblinglessTotalRows(filters), locations],
    [options => options.length],
  );

  const halfwayIndex = Math.floor(sortedFilters.length / 2);

  return {
    columnGroups: sortedFilters.slice(0, halfwayIndex),
    rowGroups: sortedFilters.slice(halfwayIndex),
    columns: timePeriods,
    rows: indicators,
  };
};

export default getDefaultTableHeaderConfig;
