import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import {
  FilterOption,
  LocationOption,
} from '@common/services/tableBuilderService';
import isGuid from '@common/utils/string/isGuid';
import compact from 'lodash/compact';

const isTimePeriod = (value: string): boolean => {
  if (isGuid(value)) {
    return false;
  }

  return new RegExp(/^[0-9]{4,}_/).test(value);
};

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

  const mapTimePeriod = ({ value }: FilterOption) =>
    fullTableSubjectMeta.timePeriodRange.find(
      timePeriod => value === `${timePeriod.year}_${timePeriod.code}`,
    );

  const mapIndicator = ({ value }: FilterOption) =>
    fullTableSubjectMeta.indicators.find(
      indicator => indicator.value === value,
    );

  if (columns.length > 0 && isTimePeriod(columns[0].value)) {
    mappedColumns = columns.map(mapTimePeriod);
    mappedRows = rows.map(mapIndicator);
  } else {
    mappedRows = rows.map(mapTimePeriod);
    mappedColumns = columns.map(mapIndicator);
  }

  // rowGroups/columnGroups can only be filters and locations
  const locationAndFilterGroups: (LocationFilter | CategoryFilter)[][] = [
    ...Object.values(fullTableSubjectMeta.filters).map(
      filters => filters.options,
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
            locationAndFilterGroups[currentIndex].find(filter => {
              const hasMatch = filter.value === option.value;

              // The filter might be a location so just cast
              // these as variables for convenience.
              const locationOption = option as LocationOption;
              const locationFilter = filter as LocationFilter;

              if (locationOption.level && locationFilter.level) {
                return (
                  hasMatch && locationFilter.level === locationOption.level
                );
              }

              return hasMatch;
            }) as LocationFilter | CategoryFilter,
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
