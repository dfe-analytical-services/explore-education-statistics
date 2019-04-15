import { DataTableResult } from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import isEqual from 'lodash/isEqual';
import React, { useState } from 'react';
import FixedHeaderGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from 'src/prototypes/table-tool/components/FixedHeaderGroupedDataTable';
import {
  FilterOption,
  IndicatorOption,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import TableHeadersForm from 'src/prototypes/table-tool/components/TableHeadersForm';

interface Props {
  filters: {
    indicators: IndicatorOption[];
    categorical: {
      [key: string]: FilterOption[];
    };
    timePeriods: TimePeriod[];
  };
  results: DataTableResult[];
}

const TimePeriodDataTable = (props: Props) => {
  const { filters, results } = props;
  const { categorical, indicators, timePeriods } = filters;

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

  interface TableHeaders {
    columnGroups: FilterOption[];
    columns: TimePeriod[];
    rowGroups: FilterOption[];
    rows: IndicatorOption[];
  }

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
      columnGroups: categorical.schoolTypes,
      columns: timePeriods,
      rowGroups: categorical.characteristics,
      rows: indicators,
    });
  }

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
            return '--';
          }

          const value = Number(matchingResult.indicators[row.value]);

          if (Number.isNaN(value)) {
            return '--';
          }

          return `${value.toLocaleString('en-GB')}${row.unit}`;
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
        onSubmit={value => {
          setTableHeaders(value as TableHeaders);
        }}
      />

      <FixedHeaderGroupedDataTable
        caption={caption}
        headers={headerRow}
        rowGroups={groupedData}
      />
    </div>
  );
};

export default TimePeriodDataTable;
