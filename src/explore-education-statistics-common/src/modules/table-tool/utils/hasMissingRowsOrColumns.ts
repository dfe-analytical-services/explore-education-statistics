import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';

interface Props {
  query: ReleaseTableDataQuery;
  subjectMeta: FullTableMeta;
  tableHeaderFilters: Set<string>;
}

/**
 * Determines whether any rows or columns are excluded from the table because they have no data.
 * For filters, indicators and locations:
 *  - when there's no data, these aren't included in subjectMeta.
 *  - compare subjectMeta with the query to find the excluded ones.
 *  For timePeriod:
 *  - when there's no data, these are in subjectMeta but aren't in tableHeadersConfig.
 *  - compare subjectMeta with tableHeadersConfig to find the excluded ones.
 *  - the query isn't useful here as just has the start and end timePeriods.
 */
export default function hasMissingRowsOrColumns({
  query,
  subjectMeta,
  tableHeaderFilters,
}: Props): boolean {
  if (
    query.locationIds.length !== subjectMeta.locations.length ||
    query.indicators.length !== subjectMeta.indicators.length
  ) {
    return true;
  }

  const subjectMetaFilters = Object.values(subjectMeta.filters)
    .flatMap(filterGroup => filterGroup.options)
    .map(filter => filter.id);

  if (query.filters.length !== subjectMetaFilters.length) {
    return true;
  }

  if (
    !subjectMeta.timePeriodRange.every(timePeriod =>
      tableHeaderFilters.has(timePeriod.value),
    )
  ) {
    return true;
  }

  return false;
}
