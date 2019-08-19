import sortBy from 'lodash/sortBy';
import { Dictionary } from '@common/types/util';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import {
  CategoryFilter,
  Indicator,
} from '@frontend/modules/table-tool/components/types/filters';
import { FullTableMeta } from '@frontend/services/permalinkService';
import TimePeriod from '../components/types/TimePeriod';

const removeSiblinglessTotalRows = (
  categoryFilters: Dictionary<CategoryFilter[]>,
): CategoryFilter[][] => {
  return Object.values(categoryFilters).filter(filter => {
    return filter.length > 1 || !filter[0].isTotal;
  });
};

const transformTableMetaFiltersToCategoryFilters = (
  filters: FullTableMeta['filters'],
): Dictionary<CategoryFilter[]> => {
  return mapValuesWithKeys(filters, (filterKey, filterValue) =>
    Object.values(filterValue.options)
      .map(options => options.options)
      .flat()
      .map(
        filter =>
          new CategoryFilter(filter, filter.value === filterValue.totalValue),
      ),
  );
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
    columns: timePeriodRange.map(timePeriod => new TimePeriod(timePeriod)),
    rows: indicators.map(indicator => new Indicator(indicator)),
  };
};

export default getDefaultTableHeaderConfig;
