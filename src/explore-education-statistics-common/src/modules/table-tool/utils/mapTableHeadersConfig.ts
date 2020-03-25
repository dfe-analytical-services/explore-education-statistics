import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import { UnmappedTableHeadersConfig } from '@common/modules/table-tool/services/permalinkService';
import { FilterOption } from '@common/modules/table-tool/services/tableBuilderService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
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

  if (columns.length > 0 && isTimePeriod(columns[0].value)) {
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
            locationAndFilterGroups[currentIndex].find(filter => {
              const hasMatch =
                filter.value === option.value && filter.label === option.label;

              // The filter might be a location so just cast
              // these as variables for convenience.
              const locationOption = option as LocationFilter;
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
