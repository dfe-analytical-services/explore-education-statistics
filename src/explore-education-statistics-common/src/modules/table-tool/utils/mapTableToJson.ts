import Header from '@common/modules/table-tool/utils/Header';
import { Filter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import createExpandedColumnHeaders from '@common/modules/table-tool/utils/createExpandedColumnHeaders';
import createExpandedRowHeaders from '@common/modules/table-tool/utils/createExpandedRowHeaders';
import hasMissingRowsOrColumns from '@common/modules/table-tool/utils/hasMissingRowsOrColumns';
import createTableCartesian from '@common/modules/table-tool/utils/createTableCartesian';
import optimizeFilters from '@common/modules/table-tool/utils/optimizeFilters';
import {
  ReleaseTableDataQuery,
  TableDataResult,
} from '@common/services/tableBuilderService';
import cartesian from '@common/utils/cartesian';
import last from 'lodash/last';
import sumBy from 'lodash/sumBy';

export type Scope = 'colgroup' | 'col' | 'rowgroup' | 'row';

export interface TableCellJson {
  colSpan?: number;
  rowSpan?: number;
  scope?: Scope;
  tag: 'th' | 'td';
  text?: string;
}

export interface TableJson {
  thead: TableCellJson[][];
  tbody: TableCellJson[][];
}

interface Props {
  tableHeadersConfig: TableHeadersConfig;
  subjectMeta: FullTableMeta;
  results: TableDataResult[];
  query?: ReleaseTableDataQuery;
}

/**
 * Used to convert a table into a JSON representation of the HTML table
 * that can be used to render the table in the frontend.
 */
export default function mapTableToJson({
  tableHeadersConfig,
  subjectMeta,
  results,
  query,
}: Props): {
  tableJson: TableJson;
  hasMissingRowsOrColumns?: boolean;
} {
  const rowHeadersCartesian = cartesian(
    ...tableHeadersConfig.rowGroups,
    tableHeadersConfig.rows,
  );

  const columnHeadersCartesian = cartesian(
    ...tableHeadersConfig.columnGroups,
    tableHeadersConfig.columns,
  );

  const tableHeaderFilters = [
    ...tableHeadersConfig.columnGroups.flatMap(filterGroup => filterGroup),
    ...tableHeadersConfig.rowGroups.flatMap(filterGroup => filterGroup),
    ...tableHeadersConfig.columns,
    ...tableHeadersConfig.rows,
  ].reduce((acc, filter) => acc.add(filter.id), new Set<string>());

  const excludedFilterIds = getExcludedFilterIds(
    tableHeaderFilters,
    subjectMeta,
  );

  const tableCartesian = createTableCartesian({
    rowHeadersCartesian,
    columnHeadersCartesian,
    results,
    excludedFilterIds,
  });

  const rowHeaders = tableCartesian.reduce<Header[]>((acc, row) => {
    // Only need to use first column's rowFilters
    // as they are the same for every column.
    const filters = optimizeFilters(row[0].rowFilters, [
      ...tableHeadersConfig.rowGroups,
      tableHeadersConfig.rows,
    ]);

    return addFilters(acc, filters);
  }, []);

  // Only need to use first row's columnFilters
  // as they are the same for every row.
  const columnHeaders = tableCartesian[0].reduce<Header[]>((acc, column) => {
    const filters = optimizeFilters(column.columnFilters, [
      ...tableHeadersConfig.columnGroups,
      tableHeadersConfig.columns,
    ]);

    return addFilters(acc, filters);
  }, []);

  const rows: TableCellJson[][] = tableCartesian.map(row =>
    row.map(cell => ({ text: cell.text, tag: 'td' })),
  );

  const expandedColumnHeaders = createExpandedColumnHeaders(columnHeaders);

  const expandedRowHeaders = createExpandedRowHeaders(rowHeaders);

  const totalColumns = sumBy(
    expandedRowHeaders[0],
    header => header.colSpan ?? 0,
  );

  // Insert a spacer cell at the start of column headers.
  const spacerCell: TableCellJson = {
    colSpan: totalColumns,
    rowSpan: expandedColumnHeaders.length,
    tag: 'td',
  };

  expandedColumnHeaders[0].unshift(spacerCell);

  return {
    tableJson: {
      thead: expandedColumnHeaders,
      tbody: rows.map((row, index) => [...expandedRowHeaders[index], ...row]),
    },
    hasMissingRowsOrColumns: query
      ? hasMissingRowsOrColumns({
          query,
          subjectMeta,
          tableHeaderFilters,
        })
      : false,
  };
}

/**
 * Gets filters which are in the subjectMeta but not in the tableHeaderFilters.
 */
function getExcludedFilterIds(
  tableHeaderFilters: Set<string>,
  subjectMeta: FullTableMeta,
): Set<string> {
  const subjectMetaFilters = [
    ...Object.values(subjectMeta.filters).flatMap(
      filterGroup => filterGroup.options,
    ),
    ...subjectMeta.timePeriodRange,
    ...subjectMeta.locations,
    ...subjectMeta.indicators,
  ].map(filter => filter.id);

  return new Set(
    subjectMetaFilters.filter(
      subjectMetaFilter => !tableHeaderFilters.has(subjectMetaFilter),
    ),
  );
}

/**
 * Convert {@param filters} into {@see Header} instances
 * and add them to {@param headers}.
 */
function addFilters(headers: Header[], filters: Filter[]) {
  filters.forEach((filter, filterIndex) => {
    if (!headers.length) {
      headers.push(new Header(filter.id, filter.label));
      return;
    }

    const currentHeader = last(headers);

    if (!currentHeader) {
      return;
    }

    if (currentHeader.id === filter.id) {
      currentHeader.span += 1;
    } else if (filterIndex === 0) {
      headers.push(new Header(filter.id, filter.label));
    } else {
      currentHeader.addChildToLastParent(
        new Header(filter.id, filter.label),
        filterIndex - 1,
      );
    }
  });

  return headers;
}
