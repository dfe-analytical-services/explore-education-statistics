import last from 'lodash/last';
import React, {memo, forwardRef} from 'react';
import camelCase from 'lodash/camelCase';
import cartesian from '@common/lib/utils/cartesian';
import formatPretty from '@common/lib/utils/number/formatPretty';
import WarningMessage from '@common/components/WarningMessage';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import {FullTable} from '@common/modules/full-table/types/fullTable';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';
import {SortableOptionWithGroup, TableHeadersFormValues} from './TableHeadersForm';

interface Props {
  fullTable: FullTable;
  tableHeadersConfig: TableHeadersFormValues;
}

export const createRowGroups = (rowGroups: SortableOptionWithGroup[][]) : string[][] => {
  return rowGroups.flatMap(rowGroup =>

    rowGroup.reduce<[string[], string[]]>(([b, c], group) => (
      [
        group.filterGroup && [...b, group.filterGroup || ''] || b ,
        [...c, group.label]
      ]
    ), [[], []])
/*
      .map((filters,mapIndex) => {
          if (mapIndex === 0) {
            return filters.map((filterGroup, index, ary) => {
              if (index === filters.length-1 || filterGroup === ary[index+1]) return filterGroup;
              return '';
            });
          }
          return filters;
        })
*/
      .filter(ary => ary.length > 0)
  );
}

export const createIgnoreRowGroups = (rowGroups: SortableOptionWithGroup[][]) : boolean[] => {
  return rowGroups.flatMap(rowGroup =>

    rowGroup.reduce<[boolean[], boolean[]]>(([b, c], group) => (
      [
        group.filterGroup && [...b, true] || b,
        [...c, false]
      ]
    ), [[], []])
    /*
          .map((filters,mapIndex) => {
              if (mapIndex === 0) {
                return filters.map((filterGroup, index, ary) => {
                  if (index === filters.length-1 || filterGroup === ary[index+1]) return filterGroup;
                  return '';
                });
              }
              return filters;
            })
    */
      .filter(ary => ary.length > 0)
  ).map(group => group.includes(true));
};

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  (props: Props, dataTableRef) => {
    const {fullTable, tableHeadersConfig} = props;
    const {subjectMeta, results} = fullTable;

    if (results.length === 0) {
      return (
        <WarningMessage>
          A table could not be returned. There is no data for the options
          selected.
        </WarningMessage>
      );
    }

    const columnHeaders: string[][] = [
      ...tableHeadersConfig.columnGroups.map(colGroup =>
        colGroup.map(group => group.label),
      ),
      tableHeadersConfig.columns.map(column => column.label),
    ];

    const rowHeaders: string[][] = [
      ...createRowGroups(tableHeadersConfig.rowGroups),
      tableHeadersConfig.rows.map(row => row.label),
    ];

    const ignoreRowHeaders: boolean[] = [
      ...createIgnoreRowGroups(tableHeadersConfig.rowGroups),
      false
    ];

    const rowHeadersCartesian = cartesian(
      ...tableHeadersConfig.rowGroups,
      tableHeadersConfig.rows,
    );

    const columnHeadersCartesian = cartesian(
      ...tableHeadersConfig.columnGroups,
      tableHeadersConfig.columns,
    );

    const rows = rowHeadersCartesian.map(rowFilterCombination => {
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
                result.location[geographicLevel].code === filter.value
              );
            })
          );
        });

        if (!matchingResult) {
          return 'n/a';
        }

        const value = matchingResult.measures[indicator.value];

        if (Number.isNaN(Number(value))) {
          return value;
        }

        return `${formatPretty(value)}${indicator.unit}`;
      });
    });

    return (
      <FixedMultiHeaderDataTable
        caption={<DataTableCaption {...subjectMeta} id="dataTableCaption" />}
        columnHeaders={columnHeaders}
        rowHeaders={rowHeaders}
        ignoreRowHeaders={ignoreRowHeaders}
        rows={rows}
        ref={dataTableRef}
        footnotes={subjectMeta.footnotes}
      />
    );
  },
);

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
