import ErrorBoundary from '@common/components/ErrorBoundary';
import WarningMessage from '@common/components/WarningMessage';
import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import { getIndicatorPath } from '@common/modules/charts/util/groupResultMeasuresByDataSet';
import groupResultMeasuresByCombination from '@common/modules/table-tool/components/utils/groupResultMeasuresByCombination';
import Header from '@common/modules/table-tool/components/utils/Header';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import {
  FullTable,
  FullTableMeta,
} from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { Dictionary, PartialBy } from '@common/types';
import cartesian from '@common/utils/cartesian';
import formatPretty from '@common/utils/number/formatPretty';
import get from 'lodash/get';
import last from 'lodash/last';
import React, { forwardRef, memo } from 'react';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';

const EMPTY_CELL_TEXT = 'n/a';

class FilterGroup extends Filter {
  constructor(label: string) {
    super({
      label,
      value: label,
    });
  }
}

function getExcludedFilters(
  tableHeadersConfig: TableHeadersConfig,
  subjectMeta: FullTableMeta,
): string[] {
  const tableHeaderFilters = [
    ...tableHeadersConfig.columnGroups.flatMap(filterGroup => filterGroup),
    ...tableHeadersConfig.rowGroups.flatMap(filterGroup => filterGroup),
    ...tableHeadersConfig.columns,
    ...tableHeadersConfig.rows,
  ].map(filter => filter.id);

  const subjectMetaFilters = [
    ...Object.values(subjectMeta.filters).flatMap(
      filterGroup => filterGroup.options,
    ),
    ...subjectMeta.timePeriodRange,
    ...subjectMeta.locations,
    ...subjectMeta.indicators,
  ].map(filter => filter.id);

  return subjectMetaFilters.filter(
    subjectMetaFilter => !tableHeaderFilters.includes(subjectMetaFilter),
  );
}

function getCellText(
  measuresByDataSet: Dictionary<unknown>,
  dataSet: ExpandedDataSet,
): string {
  const { location, timePeriod, filters, indicator } = dataSet;

  const path = getIndicatorPath({
    filters: filters.map(filter => filter.value),
    location: location
      ? {
          value: location.value,
          level: location.level,
        }
      : undefined,
    timePeriod: timePeriod?.value,
    indicator: indicator?.value,
  });

  const value = get(measuresByDataSet, path);

  if (typeof value === 'undefined') {
    return EMPTY_CELL_TEXT;
  }

  if (Number.isNaN(Number(value))) {
    return value;
  }

  return formatPretty(value, indicator.unit, indicator.decimalPlaces);
}

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

const TimePeriodDataTableInternal = forwardRef<HTMLElement, Props>(
  function TimePeriodDataTableInternal(
    { fullTable, tableHeadersConfig, captionTitle, source }: Props,
    dataTableRef,
  ) {
    const { subjectMeta, results } = fullTable;

    if (!tableHeadersConfig.rows.length) {
      return (
        <WarningMessage>
          There was a problem rendering the table.
        </WarningMessage>
      );
    }

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

    // Track which columns actually have text values
    // as we want to remove empty ones later.
    const columnsWithText = columnHeadersCartesian.map(() => false);

    const excludedFilters = getExcludedFilters(tableHeadersConfig, subjectMeta);

    // Group measures by their respective combination of filters
    // allowing lookups later on to be MUCH faster.
    const measuresByFilterCombination = groupResultMeasuresByCombination(
      results,
      excludedFilters,
    );

    const tableCartesian: TableCell[][] = rowHeadersCartesian.map(
      rowFilterCombination => {
        return columnHeadersCartesian.map(
          (columnFilterCombination, columnIndex) => {
            const filterCombination = [
              ...rowFilterCombination,
              ...columnFilterCombination,
            ];

            const dataSet = filterCombination.reduce<
              PartialBy<ExpandedDataSet, 'indicator'>
            >(
              (acc, filter) => {
                if (filter instanceof CategoryFilter) {
                  acc.filters.push(filter);
                }

                if (filter instanceof TimePeriodFilter) {
                  acc.timePeriod = filter;
                }

                if (filter instanceof LocationFilter) {
                  acc.location = filter;
                }

                if (filter instanceof Indicator) {
                  acc.indicator = filter;
                }

                return acc;
              },
              {
                filters: [],
              },
            );

            if (!dataSet.indicator) {
              throw new Error('No indicator for filter combination');
            }

            const text = getCellText(
              measuresByFilterCombination,
              dataSet as ExpandedDataSet,
            );

            // There is at least one cell in this
            // column that has a text value.
            if (text !== EMPTY_CELL_TEXT) {
              columnsWithText[columnIndex] = true;
            }

            return {
              text,
              rowFilters: rowFilterCombination,
              columnFilters: columnFilterCombination,
            };
          },
        );
      },
    );

    const filteredCartesian = tableCartesian
      .filter(row => row.some(cell => cell.text !== EMPTY_CELL_TEXT))
      .map(row => row.filter((_, index) => columnsWithText[index]));

    /**
     * Function to optimize a set of filters for the best viewing experience.
     *
     * Typically we add or remove filters depending on whether they are
     * actually needed for the user to understand the table.
     */
    function optimizeFilters(filters: Filter[], headerConfig: Filter[][]) {
      const rowColFilters = last(headerConfig);

      let optimizedFilters = filters;

      // There is only one or zero row/col filter header, and we want to avoid
      // having only a single header as this can often get repeated many times
      // for tables with multiple filter groups.
      // We should should try and display these filter group headers instead of the
      // row/col header as they should provide more useful information to the user.
      if (rowColFilters && rowColFilters.length <= 1) {
        optimizedFilters = filters.length > 1 ? filters.slice(0, -1) : filters;
      }

      return (
        optimizedFilters
          // Add additional filter sub groups
          // to our filters if required.
          .flatMap((filter, index) => {
            const firstSubGroup = headerConfig[index][0].group;

            // Don't bother showing a single subgroup as this adds
            // additional groups to a potentially crowded table.
            const hasMultipleSubGroups = headerConfig[index].some(
              header => header.group !== firstSubGroup,
            );

            return hasMultipleSubGroups &&
              filter.group &&
              filter.group !== 'Default'
              ? [new FilterGroup(filter.group), filter]
              : filter;
          })
      );
    }

    /**
     * Convert {@param filters} into {@see Header} instances
     * and add them to {@param headers}.
     */
    function addFilters(headers: Header[], filters: Filter[]): Header[] {
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

    const rowHeaders = filteredCartesian.reduce<Header[]>((acc, row) => {
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
    const columnHeaders = filteredCartesian[0].reduce<Header[]>(
      (acc, column) => {
        const filters = optimizeFilters(column.columnFilters, [
          ...tableHeadersConfig.columnGroups,
          tableHeadersConfig.columns,
        ]);

        return addFilters(acc, filters);
      },
      [],
    );

    const rows = filteredCartesian.map(row => row.map(cell => cell.text));

    return (
      <FixedMultiHeaderDataTable
        caption={
          <DataTableCaption
            {...subjectMeta}
            title={captionTitle}
            id="dataTableCaption"
          />
        }
        columnHeaders={columnHeaders}
        rowHeaders={rowHeaders}
        rows={rows}
        ref={dataTableRef}
        footnotes={subjectMeta.footnotes}
        source={source}
      />
    );
  },
);

const TimePeriodDataTable = forwardRef<HTMLElement, Props>((props, ref) => {
  return (
    <ErrorBoundary
      fallback={
        <WarningMessage>
          There was a problem rendering the table.
        </WarningMessage>
      }
    >
      <TimePeriodDataTableInternal {...props} ref={ref} />
    </ErrorBoundary>
  );
});

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
