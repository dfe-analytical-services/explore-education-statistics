import {
  FilterOption,
  LocationOption,
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import combineMeasuresWithDuplicateLocationCodes from '@common/services/util/combineMeasuresWithDuplicateLocationCodes';
import groupBy from 'lodash/groupBy';
import mapValues from 'lodash/mapValues';
import uniq from 'lodash/uniq';

/**
 * Given a set of {@param tableData}, this function will merge any Locations
 * that have duplicate codes and geographic levels into combined Locations with
 * a label derived from all of the distinct Location names.
 *
 * E.g. if 2 Locations, Provider 1 and Provider 2 share the same geographic
 * level and code, this will merge those Locations into a single Location with
 * the label "Provider 1 / Provider 2", combining in alphabetical order.
 *
 * This will merge not only the Locations in the TableDataSubjectMeta but also
 * the TableDataResults for those duplicate Locations, merging duplicate rows
 * into single rows with combined values derived form each duplicate Location.
 *
 * @deprecated Remove in EES-2783
 */
export function deduplicateTableDataLocations(
  tableData: TableDataResponse,
): TableDataResponse {
  if (!tableData.subjectMeta) {
    return tableData;
  }

  const locationsGroupedByLevelAndCode = groupBy(
    tableData.subjectMeta.locations,
    location => `${location.level}_${location.value}`,
  );

  const mergedLocations = Object.values(
    locationsGroupedByLevelAndCode,
  ).flatMap(locations => mergeLocationLabels(locations));

  const deduplicatedLocations = mergedLocations.filter(
    location => !tableData.subjectMeta.locations.includes(location),
  );

  const mergedResults = combineMeasuresWithDuplicateLocationCodes(
    tableData.results,
    deduplicatedLocations,
  );

  return {
    ...tableData,
    subjectMeta: {
      ...tableData.subjectMeta,
      locations: mergedLocations,
    },
    results: mergedResults,
  };
}

/**
 * Given {@param subjectMeta}, this function will merge any Locations that
 * have duplicate codes and geographic levels into combined Locations with a
 * label derived from all of the distinct Location names.
 *
 * E.g. if 2 Locations, Provider 1 and Provider 2 share the same geographic
 * level and code, this will merge those Locations into a single
 * Location with the label "Provider 1 / Provider 2", combining in
 * alphabetical order.
 *
 * @deprecated Remove in EES-2783
 */
export function deduplicateSubjectMetaLocations(
  subjectMeta: SubjectMeta,
): SubjectMeta {
  const mergedLocations = mapValues(subjectMeta.locations, level => {
    const optionsGroupedByCode = groupBy(level.options, option => option.value);

    return {
      ...level,
      options: Object.values(optionsGroupedByCode).flatMap(locations =>
        mergeLocationLabels(locations),
      ),
    };
  });

  return {
    ...subjectMeta,
    locations: mergedLocations,
  };
}

/**
 * Given a set of {@param locations}, this function will return
 * a single location record with a combined label.
 */
function mergeLocationLabels<T extends LocationOption | FilterOption>(
  locations: T[],
): T {
  // If there's only one Location provided,
  // there's no need to do any merging.
  if (locations.length === 1) {
    return locations[0];
  }

  // Otherwise, produce a merged Location based upon the
  // variations provided.
  const distinctLabels = uniq(locations.map(l => l.label));

  return {
    ...locations[0],
    label: distinctLabels.sort().join(' / '),
  };
}
