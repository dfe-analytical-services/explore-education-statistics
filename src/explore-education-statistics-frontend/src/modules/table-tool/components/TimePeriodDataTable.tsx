import {
  FilterOption,
  IndicatorOption,
  TableData,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import FixedHeaderGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from '@frontend/modules/table-tool/components/FixedHeaderGroupedDataTable';
import TableHeadersForm from '@frontend/modules/table-tool/components/TableHeadersForm';
import sortBy from 'lodash/sortBy';
import React, { memo, useEffect, useRef, useState } from 'react';

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
  results: TableData['result'];
}

const TimePeriodDataTable = ({
  filters,
  timePeriods,
  indicators,
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

  const caption =
    startLabel === endLabel
      ? `Comparing statistics for ${startLabel}`
      : `Comparing statistics between ${startLabel} and ${endLabel}`;

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
      const columnGroupValues = tableHeaders.columnGroups.map(colGroup =>
        tableHeaders.columns.map(column => {
          const matchingResult = results.find(result => {
            return Boolean(
              result.measures[row.value] !== undefined &&
                result.filters.every(filter =>
                  [colGroup.value, rowGroup.value].includes(filter.toString()),
                ) &&
                result.timeIdentifier === column.code &&
                result.year === column.year,
            );
          });

          if (!matchingResult) {
            return 'n/a';
          }

          const rawValue = matchingResult.measures[row.value];
          const parsedValue = Number(rawValue);

          if (Number.isNaN(parsedValue)) {
            return rawValue;
          }

          return `${parsedValue.toLocaleString('en-GB')}${row.unit}`;
        }),
      );

      return {
        columnGroups: columnGroupValues,
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
        headers={headerRow}
        rowGroups={groupedData}
        ref={dataTableRef}
      />
    </div>
  );
};

export default memo(TimePeriodDataTable);
