import {
  Filter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import {
  FullTable,
  FullTableMeta,
} from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import last from 'lodash/last';
import orderBy from 'lodash/orderBy';

const removeSingleOptionFilterGroups = (filters: Filter[][]): Filter[][] => {
  return filters.filter(filterGroup => filterGroup.length > 1);
};

/**
 * Distributes a list of {@param filters} into row groups
 * and column groups sorted by their sizes. We try to
 * optimise this for the best default result possible.
 */
function getSortedRowColGroups(
  filters: Filter[][],
): Pick<TableHeadersConfig, 'rowGroups' | 'columnGroups'> {
  const sortedFilters = orderBy(filters, [
    // Sort groups by number of options and total label length as we want
    // to place wordier filters as rows to avoid having to scroll horizontally.
    options => options.length,
    options => options.reduce((acc, option) => acc + option.label.length, 0),
  ]);

  const halfwayIndex = Math.floor(sortedFilters.length / 2);

  // Re-sort by number of options. We want to avoid cases where groups
  // with small number of options repeat many times, causing the
  // table headers for that direction to look more 'dense'.
  const columnGroups = orderBy(
    sortedFilters.slice(0, halfwayIndex),
    options => options.length,
  );

  const rowGroups = orderBy(
    sortedFilters.slice(halfwayIndex),
    options => options.length,
  );

  // Finally, we just need to sort the individual
  // sub groups of the filter group so that
  // they're in ascending order.
  return {
    columnGroups,
    rowGroups,
  };
}

function getFixedTimePeriodAndIndicatorTableHeadersConfig(
  fullTableMeta: FullTableMeta,
  filteredTimePeriodRange: TimePeriodFilter[],
): TableHeadersConfig {
  const { indicators, filters, locations } = fullTableMeta;

  const { columnGroups, rowGroups } = getSortedRowColGroups(
    removeSingleOptionFilterGroups([
      ...Object.values(filters).map(group => group.options),
      locations,
    ]),
  );

  return {
    columnGroups,
    rowGroups,
    columns: filteredTimePeriodRange,
    rows: indicators,
  };
}

/**
 * Get a default table header configuration from a {@param fullTable}.
 *
 * We try our best to return table headers for a good default viewing
 * experience, but this is not always guaranteed due to the flexibility
 * of table tool. The user may need to tweak the table headers further
 * to create the best possible table for them.
 */
export default function getDefaultTableHeaderConfig(
  fullTable: FullTable,
): TableHeadersConfig {
  const { indicators, filters, locations, timePeriodRange } =
    fullTable.subjectMeta;
  const { results } = fullTable;

  // When terms are selected the time period range can include ones not displayed in the table,
  // filter these out to ensure the reorder headings list is correct
  const filteredTimePeriodRange = timePeriodRange.filter(period =>
    results.find(result => result.timePeriod === period.value),
  );

  // In the following instance, we should display the time periods in the
  // columns and the indicators in the rows as this should be still be
  // okay for most resolutions.
  // Above this, we will potentially start creating too many columns
  // and cause the user to have to horizontally scroll. We should avoid this!
  if (filteredTimePeriodRange.length < 8) {
    return getFixedTimePeriodAndIndicatorTableHeadersConfig(
      fullTable.subjectMeta,
      filteredTimePeriodRange,
    );
  }

  const { columnGroups, rowGroups } = getSortedRowColGroups([
    // Can potentially remove all other groups, but should
    // at least show the time period and indicator as these
    // should provide the most information to the user.
    ...removeSingleOptionFilterGroups([
      ...Object.values(filters).map(group => group.options),
      locations,
    ]),
    filteredTimePeriodRange,
    indicators,
  ]);

  return {
    columnGroups: columnGroups.length > 1 ? columnGroups.slice(0, -1) : [],
    rowGroups: rowGroups.length > 1 ? rowGroups.slice(0, -1) : [],
    columns: last(columnGroups) ?? [],
    rows: last(rowGroups) ?? [],
  };
}
