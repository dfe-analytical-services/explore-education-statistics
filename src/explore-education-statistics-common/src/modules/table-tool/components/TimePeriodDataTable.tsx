import WarningMessage from '@common/components/WarningMessage';
import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import { getIndicatorPath } from '@common/modules/charts/util/groupResultMeasuresByDataSet';
import groupResultMeasuresByCombination from '@common/modules/table-tool/components/utils/groupResultMeasuresByCombination';
import Header from '@common/modules/table-tool/components/utils/Header';
import logger from '@common/services/logger';
import isErrorLike from '@common/utils/error/isErrorLike';
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
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import { Dictionary, PartialBy } from '@common/types';
import cartesian from '@common/utils/cartesian';
import formatPretty from '@common/utils/number/formatPretty';
import get from 'lodash/get';
import last from 'lodash/last';
import React, { forwardRef, memo } from 'react';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';

const EMPTY_CELL_TEXT = 'no data';

class FilterGroup extends Filter {
  constructor(label: string) {
    super({
      label,
      value: label,
    });
  }
}

function getExcludedFilters(
  tableHeaderFilters: string[],
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
      subjectMetaFilter => !tableHeaderFilters.includes(subjectMetaFilter),
    ),
  );
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
const hasMissingRowsOrColumns = (
  query: ReleaseTableDataQuery,
  subjectMeta: FullTableMeta,
  tableHeaderFilters: string[],
): boolean => {
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
      tableHeaderFilters.includes(timePeriod.value),
    )
  ) {
    return true;
  }

  return false;
};

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

        // The location hierarchy expects grouping, for example the LAD attribute is grouped by Region.
        // However, the screener does not require the data to have all attributes of the hierarchy.
        // When this data is missing the backend returns an empty string, this causes table layout problems as there is a missing header cell where the group would have been.
        // To fix this an empty header for the missing group data is added, When the table is rendered these empty header cells are converted to <td> as empty <th>'s cause accessibility problems.
        const isMissingLocationGroup =
          filter instanceof LocationFilter && filter.group === '';

        return hasMultipleSubGroups &&
          ((filter.group && filter.group !== 'Default') ||
            isMissingLocationGroup)
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

interface TableCell {
  text: string;
  rowFilters: Filter[];
  columnFilters: Filter[];
}

interface Props {
  captionTitle?: string;
  dataBlockId?: string;
  footnotesClassName?: string;
  fullTable: FullTable;
  query?: ReleaseTableDataQuery;
  source?: string;
  tableHeadersConfig: TableHeadersConfig;
  onError?: (message: string) => void;
}

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  function TimePeriodDataTable(
    {
      captionTitle,
      dataBlockId,
      footnotesClassName,
      fullTable,
      query,
      source,
      tableHeadersConfig,
      onError,
    }: Props,
    dataTableRef,
  ) {
    try {
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

      // Track which columns actually have text values
      // as we want to remove empty ones later.
      const columnsWithText = columnHeadersCartesian.map(() => false);

      const tableHeaderFilters = [
        ...tableHeadersConfig.columnGroups.flatMap(filterGroup => filterGroup),
        ...tableHeadersConfig.rowGroups.flatMap(filterGroup => filterGroup),
        ...tableHeadersConfig.columns,
        ...tableHeadersConfig.rows,
      ].map(filter => filter.id);

      const excludedFilters = getExcludedFilters(
        tableHeaderFilters,
        subjectMeta,
      );

      // Group measures by their respective combination of filters
      // allowing lookups later on to be MUCH faster.
      const measuresByFilterCombination = groupResultMeasuresByCombination(
        results,
        excludedFilters,
      );

      const showMissingRowsOrColumnsWarning =
        query &&
        hasMissingRowsOrColumns(query, subjectMeta, tableHeaderFilters);

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

      const captionId = dataBlockId
        ? `dataTableCaption-${dataBlockId}`
        : 'dataTableCaption';

      const rows = filteredCartesian.map(row => row.map(cell => cell.text));

      return (
        <>
          {showMissingRowsOrColumnsWarning && (
            <WarningMessage>
              Some rows and columns are not shown in this table as the data does
              not exist in the underlying file.
            </WarningMessage>
          )}
          <FixedMultiHeaderDataTable
            caption={
              <DataTableCaption
                title={captionTitle}
                meta={subjectMeta}
                id={captionId}
              />
            }
            captionId={captionId}
            columnHeaders={columnHeaders}
            footnotesClassName={footnotesClassName}
            footnotesId={
              dataBlockId
                ? `dataTableFootnotes-${dataBlockId}`
                : 'dataTableFootnotes'
            }
            rowHeaders={rowHeaders}
            rows={rows}
            ref={dataTableRef}
            footnotes={subjectMeta.footnotes}
            source={source}
            footnotesHeadingHiddenText={`for ${captionTitle}`}
          />
        </>
      );
    } catch (error) {
      logger.error(error);

      onError?.(isErrorLike(error) ? error.message : 'Unknown error');

      return (
        <WarningMessage testId="table-error">
          There was a problem rendering the table.
        </WarningMessage>
      );
    }
  },
);

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
