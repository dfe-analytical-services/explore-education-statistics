import cartesian from '@common/lib/utils/cartesian';
import formatPretty from '@common/lib/utils/number/formatPretty';
import commaList from '@common/lib/utils/string/commaList';
import {
  FilterOption,
  IndicatorOption,
  TableData,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import sortBy from 'lodash/sortBy';
import React, { memo, useEffect, useRef, useState } from 'react';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';
import TableHeadersForm, { TableHeadersFormValues } from './TableHeadersForm';

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

  const [tableHeaders, setTableHeaders] = useState<TableHeadersFormValues>({
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  });

  useEffect(() => {
    const sortedFilters = sortBy(Object.values(filters), [
      options => options.length,
    ]);

    const halfwayIndex = Math.floor(sortedFilters.length / 2);
    const columnGroups = sortedFilters.slice(0, halfwayIndex);
    const rowGroups = sortedFilters.slice(halfwayIndex);

    setTableHeaders({
      columnGroups,
      rowGroups,
      columns: timePeriods,
      rows: indicators,
    });
  }, [filters, timePeriods, indicators]);

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

      const aggregateFilters = [
        ...rowFilterCombination.slice(0, -1),
        ...columnFilterCombination.slice(0, -1),
      ];

      const matchingResult = results.find(result => {
        return (
          aggregateFilters.every(filter => {
            return result.filters.includes(filter.value);
          }) && result.timePeriod === time.value
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
