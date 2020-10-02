import tableBuilderService, {
  ReleaseTableDataQuery,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import pick from 'lodash/pick';

function hasAllLocations(
  query: ReleaseTableDataQuery,
  subjectMeta: SubjectMeta,
) {
  const locationEntries = Object.entries(query.locations);

  if (locationEntries.length === 0) {
    return false;
  }

  return locationEntries.every(([locationLevel, locations]) => {
    if (locations.length === 0) {
      return false;
    }

    return locations.every(locationCode =>
      subjectMeta.locations[locationLevel]?.options?.find(
        metaLocation => metaLocation.value === locationCode,
      ),
    );
  });
}

function hasAllTimePeriods(
  query: ReleaseTableDataQuery,
  subjectMeta: SubjectMeta,
) {
  if (!query.timePeriod) {
    return false;
  }

  return (
    subjectMeta.timePeriod.options.some(
      timePeriod =>
        timePeriod.year === query.timePeriod?.startYear &&
        timePeriod.code === query.timePeriod?.startCode,
    ) &&
    subjectMeta.timePeriod.options.some(
      timePeriod =>
        timePeriod.year === query.timePeriod?.endYear &&
        timePeriod.code === query.timePeriod?.endCode,
    )
  );
}

function hasAllFilters(query: ReleaseTableDataQuery, subjectMeta: SubjectMeta) {
  const metaFilters = Object.values(subjectMeta.filters)
    .flatMap(filter => Object.values(filter.options))
    .flatMap(filterGroup => filterGroup.options)
    .flatMap(filterItem => filterItem.value);

  return query.filters.every(filter => metaFilters.includes(filter));
}

function hasAllIndicators(
  query: ReleaseTableDataQuery,
  subjectMeta: SubjectMeta,
) {
  const metaIndicators = Object.values(subjectMeta.indicators)
    .flatMap(indicatorGroup => indicatorGroup.options)
    .map(indicator => indicator.value);

  return query.indicators.every(indicator =>
    metaIndicators.includes(indicator),
  );
}

export interface InitialStepSubjectMeta {
  initialStep: number;
  subjectMeta?: SubjectMeta;
}

/**
 * Get the initial state for table tool given a {@param query}.
 * This allows us to partially setup table tool, even when
 * some of the query options are invalid. The user can then
 * go back through table tool and pick valid options.
 */
export default async function getInitialStepSubjectMeta(
  query: ReleaseTableDataQuery,
): Promise<InitialStepSubjectMeta> {
  if (!query.releaseId) {
    return {
      initialStep: 1,
    };
  }

  if (!query.subjectId) {
    return {
      initialStep: 2,
    };
  }

  const subjectMeta = await tableBuilderService.getSubjectMeta(query.subjectId);

  if (!hasAllLocations(query, subjectMeta)) {
    return {
      initialStep: 3,
      subjectMeta,
    };
  }

  let filteredSubjectMeta = await tableBuilderService.filterSubjectMeta(
    pick(query, ['subjectId', 'locations']),
  );

  if (!hasAllTimePeriods(query, filteredSubjectMeta)) {
    return {
      initialStep: 4,
      subjectMeta,
    };
  }

  filteredSubjectMeta = await tableBuilderService.filterSubjectMeta(
    pick(query, ['subjectId', 'locations', 'timePeriod']),
  );

  if (
    !hasAllIndicators(query, filteredSubjectMeta) ||
    !hasAllFilters(query, filteredSubjectMeta)
  ) {
    return {
      initialStep: 5,
      subjectMeta,
    };
  }

  return {
    initialStep: 6,
    subjectMeta,
  };
}
