import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import {
  TableHeader,
  UnmappedTableHeadersConfig,
} from '@common/services/permalinkService';

const mapToTableHeaders = (filters: Filter[]): TableHeader[] => {
  return filters.map<TableHeader>(filter => {
    if (filter instanceof LocationFilter) {
      return {
        type: 'Location',
        value: filter.value,
        level: filter.level,
      };
    }

    if (filter instanceof TimePeriodFilter) {
      return {
        type: 'TimePeriod',
        value: filter.value,
      };
    }

    if (filter instanceof CategoryFilter) {
      return {
        type: 'Filter',
        value: filter.value,
      };
    }

    if (filter instanceof Indicator) {
      return {
        type: 'Indicator',
        value: filter.value,
      };
    }

    throw new Error('Could not map filter to header');
  });
};

export default function mapUnmappedTableHeaders(
  tableHeaders: TableHeadersConfig,
): UnmappedTableHeadersConfig {
  return {
    columnGroups: tableHeaders.columnGroups.map(filters =>
      mapToTableHeaders(filters),
    ),
    columns: mapToTableHeaders(tableHeaders.columns),
    rowGroups: tableHeaders.rowGroups.map(filters =>
      mapToTableHeaders(filters),
    ),
    rows: mapToTableHeaders(tableHeaders.rows),
  };
}
