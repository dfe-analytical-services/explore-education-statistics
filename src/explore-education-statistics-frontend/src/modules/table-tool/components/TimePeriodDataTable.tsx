import cartesian from '@common/lib/utils/cartesian';
import formatPretty from '@common/lib/utils/number/formatPretty';
import {
  IndicatorOption,
  TableData,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import DataTableCaption from '@frontend/modules/table-tool/components/DataTableCaption';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import sortBy from 'lodash/sortBy';
import React, { memo, useEffect, useRef, useState } from 'react';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';
import TableHeadersForm, { TableHeadersFormValues } from './TableHeadersForm';

interface Props {
  indicators: Indicator[];
  filters: Dictionary<CategoryFilter[]>;
  timePeriods: TimePeriod[];
  publicationName: string;
  subjectName: string;
  locations: Dictionary<LocationFilter[]>;
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

  useEffect(() => {
    const sortedFilters = sortBy(
      Object.values({
        ...filters,
        locations: Object.values(locations).flat(),
      }),
      [options => options.length],
    );

    const halfwayIndex = Math.floor(sortedFilters.length / 2);
    const columnGroups = sortedFilters.slice(0, halfwayIndex);
    const rowGroups = sortedFilters.slice(halfwayIndex);

    setTableHeaders({
      columnGroups,
      rowGroups,
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
    const indicatorOption = rowFilterCombination[
      rowFilterCombination.length - 1
    ] as IndicatorOption;

    return columnHeadersCartesian.map(columnFilterCombination => {
      const time = columnFilterCombination[
        columnFilterCombination.length - 1
      ] as TimePeriod;

      const combinationFilters: LocationFilter[] | CategoryFilter[] = [
        ...rowFilterCombination.slice(0, -1),
        ...columnFilterCombination.slice(0, -1),
      ];

      const categoryFilters = combinationFilters.filter(
        filter => filter instanceof CategoryFilter,
      );

      const locationFilters = combinationFilters.filter(
        filter => filter instanceof LocationFilter,
      ) as LocationFilter[];

      const matchingResult = results.find(result => {
        return (
          categoryFilters.every(filter =>
            result.filters.includes(filter.value),
          ) &&
          result.timeIdentifier === time.code &&
          result.year === time.year &&
          locationFilters.every(
            filter => result.location[filter.level].code === filter.value,
          )
        );
      });

      if (!matchingResult) {
        return 'n/a';
      }

      const value = matchingResult.measures[indicatorOption.value];

      if (Number.isNaN(Number(value))) {
        return value;
      }

      return `${formatPretty(value)}${indicatorOption.unit}`;
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
