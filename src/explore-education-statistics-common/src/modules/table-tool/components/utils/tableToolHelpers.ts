import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  locationLevelKeys,
  LocationLevelKeys,
  PublicationSubjectMeta,
  TableDataQuery,
  TimeIdentifier,
} from '@common/modules/full-table/services/tableBuilderService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import {
  FullTable,
  FullTableMeta,
} from '@common/modules/full-table/types/fullTable';
import { mapFullTable } from '@common/modules/full-table/utils/mapPermalinks';
import getDefaultTableHeaderConfig, {
  TableHeadersConfig,
} from '@common/modules/full-table/utils/tableHeaders';
import { FormValues } from '@common/modules/table-tool/components/FiltersForm';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import mapOptionValues from '@common/modules/table-tool/components/utils/mapOptionValues';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { Dictionary, PartialRecord } from '@common/types';

import mapValues from 'lodash/mapValues';
import sortBy from 'lodash/sortBy';

export interface DateRangeState {
  startYear?: number;
  startCode?: TimeIdentifier;
  endYear?: number;
  endCode?: TimeIdentifier;
}

export const transformTableMetaFiltersToCategoryFilters = (
  filters: DataBlockMetadata['filters'] | FullTableMeta['filters'],
): Dictionary<CategoryFilter[]> => {
  return mapValuesWithKeys(filters, (filterKey, filterValue) =>
    sortBy(Object.values(filterValue.options), o => o.label)
      .flatMap(options => {
        return options.options.map(option => ({
          ...option,
          filterGroup: options.label,
        }));
      })
      .map(
        filter =>
          new CategoryFilter(filter, filter.value === filterValue.totalValue),
      ),
  );
};

export const reverseMapTableHeadersConfig = (
  { columns, rows, columnGroups, rowGroups }: TableHeadersFormValues,
  fullTableSubjectMeta: FullTableMeta,
): TableHeadersConfig => {
  /**
   * config filters only contain `values`.
   * This function remaps the config filters into full filters, using the subjectMeta
   */
  let mappedRows: (TimePeriodFilter | Indicator)[] = [];
  let mappedColumns: (TimePeriodFilter | Indicator)[] = [];

  const initialValue = columns.length > 0 && columns[0].value;
  // rows/columns can only be TimePeriods / Indicators
  if (Number.isNaN(Number(initialValue))) {
    // if NaN then timePeriod
    mappedColumns = columns
      .map(({ value }) => {
        const tp = fullTableSubjectMeta.timePeriodRange.find(
          timePeriod => value === `${timePeriod.year}_${timePeriod.code}`,
        ) as TimePeriodFilter;

        if (tp) {
          return new TimePeriodFilter(tp);
        }
        return tp;
      })
      .filter(_ => _ !== undefined);

    mappedRows = rows
      .map(({ value }) => {
        const i = fullTableSubjectMeta.indicators.find(
          indicator => indicator.value === value,
        ) as Indicator;
        if (i) return new Indicator({ ...i });
        return i;
      })
      .filter(_ => _ !== undefined);
  } else {
    mappedRows = rows
      .map(({ value }) => {
        const tp = fullTableSubjectMeta.timePeriodRange.find(
          timePeriod => value === `${timePeriod.year}_${timePeriod.code}`,
        ) as TimePeriodFilter;
        if (tp) {
          return new TimePeriodFilter(tp);
        }
        return tp;
      })
      .filter(_ => _ !== undefined);
    mappedColumns = columns
      .map(({ value }) => {
        const i = fullTableSubjectMeta.indicators.find(
          indicator => indicator.value === value,
        ) as Indicator;
        if (i) return new Indicator(i);
        return i;
      })
      .filter(_ => _ !== undefined);
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

      if (currentIndex === -1) return [];

      return optionGroup
        .map(
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
        )
        .filter(_ => _ !== undefined);
    });

  return {
    columns: mappedColumns.filter(_ => _ !== undefined),
    columnGroups: mapOptionGroupsToFilterGroups(columnGroups),
    rows: mappedRows.filter(_ => _ !== undefined),
    rowGroups: mapOptionGroupsToFilterGroups(rowGroups),
  };
};

export const createQuery = (
  filters: Dictionary<CategoryFilter[]>,
  indicators: Indicator[],
  {
    subjectId,
    startYear,
    startCode,
    endYear,
    endCode,
    locations,
  }: {
    subjectId: string;
    locations: PartialRecord<LocationLevelKeys, string[]>;
  } & DateRangeState,
): TableDataQuery => {
  if (!startYear || !startCode || !endYear || !endCode) {
    throw new Error('Missing required timePeriod parameters');
  }

  return {
    ...locations,
    subjectId,
    indicators: indicators.map(indicator => indicator.value),
    filters: Object.values(filters).flatMap(categoryFilters =>
      categoryFilters.flatMap(filter => filter.value),
    ),
    timePeriod: {
      startYear,
      startCode,
      endYear,
      endCode,
    },
  };
};

export const getFiltersForTableGeneration = (
  { filters: metaFilters }: PublicationSubjectMeta,
  { filters }: FormValues,
) => {
  const filtersByValue = mapValues(metaFilters, value =>
    mapOptionValues(value.options),
  );

  return mapValues(filters, (selectedFilters, filterGroup) =>
    selectedFilters.map(
      filter =>
        new CategoryFilter(
          filtersByValue[filterGroup][filter],
          filter === metaFilters[filterGroup].totalValue,
        ),
    ),
  );
};

export const getIndicatorsForTableGeneration = (
  { indicators: indicatorsMeta }: PublicationSubjectMeta,
  { indicators }: FormValues,
) => {
  const indicatorsByValue = mapOptionValues<IndicatorOption>(indicatorsMeta);

  return indicators.map(
    indicator => new Indicator(indicatorsByValue[indicator]),
  );
};

export const queryForTable = async (
  query: TableDataQuery,
  releaseId?: string,
): Promise<FullTable> => {
  if (releaseId) {
    return tableBuilderService.getTableDataForRelease(query, releaseId);
  }
  return tableBuilderService.getTableData(query);
};

export const tableGeneration = async (
  dateRange: DateRangeState,
  subjectMeta: PublicationSubjectMeta,
  values: FormValues,
  subjectId: string,
  locations: PartialRecord<LocationLevelKeys, string[]>,
  releaseId: string | undefined,
): Promise<{
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  query: TableDataQuery;
}> => {
  const query: TableDataQuery = createQuery(
    getFiltersForTableGeneration(subjectMeta, values),
    getIndicatorsForTableGeneration(subjectMeta, values),
    {
      subjectId,
      locations,
      ...dateRange,
    },
  );

  const unmappedCreatedTable = await queryForTable(query, releaseId);
  const table = mapFullTable(unmappedCreatedTable);

  const tableHeaders = getDefaultTableHeaderConfig(table.subjectMeta);

  return {
    table,
    tableHeaders,
    query,
  };
};

export type OptionalTableDataQuery = Partial<TableDataQuery> | undefined;

export const initialiseQuery = (_: TableDataQuery): OptionalTableDataQuery => {
  return {};
};

export const validateAndPopulateSubject = (
  initialQuery: TableDataQuery,
): OptionalTableDataQuery => {
  if (!initialQuery.subjectId) return undefined;

  return {
    subjectId: initialQuery.subjectId,
  };
};

const getLocationOptions = (
  initialQuery?: TableDataQuery,
): PartialRecord<LocationLevelKeys, string[]> => {
  if (!initialQuery) {
    return {};
  }

  return Object.fromEntries(
    Object.entries(initialQuery).filter(([key]) =>
      locationLevelKeys.includes(key as LocationLevelKeys),
    ),
  );
};

export const validateAndPopulateLocations = (
  locations: PartialRecord<LocationLevelKeys, string[]>,
) => {
  return (): OptionalTableDataQuery => {
    // Validate there are any locations at all
    return Object.values(locations).some(level => level?.length)
      ? locations
      : undefined;
  };
};

export const validateAndPopulateDateRange = (
  initialQuery: TableDataQuery,
): OptionalTableDataQuery => {
  const newDateRange: DateRangeState = {
    ...initialQuery.timePeriod,
  };

  if (
    newDateRange.endCode === undefined ||
    newDateRange.endYear === undefined ||
    newDateRange.startCode === undefined ||
    newDateRange.startYear === undefined
  ) {
    return undefined;
  }

  return {
    timePeriod: initialQuery.timePeriod,
  };
};

export const validateAndPopulateFiltersAndIndicators = (
  initialQuery: TableDataQuery,
): OptionalTableDataQuery => {
  return {
    filters: [...(initialQuery.filters || [])],
    indicators: [...(initialQuery.indicators || [])],
  };
};

export const getDefaultSubjectMeta = () => ({
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
  locations: {},
  indicators: {},
  filters: {},
});

export const initialiseFromInitialQuery = async (
  releaseId?: string,
  initialQuery?: TableDataQuery,
) => {
  let query: TableDataQuery | undefined;
  let createdTable: FullTable | undefined;

  let subjectId = '';
  const locations: PartialRecord<
    LocationLevelKeys,
    string[]
  > = getLocationOptions(initialQuery);

  let dateRange: DateRangeState = {};
  let initialStep = 1;
  let subjectMeta: PublicationSubjectMeta;

  if (initialQuery) {
    let buildNewQuery: TableDataQuery = {
      subjectId: '',
      filters: [],
      indicators: [],
    };

    [
      initialiseQuery,
      validateAndPopulateSubject,
      validateAndPopulateLocations(locations),
      validateAndPopulateDateRange,
      validateAndPopulateFiltersAndIndicators,
    ].every((fn, idx) => {
      const q = fn(initialQuery);

      if (q === undefined) return false;

      initialStep = idx + 1;
      buildNewQuery = { ...buildNewQuery, ...q };

      return true;
    });

    if (initialStep === 5) {
      createdTable = await queryForTable(buildNewQuery, releaseId);
    }

    const queryForEntireSubject = {
      subjectId: initialQuery.subjectId,
    };

    subjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      queryForEntireSubject,
    );

    // eslint-disable-next-line prefer-destructuring
    if (initialStep > 1) subjectId = buildNewQuery.subjectId;

    if (initialStep > 3) dateRange = { ...buildNewQuery.timePeriod };

    query = buildNewQuery;
  } else {
    query = undefined;
    subjectMeta = getDefaultSubjectMeta();
  }

  return {
    query,
    validInitialQuery: query,
    subjectId,
    locations,
    dateRange,
    subjectMeta,
    createdTable,
    initialStep,
  };
};
