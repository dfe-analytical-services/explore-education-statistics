import ErrorBoundary from '@common/components/ErrorBoundary';
import WarningMessage from '@common/components/WarningMessage';
import { Header } from '@common/modules/table-tool/components/MultiHeaderTable';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import {
  FullTable,
  FullTableResult,
} from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
import cartesian from '@common/utils/cartesian';
import formatPretty from '@common/utils/number/formatPretty';
import camelCase from 'lodash/camelCase';
import last from 'lodash/last';
import React, { forwardRef, memo } from 'react';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';

const getCellText = (
  result: FullTableResult | undefined,
  indicator: Indicator,
): string => {
  if (!result) {
    return 'n/a';
  }

  const value = result.measures[indicator.value];

  if (Number.isNaN(Number(value))) {
    return value;
  }

  return formatPretty(value, indicator.unit);
};

interface TableCell {
  text: string;
  rowFilters: Filter[];
  columnFilters: Filter[];
}

interface Props {
  captionTitle?: string;
  fullTable: FullTable;
  tableHeadersConfig: TableHeadersConfig;
  source?: string;
}

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  (
    { fullTable, tableHeadersConfig, captionTitle, source }: Props,
    dataTableRef,
  ) => {
    const { subjectMeta, results } = fullTable;

    if (results.length === 0) {
      return (
        <WarningMessage>
          A table could not be returned. There is no data for the options
          selected.
        </WarningMessage>
      );
    }

    const rowHeadersCartesian = cartesian(
      ...tableHeadersConfig.rowGroups,
      tableHeadersConfig.rows as Filter[],
    );

    const columnHeadersCartesian = cartesian(
      ...tableHeadersConfig.columnGroups,
      tableHeadersConfig.columns as Filter[],
    );

    const tableCartesian: TableCell[][] = rowHeadersCartesian.map(
      rowFilterCombination => {
        const rowCol1 = last(rowFilterCombination);

        return columnHeadersCartesian.map(columnFilterCombination => {
          const rowCol2 = last(columnFilterCombination);

          // User could choose to flip rows and columns
          const indicator = (rowCol1 instanceof Indicator
            ? rowCol1
            : rowCol2) as Indicator;

          const timePeriod = (rowCol2 instanceof TimePeriodFilter
            ? rowCol2
            : rowCol1) as TimePeriodFilter;

          const filterCombination = [
            ...rowFilterCombination,
            ...columnFilterCombination,
          ];

          const categoryFilters = filterCombination.filter(
            filter => filter instanceof CategoryFilter,
          );

          const locationFilters = filterCombination.filter(
            filter => filter instanceof LocationFilter,
          ) as LocationFilter[];

          const matchingResult = results.find(result => {
            return (
              categoryFilters.every(filter =>
                result.filters.includes(filter.value),
              ) &&
              result.timePeriod === timePeriod.value &&
              locationFilters.every(filter => {
                const geographicLevel = camelCase(result.geographicLevel);
                return (
                  result.location[geographicLevel] &&
                  result.location[geographicLevel].code === filter.value &&
                  filter.level === geographicLevel
                );
              })
            );
          });

          return {
            text: getCellText(matchingResult, indicator),
            rowFilters: rowFilterCombination,
            columnFilters: columnFilterCombination,
          };
        });
      },
    );

    /**
     * Extract and push table headers from a set
     * of filters into an accumulator of the new
     * headers that we are trying to create.
     *
     * This function isn't particularly glamorous as
     * we rely on mutation, but it seems like the
     * simplest way of creating our table headers
     * without needing lots of transient transformations.
     */
    const pushHeaders = ({
      acc,
      filters,
      start,
    }: {
      acc: Header[][];
      filters: Filter[];
      start: number;
    }): Header[][] => {
      filters.forEach((filter, filterGroupIndex) => {
        const isLastHeader = filterGroupIndex === filters.length - 1;

        const currentGroup = acc[filterGroupIndex];

        if (!currentGroup) {
          acc.push([
            {
              id: filter.id,
              text: filter.label,
              start,
              span: 1,
              group: filter.filterGroup,
            },
          ]);

          return;
        }

        const lastHeaderGroup = last(currentGroup);
        const isGroupHeader = !isLastHeader;

        // Increase spans for rowgroup/colgroup headers as these
        // are able to span multiple row/col headers.
        if (isGroupHeader && lastHeaderGroup?.id === filter.id) {
          lastHeaderGroup.span += 1;
        } else {
          currentGroup.push({
            id: filter.id,
            text: filter.label,
            start,
            span: 1,
            group: filter.filterGroup,
          });
        }
      });

      return acc;
    };

    const rowHeaders = tableCartesian.reduce<Header[][]>(
      (acc, row, rowIndex) => {
        // Only need to use first column's rowFilters
        // as they are the same for every column.
        return pushHeaders({
          acc,
          filters: row[0].rowFilters,
          start: rowIndex,
        });
      },
      [],
    );

    // Only need to use first row's columnFilters
    // as they are the same for every row.
    const columnHeaders = tableCartesian[0].reduce<Header[][]>(
      (acc, column, columnIndex) => {
        return pushHeaders({
          acc,
          filters: column.columnFilters,
          start: columnIndex,
        });
      },
      [],
    );

    const rows = tableCartesian.map(row => row.map(cell => cell.text));

    const cleanupHeaders = (
      headerGroups: Header[][],
      rowColFilters: Filter[],
    ): Header[][] => {
      const headers = headerGroups.map(headerGroup => {
        const firstHeaderSubGroup = headerGroup[0].group;

        // Don't bother showing a single subgroup as this adds
        // additional groups on a potentially crowded table.
        const hasMultipleSubGroups = headerGroup.some(
          header => header.group !== firstHeaderSubGroup,
        );

        return headerGroup.map(header => {
          return {
            ...header,
            group:
              hasMultipleSubGroups && header.group !== 'Default'
                ? header.group
                : undefined,
          };
        });
      });

      if (rowColFilters.length > 1) {
        return headers;
      }

      // There is only one or zero row/col filter header,
      // and we want to avoid having only a single header
      // as this can often get repeated many times for
      // tables with multiple filter groups.
      // We should should try and display these filter group
      // headers instead of the row/col header as they
      // should provide more useful information to the user.
      return headers.length > 1 ? headers.slice(0, -1) : headers;
    };

    return (
      <ErrorBoundary
        fallback={
          <WarningMessage>
            There was a problem rendering your table.
          </WarningMessage>
        }
      >
        <FixedMultiHeaderDataTable
          caption={
            <DataTableCaption
              {...subjectMeta}
              title={captionTitle}
              id="dataTableCaption"
            />
          }
          columnHeaders={cleanupHeaders(
            columnHeaders,
            tableHeadersConfig.columns,
          )}
          rowHeaders={cleanupHeaders(rowHeaders, tableHeadersConfig.rows)}
          rows={rows}
          ref={dataTableRef}
          footnotes={subjectMeta.footnotes}
          source={source}
        />
      </ErrorBoundary>
    );
  },
);

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
