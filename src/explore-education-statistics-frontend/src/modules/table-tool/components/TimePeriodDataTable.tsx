import cartesian from '@common/lib/utils/cartesian';
import formatPretty from '@common/lib/utils/number/formatPretty';
import { TableData } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import last from 'lodash/last';
import sortBy from 'lodash/sortBy';
import React, { memo, useEffect, useRef, useState } from 'react';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';
import TableHeadersForm, { TableHeadersFormValues } from './TableHeadersForm';

interface Props {
  indicators: Indicator[];
  filters: Dictionary<CategoryFilter[]>;
  timePeriods: TimePeriod[];
  publicationName: string;
  subjectName: string;
  locations: LocationFilter[];
  results: TableData['result'];
}

const TimePeriodDataTable = (props: Props) => {
  const { filters, timePeriods, locations, indicators, results } = props;

  const dataTableRef = useRef<HTMLTableElement>(null);

  const [tableHeaders, setTableHeaders] = useState<TableHeadersFormValues>({
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  });

  const removeSiblinglessTotalRows = (
    categoryFilters: Dictionary<CategoryFilter[]>,
  ): CategoryFilter[][] => {
    return Object.values(categoryFilters).filter(filter => {
      return filter.length > 1 || !filter[0].isTotal;
    });
  };

  useEffect(() => {
    const sortedFilters = sortBy(
      [...removeSiblinglessTotalRows(filters), locations],
      [options => options.length],
    );

    const halfwayIndex = Math.floor(sortedFilters.length / 2);

    setTableHeaders({
      columnGroups: sortedFilters.slice(0, halfwayIndex),
      rowGroups: sortedFilters.slice(halfwayIndex),
      columns: timePeriods,
      rows: indicators,
    });
  }, [filters, timePeriods, locations, indicators]);

  const columnHeaders: string[][] = [
    ...tableHeaders.columnGroups.map(colGroup =>
      colGroup.map(group => group.label),
    ),
    tableHeaders.columns.map(column => column.label),
  ];

  const rowHeaders: string[][] = [
    ...tableHeaders.rowGroups.map(rowGroup =>
      rowGroup.map(group => group.label),
    ),
    tableHeaders.rows.map(row => row.label),
  ];

  const rowHeadersCartesian = cartesian(
    ...tableHeaders.rowGroups,
    tableHeaders.rows,
  );

  const columnHeadersCartesian = cartesian(
    ...tableHeaders.columnGroups,
    tableHeaders.columns,
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
    <div>
      <TableHeadersForm
        initialValues={tableHeaders}
        onSubmit={value => {
          setTableHeaders(value);

          if (dataTableRef.current) {
            dataTableRef.current.scrollIntoView({
              behavior: 'smooth',
              block: 'start',
            });
          }
        }}
      />

      <FixedMultiHeaderDataTable
        caption={<DataTableCaption {...props} id="dataTableCaption" />}
        columnHeaders={columnHeaders}
        rowHeaders={rowHeaders}
        rows={rows}
        ref={dataTableRef}
      />
    </div>
  );
};

export default memo(TimePeriodDataTable);
