import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption, IndicatorOption, LocationLevelKeysEnum,
  PublicationSubjectMeta,
  TableDataQuery,
} from '@common/modules/full-table/services/tableBuilderService';
import { CategoryFilter, Indicator, LocationFilter } from '@common/modules/full-table/types/filters';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import { mapFullTable } from '@common/modules/full-table/utils/mapPermalinks';
import getDefaultTableHeaderConfig, { TableHeadersConfig } from '@common/modules/full-table/utils/tableHeaders';
import { FormValues } from '@common/modules/table-tool/components/FiltersForm';
import { LocationsFormValues } from '@common/modules/table-tool/components/LocationFiltersForm';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import mapOptionValues from '@common/modules/table-tool/components/utils/mapOptionValues';
import { Dictionary } from '@common/types';

import mapValues from 'lodash/mapValues';


export interface DateRangeState {
  startYear?: number;
  startCode?: string;
  endYear?: number;
  endCode?: string;
}


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
) =>
  mapValuesWithKeys(selectedLocations, (locationLevel, locationOptions) =>
    locationOptions
      .map(location =>
        locationsMeta[locationLevel].options.find(
          option => option.value === location,
        ),
      )
      .filter(option => typeof option !== 'undefined')
      .map(option => new LocationFilter(option as FilterOption, locationLevel)),
  );

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
  table?: FullTable;
  tableHeaders?: TableHeadersConfig;
  query?: TableDataQuery;
}> => {
  const { startYear, startCode, endYear, endCode } = dateRange;

  if (!startYear || !startCode || !endYear || !endCode) {
    return {};
  }

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

export type OptionalTableDataQuery = { [K in keyof TableDataQuery]?: TableDataQuery[K] } | undefined;


export const initialiseQuery = (_: TableDataQuery): OptionalTableDataQuery => {
  return {};
};

export const validateAndPopulateSubject = (initialQuery: TableDataQuery): OptionalTableDataQuery => {

  if (!initialQuery.subjectId) return undefined;

  return {
    subjectId: initialQuery.subjectId,
  };
};

export const validateAndPopulateLocations = (initialQuery: TableDataQuery): OptionalTableDataQuery => {
  const initialLocations: Dictionary<string[]> = Object.entries(
    mapValuesWithKeys(
      LocationLevelKeysEnum,
      keyName => (initialQuery as Dictionary<string[]>)[keyName],
    ),
  ).reduce(
    (filtered, [key, value]) =>
      value ? { ...filtered, [key]: value } : filtered,
    {},
  );

  // check if any are actually set to validate if it's actually valid
  const allLocations = ([] as string[]).concat(
    ...Object.values(initialLocations),
  );
  if (allLocations.length === 0) return undefined;

  return initialLocations;


};

export const validateAndPopulateDateRange = (initialQuery: TableDataQuery): OptionalTableDataQuery => {
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

export const validateAndPopulateFiltersAndIndicators = (initialQuery: TableDataQuery): OptionalTableDataQuery => {
  if (
    initialQuery.filters.length === 0 ||
    initialQuery.indicators.length === 0
  ) {
    return undefined;
  }

  return {
    filters: initialQuery.filters,
    indicators: initialQuery.indicators,
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

export const initialiseFromInitialQuery = async (releaseId?: string, initialQuery?: TableDataQuery, initialTableHeaders?: TableHeadersFormValues) => {

  let newQuery: TableDataQuery | undefined;
  let newTable: FullTable | undefined;

  let newTableHeaders: TableHeadersFormValues = {
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  };

  let newSubjectId = '';
  let newLocations: Dictionary<LocationFilter[]> = {};
  let newDateRange: DateRangeState = {};
  let finalValidStepNumber = 1;
  let meta: PublicationSubjectMeta;

  if (initialQuery) {

    meta = await tableBuilderService.filterPublicationSubjectMeta(
      initialQuery,
    );

    let buildNewQuery: TableDataQuery = {
      subjectId: '',
      filters: [],
      indicators: [],
    };


    validationFunctions.every((fn, idx) => {
      const q = fn(initialQuery);

      if (q === undefined) return false;

      finalValidStepNumber = idx + 1;
      buildNewQuery = { ...buildNewQuery, ...q };

      return true;
    });


    if (finalValidStepNumber === 5) {
      newTable = await queryForTable(buildNewQuery, releaseId);

      newTableHeaders =
        initialTableHeaders ||
        getDefaultTableHeaderConfig(newTable.subjectMeta);
    }


    if (finalValidStepNumber > 1) newSubjectId = buildNewQuery.subjectId;

    if (finalValidStepNumber > 2) newLocations = mapLocations(
      getSelectedLocationsForQuery(buildNewQuery),
      meta.locations,
    );

    if (finalValidStepNumber > 3) newDateRange = { ...buildNewQuery.timePeriod };

    newQuery = buildNewQuery;
  } else {
    newQuery = undefined;
    meta = getDefaultSubjectMeta();
  }


  return {
    query: newQuery,
    validInitialQuery: newQuery,
    subjectId: newSubjectId,
    locations: newLocations,
    dateRange: newDateRange,
    subjectMeta: meta,
    createdTable: newTable,
    tableHeaders: newTableHeaders,
    initialStep: finalValidStepNumber,
  };

};
