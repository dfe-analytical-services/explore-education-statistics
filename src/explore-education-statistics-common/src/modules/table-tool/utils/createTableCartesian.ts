import { ExpandedDataSet } from '@common/modules/charts/types/dataSet';
import { Dictionary, PartialBy } from '@common/types';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import {
  getIndicatorPath,
  MeasuresGroupedByDataSet,
} from '@common/modules/table-tool/components/utils/groupResultMeasuresByDataSet';
import get from 'lodash/get';
import formatPretty from '@common/utils/number/formatPretty';
import set from 'lodash/set';
import { TableDataResult } from '@common/services/tableBuilderService';

export const EMPTY_CELL_TEXT = 'no data';

interface TableCell {
  text: string;
  rowFilters: Filter[];
  columnFilters: Filter[];
}

/**
 * TODO: - add description
 * @param rowHeadersCartesian
 * @param columnHeadersCartesian
 * @param results
 * @param excludedFilters
 * @returns
 */
export default function createTableCartesian(
  rowHeadersCartesian: Filter[][],
  columnHeadersCartesian: Filter[][],
  results: TableDataResult[],
  excludedFilters: Set<string>,
): TableCell[][] {
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

  // Group measures by their respective combination of filters
  // allowing lookups later on to be MUCH faster.

  /**
   * Group {@param results} and their measures by their
   * associated combination of filters at each level
   * of a nested dictionary.
   *
   * {@param excludedFilterIds} should be used to prevent filters
   * that have been stripped out of the table headers from being
   * included in the path. Otherwise, it will not be possible
   * to resolve all combinations to a result.
   *
   * This is convenient for larger result sets where iteration
   * can be really expensive. By grouping the results in such a
   * way, we can achieve O(1) time complexity.
   *
   * Note that we cannot specify a return type as it's not
   * possible to define one due to the dynamic nature of the result.
   */

  function groupResultMeasuresByCombination(
    tableDataResults: TableDataResult[],
    excludedFilterIds: Set<string> = new Set(),
  ): Dictionary<unknown> {
    return tableDataResults.reduce<MeasuresGroupedByDataSet>((acc, result) => {
      const {
        geographicLevel,
        filters,
        timePeriod,
        locationId,
        location,
        measures,
      } = result;

      // Default to using the code in the legacy 'location' field that exists for historical Permalinks created prior to EES-2955.
      // Note that EES-3203 added 'locationId' to table results before EES-2955 switched over to using location id's
      // and removed 'location'.
      // The presence of a location id doesn't mean the code should be ignored.
      const locationCodeOrId =
        (location && location[geographicLevel]?.code) || locationId;

      const path = getIndicatorPath({
        filters,
        timePeriod,
        indicator: '',
        location: locationCodeOrId
          ? {
              value: locationCodeOrId,
              level: geographicLevel,
            }
          : undefined,
      });

      const filteredPath = path.filter(id => !excludedFilterIds.has(id));
      const existing = get(acc, filteredPath);

      if (existing) {
        set(acc, filteredPath, {
          ...existing,
          ...measures,
        });
      } else {
        set(acc, filteredPath, measures);
      }

      return acc;
    }, {});
  }

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

        const measuresByFilterCombination = groupResultMeasuresByCombination(
          results,
          excludedFilters,
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
