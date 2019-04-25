import { DataTableResult } from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import FixedHeaderGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from '@frontend/prototypes/table-tool/components/FixedHeaderGroupedDataTable';
import {
  FilterOption,
  IndicatorOption,
} from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import TableHeadersForm from '@frontend/prototypes/table-tool/components/TableHeadersForm';
import isEqual from 'lodash/isEqual';
import React, { useRef, useState } from 'react';

interface TableHeaders {
  columnGroups: FilterOption[];
  columns: TimePeriod[];
  rowGroups: FilterOption[];
  rows: IndicatorOption[];
}

interface Props {
  indicators: IndicatorOption[];
  filters: {
    [key: string]: FilterOption[];
  };
  timePeriods: TimePeriod[];
  results: DataTableResult[];
}

const TimePeriodDataTable = ({
  filters,
  timePeriods,
  indicators,
  results,
}: Props) => {
  const dataTableRef = useRef<HTMLTableElement>(null);

  const [prevFilters, setPrevFilters] = useState<Props['filters']>();
  const [tableHeaders, setTableHeaders] = useState<TableHeaders>({
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  });

  if (!isEqual(prevFilters, filters)) {
    setPrevFilters(filters);
    setTableHeaders({
      columnGroups: filters.schoolTypes,
      columns: timePeriods,
      rowGroups: filters.characteristics,
      rows: indicators,
    });
  }

  const startLabel = timePeriods[0].label;
  const endLabel = timePeriods[timePeriods.length - 1].label;

  const caption =
    startLabel !== endLabel
      ? `Comparing statistics for ${startLabel}`
      : `Comparing statistics between ${startLabel} and ${endLabel}`;

  // TODO: Remove this when timePeriod API finalised
  const formatToAcademicYear = (year: number) => {
    const nextYear = year + 1;
    return parseInt(`${year}${`${nextYear}`.substring(2, 4)}`, 0);
  };

  const headerRow: HeaderGroup[] = tableHeaders.columnGroups.map(
    columnGroup => {
      return {
        columns: tableHeaders.columns.map(column => column.label),
        label: columnGroup.label,
      };
    },
  );

  const groupedData: RowGroup[] = tableHeaders.rowGroups.map(rowGroup => {
    const rows = tableHeaders.rows.map(row => {
      const columnGroups = tableHeaders.columnGroups.map(colGroup =>
        tableHeaders.columns.map(column => {
          // TODO: Figure out how to make filter criteria dynamic
          const matchingResult = results.find(result => {
            return Boolean(
              result.indicators[row.value] !== undefined &&
                result.characteristic &&
                result.characteristic.name === rowGroup.value &&
                result.timePeriod === formatToAcademicYear(column.year) &&
                result.schoolType === colGroup.value,
            );
          });

          if (!matchingResult) {
            return 'n/a';
          }

          const rawValue = matchingResult.indicators[row.value];
          const parsedValue = Number(rawValue);

          if (Number.isNaN(parsedValue)) {
            return rawValue;
          }

          return `${parsedValue.toLocaleString('en-GB')}${row.unit}`;
        }),
      );

      return {
        columnGroups,
        label: row.label,
      };
    });

    return {
      rows,
      label: rowGroup.label,
    };
  });

  return (
    <div>
      <TableHeadersForm
        filters={filters}
        indicators={indicators}
        timePeriods={timePeriods}
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
        headers={headerRow}
        rowGroups={groupedData}
        ref={dataTableRef}
      />
    </div>
  );
};

export default TimePeriodDataTable;
