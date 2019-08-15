import last from 'lodash/last';
import React, { memo, forwardRef } from 'react';
import cartesian from '@common/lib/utils/cartesian';
import formatPretty from '@common/lib/utils/number/formatPretty';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import { FullTable } from '@frontend/services/permalinkService';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';
import { TableHeadersFormValues } from './TableHeadersForm';

interface Props {
  fullTable: FullTable;
  tableHeadersConfig: TableHeadersFormValues;
}

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  (props: Props, dataTableRef) => {
    const { fullTable, tableHeadersConfig } = props;
    const { subjectMeta, results } = fullTable;

    const columnHeaders: string[][] = [
      ...tableHeadersConfig.columnGroups.map(colGroup =>
        colGroup.map(group => group.label),
      ),
      tableHeadersConfig.columns.map(column => column.label),
    ];

    const rowHeaders: string[][] = [
      ...tableHeadersConfig.rowGroups.map(rowGroup =>
        rowGroup.map(group => group.label),
      ),
      tableHeadersConfig.rows.map(row => row.label),
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
        const timePeriod = (rowCol2 instanceof TimePeriod
          ? rowCol2
          : rowCol1) as TimePeriod;

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
            locationFilters.every(
              filter =>
                result.location[filter.level] &&
                result.location[filter.level].code === filter.value,
            )
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
        rows={rows}
        ref={dataTableRef}
        footnotes={subjectMeta.footnotes}
      />
    );
  },
);

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
