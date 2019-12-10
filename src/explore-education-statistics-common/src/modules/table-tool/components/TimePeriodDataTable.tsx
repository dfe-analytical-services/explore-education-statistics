import WarningMessage from '@common/components/WarningMessage';
import cartesian from '@common/lib/utils/cartesian';
import formatPretty from '@common/lib/utils/number/formatPretty';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import { RowHeaderType } from '@common/modules/table-tool/components/MultiHeaderTable';
import camelCase from 'lodash/camelCase';
import last from 'lodash/last';
import React, { forwardRef, memo } from 'react';
import DataTableCaption from './DataTableCaption';
import FixedMultiHeaderDataTable from './FixedMultiHeaderDataTable';
import {
  SortableOptionWithGroup,
  TableHeadersFormValues,
} from './TableHeadersForm';
import { reverseMapTableHeadersConfig } from './utils/tableToolHelpers';

interface Props {
  fullTable: FullTable;
  tableHeadersConfig: TableHeadersFormValues;
}

const selectFilterGroup = (
  group?: string,
  previousGroup?: string,
): RowHeaderType => {
  if (group) {
    if (group === previousGroup) return undefined;
    if (group !== 'Default') return group;
  }
  return '';
};

export const createHeadersFromGroups = (
  groups: SortableOptionWithGroup[][],
): RowHeaderType[][] => {
  groups.flatMap(group => {
    group.flatMap(filter => {
      if (
        filter.level &&
        (filter.level === 'country' ||
          filter.level === 'localAuthority' ||
          filter.level === 'localAuthorityDistrict') &&
        group.length === 1
      ) {
        // eslint-disable-next-line no-param-reassign
        filter.label = '';
      }
    });
  });
  return groups.flatMap(rowGroup =>
    rowGroup
      .reduce<[RowHeaderType[], RowHeaderType[]]>(
        ([b, c], group, index) => [
          (group.filterGroup && [
            ...b,
            selectFilterGroup(
              group.filterGroup,
              (index > 0 && rowGroup[index - 1].filterGroup) || undefined,
            ),
          ]) ||
            b,
          [...c, group.label],
        ],
        [[], []],
      )
      .filter(
        ary => ary.length > 0 && ary.filter(cell => !!cell).join('').length > 0,
      ),
  );
};

export const createHeaderIgnoreFromGroups = (
  groups: SortableOptionWithGroup[][],
): boolean[] => {
  return groups
    .flatMap(rowGroup =>
      rowGroup
        .reduce<[boolean[], boolean[]]>(
          ([b, c], group, index) => [
            (selectFilterGroup(
              group.filterGroup,
              (index > 0 && rowGroup[index - 1].filterGroup) || undefined,
            ) && [...b, true]) ||
              b,
            [...c, false],
          ],
          [[], []],
        )

        .filter(ary => ary.length > 0),
    )
    .map(group => group.includes(true));
};

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  (props: Props, dataTableRef) => {
    const { fullTable, tableHeadersConfig: unmappedHeaderConfig } = props;
    const { subjectMeta, results } = fullTable;

    const tableHeadersConfig = reverseMapTableHeadersConfig(
      unmappedHeaderConfig,
      fullTable.subjectMeta,
    ) as TableHeadersFormValues;

    if (results.length === 0) {
      return (
        <WarningMessage>
          A table could not be returned. There is no data for the options
          selected.
        </WarningMessage>
      );
    }

    const columnHeaders: RowHeaderType[][] = [
      ...createHeadersFromGroups(tableHeadersConfig.columnGroups),
      tableHeadersConfig.columns.map(column => column.label),
    ];

    const columnHeaderIsGroup: boolean[] = [
      ...createHeaderIgnoreFromGroups(tableHeadersConfig.columnGroups),
      false,
    ];

    const rowHeaders: RowHeaderType[][] = [
      ...createHeadersFromGroups(tableHeadersConfig.rowGroups),
      tableHeadersConfig.rows.map(row => row.label),
    ];

    const rowHeaderIsGroup: boolean[] = [
      ...createHeaderIgnoreFromGroups(tableHeadersConfig.rowGroups),
      false,
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
        columnHeaderIsGroup={columnHeaderIsGroup}
        rowHeaders={rowHeaders}
        rowHeaderIsGroup={rowHeaderIsGroup}
        rows={rows}
        ref={dataTableRef}
        footnotes={subjectMeta.footnotes}
      />
    );
  },
);

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
