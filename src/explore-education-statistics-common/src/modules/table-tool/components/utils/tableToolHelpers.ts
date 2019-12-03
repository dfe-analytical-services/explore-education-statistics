import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  LocationLevelKeysEnum,
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
import { LocationsFormValues } from '@common/modules/table-tool/components/LocationFiltersForm';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import mapOptionValues from '@common/modules/table-tool/components/utils/mapOptionValues';
import { Dictionary } from '@common/types';

import mapValues from 'lodash/mapValues';
import { SortableOption } from '@common/modules/table-tool/components/FormSortableList';
import { DataBlockMetadata } from '@common/services/dataBlockService';

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
    Object.values(filterValue.options)
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
    optionsGroups: SortableOption[][],
  ): (LocationFilter | CategoryFilter)[][] =>
    optionsGroups.map(optionGroup => {
      const currentIndex = locationAndFilterGroups.findIndex(options =>
        options.find(element => element.value === optionGroup[0].value),
      );

      if (currentIndex === -1) return [];

      return optionGroup
        .map(
          ({ value }) =>
            locationAndFilterGroups[currentIndex].find(
              element => element.value === value,
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
    locations: Dictionary<LocationFilter[]>;
  } & DateRangeState,
): TableDataQuery => {
  if (!startYear || !startCode || !endYear || !endCode) {
    throw new Error('Missing required timePeriod parameters');
  }

  return {
    ...mapValues(locations, locationLevel =>
      locationLevel.map(location => location.value),
    ),
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

export const mapLocations = (
  selectedLocations: LocationsFormValues,
  locationsMeta: PublicationSubjectMeta['locations'],
) => {
  return mapValuesWithKeys(
    selectedLocations,
    (locationLevel, locationOptions) =>
      locationOptions
        .map(location =>
          locationsMeta[locationLevel].options.find(
            option => option.value === location,
          ),
        )
        .filter(option => typeof option !== 'undefined')
        .map(
          option => new LocationFilter(option as FilterOption, locationLevel),
        ),
  );
};

export const getSelectedLocationsForQuery = (
  locationQuery: Dictionary<string[] | undefined>,
) =>
  mapValuesWithKeys(
    LocationLevelKeysEnum,
    (key, _) => locationQuery[key] || [],
  );

export const getFiltersForTableGeneration = (
  { filters: metaFilters }: PublicationSubjectMeta,
  { filters }: FormValues,
) => {
  const filtersByValue = mapValues(metaFilters, value =>
    mapOptionValues(value.options),
  );

  return mapValuesWithKeys(filters, (filterGroup, selectedFilters) =>
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
  locations: Dictionary<LocationFilter[]>,
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

export type OptionalTableDataQuery =
  | { [K in keyof TableDataQuery]?: TableDataQuery[K] }
  | undefined;

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

export const validateAndPopulateLocations = (
  initialQuery: TableDataQuery,
): OptionalTableDataQuery => {
  const initialLocations: Dictionary<string[]> = Object.entries(
    mapValuesWithKeys(
      LocationLevelKeysEnum,
      keyName => (initialQuery as Dictionary<string[]>)[keyName],
    ),
  ).reduce(
    (filtered, [key, value]) =>
      value ? { ...filtered, [key]: [...value] } : filtered,
    {},
  );

  // check if any are actually set to validate if it's actually valid
  const allLocations = ([] as string[]).concat(
    ...Object.values(initialLocations),
  );
  if (allLocations.length === 0) return undefined;

  return initialLocations;
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
  if (
    initialQuery.filters.length === 0 ||
    initialQuery.indicators.length === 0
  ) {
    return undefined;
  }

  return {
    filters: [...initialQuery.filters],
    indicators: [...initialQuery.indicators],
  };
};

const validationFunctions = [
  initialiseQuery,
  validateAndPopulateSubject,
  validateAndPopulateLocations,
  validateAndPopulateDateRange,
  validateAndPopulateFiltersAndIndicators,
];

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
  let locations: Dictionary<LocationFilter[]> = {};
  let dateRange: DateRangeState = {};
  let initialStep = 1;
  let subjectMeta: PublicationSubjectMeta;

  if (initialQuery) {
    let buildNewQuery: TableDataQuery = {
      subjectId: '',
      filters: [],
      indicators: [],
    };

    validationFunctions.every((fn, idx) => {
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

    if (initialStep > 2)
      locations = mapLocations(
        getSelectedLocationsForQuery(buildNewQuery),
        subjectMeta.locations,
      );

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
