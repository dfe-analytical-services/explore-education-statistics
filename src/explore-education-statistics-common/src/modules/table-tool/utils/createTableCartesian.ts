import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import { Dictionary, PartialBy } from '@common/types';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { getIndicatorPath } from '@common/modules/table-tool/utils/groupResultMeasuresByDataSet';
import groupResultMeasuresByCombination from '@common/modules/table-tool/utils/groupResultMeasuresByCombination';
import formatPretty from '@common/utils/number/formatPretty';
import { TableDataResult } from '@common/services/tableBuilderService';
import get from 'lodash/get';

export const EMPTY_CELL_TEXT = 'no data';

interface TableCell {
  text: string;
  rowFilters: Filter[];
  columnFilters: Filter[];
}

interface Props {
  rowHeadersCartesian: Filter[][];
  columnHeadersCartesian: Filter[][];
  results: TableDataResult[];
  excludedFilterIds: Set<string>;
}

/**
 * Create a cartesian table of cells.
 */
export default function createTableCartesian({
  rowHeadersCartesian,
  columnHeadersCartesian,
  results,
  excludedFilterIds,
}: Props): TableCell[][] {
  // Track which columns actually have text values
  // as we want to remove empty ones later.
  const columnsWithText = columnHeadersCartesian.map(() => false);

  const tableCartesian = rowHeadersCartesian.map(rowFilterCombination => {
    return columnHeadersCartesian.map(
      (columnFilterCombination, columnIndex) => {
        const filterCombination: Filter[] = [
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

        // Group measures by their respective combination of filters
        // allowing lookups later on to be MUCH faster.
        const measuresByFilterCombination = groupResultMeasuresByCombination(
          results,
          excludedFilterIds,
        );

        const text = getCellText(
          measuresByFilterCombination,
          dataSet as ExpandedDataSet,
        );

        // There is at least one cell in this
        // column that has a text value.
        if (text !== EMPTY_CELL_TEXT) {
          // eslint-disable-next-line no-param-reassign
          columnsWithText[columnIndex] = true;
        }

        return {
          text,
          rowFilters: rowFilterCombination,
          columnFilters: columnFilterCombination,
        };
      },
    );
  });

  return tableCartesian
    .filter(row => row.some(cell => cell.text !== EMPTY_CELL_TEXT))
    .map(row => row.filter((_, index) => columnsWithText[index]));
}

/**
 * Get the text for the cell.
 * - if the value is a number, return it formatted with the unit and
 * decimal places
 * - if it's not a number, return it (sometimes symbols like `z` are
 * used in the data)
 * if there's not value return the EMPTY_CELL_TEXT
 */
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
