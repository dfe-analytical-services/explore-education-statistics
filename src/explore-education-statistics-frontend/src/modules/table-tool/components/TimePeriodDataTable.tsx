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
import FixedHeaderGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from './FixedHeaderGroupedDataTable';
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
                  [colGroup.value, rowGroup.value].includes(filter),
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
