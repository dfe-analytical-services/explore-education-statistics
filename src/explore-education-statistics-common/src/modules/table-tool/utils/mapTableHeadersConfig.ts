import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import { FilterOption } from '@common/modules/table-tool/services/tableBuilderService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
import compact from 'lodash/compact';

export interface UnmappedTableHeadersConfig {
  columnGroups: FilterOption[][];
  columns: FilterOption[];
  rowGroups: FilterOption[][];
  rows: FilterOption[];
}

/**
 * This function remaps the config filters into
 * full filter using the subjectMeta.
 */
export default function mapTableHeadersConfig(
  { columns, rows, columnGroups, rowGroups }: UnmappedTableHeadersConfig,
  fullTableSubjectMeta: FullTableMeta,
): TableHeadersConfig {
  let mappedRows: (TimePeriodFilter | Indicator | undefined)[];
  let mappedColumns: (TimePeriodFilter | Indicator | undefined)[];

  const mapTimePeriod = ({ value }: FilterOption) => {
    const filter = fullTableSubjectMeta.timePeriodRange.find(
      timePeriod => value === `${timePeriod.year}_${timePeriod.code}`,
    ) as TimePeriodFilter;

    return filter ? new TimePeriodFilter(filter) : undefined;
  };

  const mapIndicator = ({ value }: FilterOption) => {
    const filter = fullTableSubjectMeta.indicators.find(
      indicator => indicator.value === value,
    ) as Indicator;
    return filter ? new Indicator(filter) : undefined;
  };

  // If NaN then time periods are columns
  if (columns.length > 0 && Number.isNaN(Number(columns[0].value))) {
    mappedColumns = columns.map(mapTimePeriod);
    mappedRows = rows.map(mapIndicator);
  } else {
    mappedRows = rows.map(mapTimePeriod);
    mappedColumns = columns.map(mapIndicator);
  }

  // rowGroups/columnGroups can only be filters and locations
  const locationAndFilterGroups: (LocationFilter | CategoryFilter)[][] = [
    ...Object.values(
      transformTableMetaFiltersToCategoryFilters(fullTableSubjectMeta.filters),
    ),
    fullTableSubjectMeta.locations,
  ];

  const mapOptionGroupsToFilterGroups = (
    optionsGroups: FilterOption[][],
  ): (LocationFilter | CategoryFilter)[][] =>
    optionsGroups.map(optionGroup => {
      const currentIndex = locationAndFilterGroups.findIndex(options =>
        options.find(element =>
          element ? element.value === optionGroup[0].value : false,
        ),
      );

      if (currentIndex === -1) {
        return [];
      }

      return compact(
        optionGroup.map(
          option =>
            locationAndFilterGroups[currentIndex].find(
              element =>
                element.value === option.value &&
                // check for matching location level
                ((element as LocationFilter).level
                  ? (element as LocationFilter).level ===
                    (option as LocationFilter).level
                  : true),
            ) as LocationFilter | CategoryFilter,
        ),
      );
    });

  return {
    columns: compact(mappedColumns),
    columnGroups: mapOptionGroupsToFilterGroups(columnGroups),
    rows: compact(mappedRows),
    rowGroups: mapOptionGroupsToFilterGroups(rowGroups),
  };
}
