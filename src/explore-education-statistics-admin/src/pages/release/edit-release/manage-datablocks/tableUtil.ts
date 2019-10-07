/* eslint-disable */
import { SortableOption } from '@common/modules/table-tool/components/FormSortableList';
import {
  FullTable,
  FullTableMeta,
} from '@common/modules/full-table/types/fullTable';
import { TableHeadersConfig } from '@common/modules/full-table/utils/tableHeaders';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import {
  DataBlockResponse,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import { Dictionary } from '@common/types/util';

export interface UnmappedTableHeadersConfig {
  columnGroups: SortableOption[][];
  columns: SortableOption[];
  rowGroups: SortableOption[][];
  rows: SortableOption[];
}

const transformTableMetaFiltersToCategoryFilters = (
  filters: DataBlockMetadata['filters'],
): Dictionary<CategoryFilter[]> => {
  return mapValuesWithKeys(filters, (filterKey, filterValue) =>
    Object.values(filterValue.options)
      .flatMap(options => options.options)
      .map(
        filter =>
          new CategoryFilter(filter, filter.value === filterValue.totalValue),
      ),
  );
};

export const reverseMapTableHeadersConfigForDataBlock = (
  { columns, rows, columnGroups, rowGroups }: UnmappedTableHeadersConfig,
  fullTableSubjectMeta: DataBlockMetadata,
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
      return fullTableSubjectMeta.timePeriods[value] as TimePeriodFilter;
    });
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.indicators[value] as Indicator;
    });
  } else {
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.timePeriods[value] as TimePeriodFilter;
    });
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.indicators[value] as Indicator;
    });
  }

  const categoryFilters: Dictionary<
    CategoryFilter[]
  > = transformTableMetaFiltersToCategoryFilters(fullTableSubjectMeta.filters);
  // rowGroups/columnGroups can only be filters and locations
  const locationAndFilterGroups: (LocationFilter | CategoryFilter)[][] = [
    ...Object.values(categoryFilters),
    Object.values(fullTableSubjectMeta.locations),
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
  unmappedFullTable: DataBlockResponse,
): FullTable => {
  const subjectMeta = unmappedFullTable.metaData || {
    indicators: {},
    locations: {},
    timePeriodRange: {},
  };

  return {
    results: unmappedFullTable.result,
    subjectMeta: {
      subjectName: '',
      publicationName: 'Test',
      footnotes: [],
      filters: {},
      ...unmappedFullTable.metaData,
      indicators: Object.values(subjectMeta.indicators).map(
        indicator => new Indicator(indicator),
      ),
      locations: Object.values(subjectMeta.locations).map(
        location => new LocationFilter(location, location.level),
      ),
      timePeriodRange: Object.values(subjectMeta.timePeriods).map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
};
