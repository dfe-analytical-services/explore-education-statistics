import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  locationLevelKeys,
  LocationLevelKeys,
  PublicationSubjectMeta,
  TableDataQuery,
} from '@common/modules/full-table/services/tableBuilderService';
import { CategoryFilter } from '@common/modules/full-table/types/filters';
import { FullTableMeta } from '@common/modules/full-table/types/fullTable';
import { mapFullTable } from '@common/modules/full-table/utils/mapPermalinks';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import { TableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { Dictionary, PartialRecord } from '@common/types';
import sortBy from 'lodash/sortBy';

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

export type PartialTableDataQuery = Partial<TableDataQuery> | undefined;

const validateAndPopulateSubject = (
  initialQuery: TableDataQuery,
): PartialTableDataQuery => {
  if (!initialQuery.subjectId) return undefined;

  return {
    subjectId: initialQuery.subjectId,
  };
};

const validateAndPopulateLocations = (
  initialQuery: TableDataQuery,
): PartialTableDataQuery => {
  const locations = getLocationOptions(initialQuery);

  // Validate there are any locations at all
  return Object.values(locations).some(level => level?.length)
    ? locations
    : undefined;
};

const validateAndPopulateDateRange = (
  initialQuery: TableDataQuery,
): PartialTableDataQuery => {
  const { timePeriod } = initialQuery;

  if (
    timePeriod?.endCode === undefined ||
    timePeriod?.endYear === undefined ||
    timePeriod?.startCode === undefined ||
    timePeriod?.startYear === undefined
  ) {
    return undefined;
  }

  return {
    timePeriod: {
      ...timePeriod,
    },
  };
};

const validateAndPopulateFiltersAndIndicators = (
  initialQuery: TableDataQuery,
): PartialTableDataQuery => {
  return {
    filters: [...(initialQuery.filters || [])],
    indicators: [...(initialQuery.indicators || [])],
  };
};

export const getDefaultSubjectMeta = (): PublicationSubjectMeta => ({
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
  locations: {},
  indicators: {},
  filters: {},
});

export const executeTableQuery = async (
  query: TableDataQuery,
  releaseId?: string,
) => {
  const rawTableData = await tableBuilderService.getTableData(query, releaseId);

  const table = mapFullTable(rawTableData);
  const tableHeaders = getDefaultTableHeaderConfig(table.subjectMeta);

  return {
    table,
    tableHeaders,
  };
};

export const initialiseFromQuery = async (
  initialQuery?: TableDataQuery,
  releaseId?: string,
): Promise<TableToolState> => {
  const state: TableToolState = {
    initialStep: 1,
    query: {
      subjectId: '',
      indicators: [],
      filters: [],
    },
    subjectMeta: getDefaultSubjectMeta(),
  };

  if (initialQuery) {
    let newQuery: TableDataQuery = {
      subjectId: '',
      filters: [],
      indicators: [],
    };

    [
      validateAndPopulateSubject,
      validateAndPopulateLocations,
      validateAndPopulateDateRange,
      validateAndPopulateFiltersAndIndicators,
    ].every(fn => {
      const result = fn(initialQuery);

      if (result === undefined) {
        return false;
      }

      state.initialStep += 1;
      newQuery = { ...newQuery, ...result };

      return true;
    });

    state.query = newQuery;

    if (state.initialStep === 5) {
      state.response = await executeTableQuery(newQuery, releaseId);
    }

    state.subjectMeta = await tableBuilderService.filterPublicationSubjectMeta({
      subjectId: initialQuery.subjectId,
    });
  }

  return state;
};
