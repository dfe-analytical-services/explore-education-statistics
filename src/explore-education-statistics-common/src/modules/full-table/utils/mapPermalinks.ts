import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '../types/filters';
import {
  UnmappedTableHeadersConfig,
  UnmappedFullTable,
  UnmappedPermalink,
  Permalink,
  SortableOption,
} from '../services/permalinkService';
import {
  transformTableMetaFiltersToCategoryFilters,
  TableHeadersConfig,
} from './tableHeaders';
import { FullTableMeta, FullTable } from '../types/fullTable';

const reverseMapTableHeadersConfig = (
  { columns, rows, columnGroups, rowGroups }: UnmappedTableHeadersConfig,
  fullTableSubjectMeta: FullTableMeta,
): TableHeadersConfig => {
  /**
   * config filters only contain `values`.
   * This function remaps the config filters into full filters, using the subjectMeta
   */
  let mappedRows: (TimePeriodFilter | Indicator)[] = [];
  let mappedColumns: (TimePeriodFilter | Indicator)[] = [];

  const initialValue = columns[0].value;
  // rows/columns can only be TimePeriods / Indicators
  if (Number.isNaN(Number(initialValue))) {
    // if NaN then timePeriod
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.timePeriodRange.find(
        timePeriod => `${timePeriod.year}_${timePeriod.code}` === value,
      ) as TimePeriodFilter;
    });
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.indicators.find(
        indicator => indicator.value === value,
      ) as Indicator;
    });
  } else {
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.timePeriodRange.find(
        timePeriod => `${timePeriod.year}_${timePeriod.code}` === value,
      ) as TimePeriodFilter;
    });
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.indicators.find(
        indicator => indicator.value === value,
      ) as Indicator;
    });
  }

  // rowGroups/columnGroups can only be filters and locations
  const locationAndFilterGroups: (LocationFilter | CategoryFilter)[][] = [
    ...Object.values(
      transformTableMetaFiltersToCategoryFilters(fullTableSubjectMeta.filters),
    ),
    fullTableSubjectMeta.locations,
  ];

  const mapOptionGroupsToFilterGroups = (
    optionsGroups: SortableOption[][],
  ): (LocationFilter | CategoryFilter)[][] =>
    optionsGroups.map(optionGroup => {
      const currentIndex = locationAndFilterGroups.findIndex(options =>
        options.find(element => element.value === optionGroup[0].value),
      );

      return optionGroup.map(
        ({ value }) =>
          locationAndFilterGroups[currentIndex].find(
            element => element.value === value,
          ) as LocationFilter | CategoryFilter,
      );
    });

  return {
    columns: mappedColumns,
    columnGroups: mapOptionGroupsToFilterGroups(columnGroups),
    rows: mappedRows,
    rowGroups: mapOptionGroupsToFilterGroups(rowGroups),
  };
};

export const mapFullTable = (
  unmappedFullTable: UnmappedFullTable,
): FullTable => {
  return {
    ...unmappedFullTable,
    subjectMeta: {
      ...unmappedFullTable.subjectMeta,
      indicators: unmappedFullTable.subjectMeta.indicators.map(
        indicator => new Indicator(indicator),
      ),
      locations: unmappedFullTable.subjectMeta.locations.map(
        location => new LocationFilter(location, location.level),
      ),
      timePeriodRange: unmappedFullTable.subjectMeta.timePeriodRange.map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
};

export const mapPermalink = (
  unmappedPermalink: UnmappedPermalink,
): Permalink => {
  const mappedFullTable = mapFullTable(unmappedPermalink.fullTable);

  return {
    ...unmappedPermalink,
    fullTable: mappedFullTable,
    configuration: {
      tableHeadersConfig: reverseMapTableHeadersConfig(
        unmappedPermalink.configuration.tableHeadersConfig,
        mappedFullTable.subjectMeta,
      ),
    },
  };
};
