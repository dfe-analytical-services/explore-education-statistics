import cartesian from '@common/lib/utils/cartesian';
import commaList from '@common/lib/utils/string/commaList';
import {
  FilterOption,
  IndicatorOption,
  TableData,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import sortBy from 'lodash/sortBy';
import React, { memo, useEffect, useRef, useState } from 'react';
import FixedHeaderGroupedDataTable from './FixedHeaderGroupedDataTable';
import TableHeadersForm from './TableHeadersForm';

interface TableHeaders {
  columnGroups: FilterOption[];
  columns: TimePeriod[];
  rowGroups: FilterOption[];
  rows: IndicatorOption[];
}

interface Props {
  indicators: IndicatorOption[];
  filters: Dictionary<FilterOption[]>;
  timePeriods: TimePeriod[];
  publicationName: string;
  subjectName: string;
  locations: Dictionary<FilterOption[]>;
  results: TableData['result'];
}

const TimePeriodDataTable = ({
  filters,
  timePeriods,
  indicators,
  publicationName,
  subjectName,
  locations,
  results,
}: Props) => {
  const dataTableRef = useRef<HTMLTableElement>(null);

  const [columnGroups, rowGroups] = sortBy(Object.values(filters), [
    options => options.length,
  ]);

  const [tableHeaders, setTableHeaders] = useState<TableHeaders>({
    columnGroups,
    rowGroups,
    columns: timePeriods,
    rows: indicators,
  });

  useEffect(() => {
    setTableHeaders({
      columnGroups,
      rowGroups,
      columns: timePeriods,
      rows: indicators,
    });
  }, [columnGroups, rowGroups, timePeriods, indicators]);

  const startLabel = timePeriods[0].label;
  const endLabel = timePeriods[timePeriods.length - 1].label;

  const locationLabels = Object.values(locations).flatMap(locationOptions =>
    locationOptions.map(location => location.label),
  );

  const timePeriodString =
    startLabel === endLabel
      ? ` for ${startLabel}`
      : ` between ${startLabel} and ${endLabel}`;

  const caption = `Table showing '${subjectName}' from '${publicationName}' in ${commaList(
    locationLabels,
  )} ${timePeriodString}`;

  const columnHeaders: string[][] = [
    tableHeaders.columnGroups.map(colGroup => colGroup.label),
    tableHeaders.columns.map(column => column.label),
  ];

  const rowHeaders: string[][] = [
    tableHeaders.rowGroups.map(rowGroup => rowGroup.label),
    tableHeaders.rows.map(row => row.label),
  ];

  const rowHeadersCartesian = cartesian(
    tableHeaders.rowGroups,
    tableHeaders.rows,
  );

  const columnHeadersCartesian = cartesian(
    tableHeaders.columnGroups,
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

      const aggregateFilters = [
        ...rowFilterCombination.slice(0, -1),
        ...columnFilterCombination.slice(0, -1),
      ];

      const matchingResult = results.find(result => {
        return (
          aggregateFilters.every(filter => {
            return result.filters.includes(filter.value);
          }) &&
          result.timeIdentifier === time.code &&
          result.year === time.year
        );
      });

      if (!matchingResult) {
        return 'n/a';
      }

      const rawValue = matchingResult.measures[indicatorOption.value];
      const numberValue = Number(rawValue);

      if (Number.isNaN(numberValue)) {
        return rawValue;
      }

      const decimals = rawValue.split('.')[1];
      const decimalPlaces = decimals ? decimals.length : 0;

      return `${numberValue.toLocaleString('en-GB', {
        maximumFractionDigits: decimalPlaces,
        minimumFractionDigits: decimalPlaces,
      })}${indicatorOption.unit}`;
    });
  });

  return (
    <div>
      <TableHeadersForm
        initialValues={tableHeaders}
        onSubmit={value => {
          setTableHeaders(value as TableHeaders);

          if (dataTableRef.current) {
            dataTableRef.current.scrollIntoView({
              behavior: 'smooth',
              block: 'start',
            });
          }
        }}
      />

      <FixedHeaderGroupedDataTable
        caption={caption}
        columnHeaders={columnHeaders}
        rowHeaders={rowHeaders}
        rows={rows}
        ref={dataTableRef}
      />
    </div>
  );
};

export default memo(TimePeriodDataTable);
