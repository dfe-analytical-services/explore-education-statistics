import { Filter } from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import {
  TableHeader,
  UnmappedTableHeadersConfig,
} from '@common/services/permalinkService';

/**
 * This function remaps the config filters into
 * full filter using the subjectMeta.
 */
export default function mapTableHeadersConfig(
  tableHeaders: UnmappedTableHeadersConfig,
  table: FullTable,
): TableHeadersConfig {
  const { subjectMeta, results } = table;
  const mapToFilters = (headers: TableHeader[]): Filter[] => {
    // When terms are selected the time period range can include ones not displayed in the table,
    // filter these out to ensure the reorder headings list is correct
    const filteredHeaders = headers.filter(
      header =>
        header.type !== 'TimePeriod' ||
        results.find(result => result.timePeriod === header.value),
    );

    return filteredHeaders.map(header => {
      let matchingFilter: Filter | undefined;

      switch (header.type) {
        case 'Indicator':
          matchingFilter = subjectMeta.indicators.find(
            indicator => indicator.value === header.value,
          );
          break;
        case 'Filter':
          matchingFilter = Object.values(subjectMeta.filters)
            .flatMap(filterGroup => filterGroup.options)
            .find(filter => filter.value === header.value);
          break;
        case 'Location':
          matchingFilter = subjectMeta.locations.find(
            location =>
              location.level === header.level &&
              location.value === header.value,
          );
          break;
        case 'TimePeriod':
          matchingFilter = subjectMeta.timePeriodRange.find(
            timePeriod => timePeriod.value === header.value,
          );
          break;
        default:
          throw new Error(`Invalid header type: ${JSON.stringify(header)}`);
      }

      if (!matchingFilter) {
        throw new Error(
          `Could not find matching filter for header: ${JSON.stringify(
            header,
          )}`,
        );
      }

      return matchingFilter;
    });
  };

  return {
    columns: mapToFilters(tableHeaders.columns),
    columnGroups: tableHeaders.columnGroups.map(headers =>
      mapToFilters(headers),
    ),
    rows: mapToFilters(tableHeaders.rows),
    rowGroups: tableHeaders.rowGroups.map(headers => mapToFilters(headers)),
  };
}
