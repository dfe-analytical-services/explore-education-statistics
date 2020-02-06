import tableBuilderService, {
  locationLevelKeys,
  LocationLevelKeys,
  TableDataQuery,
} from '@common/modules/table-tool/services/tableBuilderService';
import { TableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import {
  executeTableQuery,
  getDefaultSubjectMeta,
} from '@common/modules/table-tool/components/utils/tableToolHelpers';

type PartialTableDataQuery = Partial<TableDataQuery> | undefined;

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
  const locations = Object.fromEntries(
    Object.entries(initialQuery).filter(([key]) =>
      locationLevelKeys.includes(key as LocationLevelKeys),
    ),
  ) as Record<LocationLevelKeys, string[]>;

  // Validate there are any locations at all
  return Object.values(locations).some(level => level.length)
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

const initialiseFromQuery = async (
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

export default initialiseFromQuery;
