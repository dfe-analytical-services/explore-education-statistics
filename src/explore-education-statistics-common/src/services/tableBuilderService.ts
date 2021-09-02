import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import { Feature, Geometry } from 'geojson';
import { groupBy, mapValues, uniq } from 'lodash';
import combineMeasuresWithDuplicateLocationCodes from '@common/services/util/combineMeasuresWithDuplicateLocationCodes';
import { produce } from 'immer';

export interface FilterOption {
  label: string;
  value: string;
}

export interface IndicatorOption extends FilterOption {
  unit: string;
  name: string;
  decimalPlaces?: number;
}

export interface TimePeriodOption {
  code: string;
  label: string;
  year: number;
}

export interface GroupedFilterOptions {
  [groupKey: string]: {
    label: string;
    options: FilterOption[];
  };
}

export interface BoundaryLevel {
  id: number;
  label: string;
}

export interface GeoJsonFeatureProperties {
  // these are what is required
  code: string;
  name: string;
  long: number;
  lat: number;

  // the following are just named here for easier finding in code completion and not required
  objectid?: number;
  ctry17cd?: string | null;
  ctry17nm?: string | null;
  lad17cd?: string | null;
  lad17nm?: string | null;

  // allow anything as this is an extension of the GeoJsonProperties object at its heart
  [name: string]: unknown;
}

export type GeoJsonFeature = Feature<Geometry, GeoJsonFeatureProperties>;

export interface LocationOption {
  label: string;
  value: string;
  level: string;
  geoJson?: GeoJsonFeature[];
}

export interface Subject {
  id: string;
  name: string;
  content: string;
  timePeriods: {
    from?: string;
    to?: string;
  };
  geographicLevels: string[];
}

export interface FeaturedTable {
  id: string;
  name: string;
  description?: string;
}
export interface SubjectMeta {
  filters: Dictionary<{
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
    name: string;
  }>;
  indicators: Dictionary<{
    label: string;
    options: IndicatorOption[];
  }>;
  locations: Dictionary<{
    legend: string;
    hint?: string;
    options: FilterOption[];
  }>;
  timePeriod: {
    hint?: string;
    legend: string;
    options: TimePeriodOption[];
  };
}

export interface TimePeriodQuery {
  startYear: number;
  startCode: string;
  endYear: number;
  endCode: string;
}

export interface TableDataQuery {
  publicationId?: string;
  subjectId: string;
  filters: string[];
  indicators: string[];
  timePeriod?: TimePeriodQuery;
  geographicLevel?: string;
  locations: Dictionary<string[]>;
  includeGeoJson?: boolean;
  boundaryLevel?: number;
}

export interface ReleaseTableDataQuery extends TableDataQuery {
  releaseId?: string;
}

export interface TableDataSubjectMeta {
  publicationName: string;
  subjectName: string;
  locations: LocationOption[];
  boundaryLevels: BoundaryLevel[];
  timePeriodRange: TimePeriodOption[];
  filters: Dictionary<{
    name: string;
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
  }>;
  geoJsonAvailable: boolean;
  indicators: IndicatorOption[];
  footnotes: {
    id: string;
    label: string;
  }[];
}

export interface TableDataResult {
  filters: string[];
  geographicLevel: string;
  location: Dictionary<{
    code: string;
    name: string;
  }>;
  measures: Dictionary<string>;
  timePeriod: string;
}

export interface TableDataResponse {
  results: TableDataResult[];
  subjectMeta: TableDataSubjectMeta;
}

export interface SelectedPublication {
  id: string;
  title: string;
  slug: string;
  selectedRelease: {
    id: string;
    slug: string;
    latestData: boolean;
    title: string;
  };
  latestRelease: {
    title: string;
  };
}

/**
 * Given a set of ${@param locations}, this function will return a single location record with a combined name.
 */
function generateMergedLocation<T extends LocationOption | FilterOption>(
  locations: T[],
): T {
  // If there's only one Location provided, there's no need to do any merging.
  if (locations.length === 1) {
    return locations[0];
  }
  // Otherwise, produce a merged Location based upon the variations provided.
  const distinctLocationNames = uniq(locations.map(l => l.label));
  const mergedNames = distinctLocationNames.sort().join(' / ');
  return produce(locations[0], draft => {
    draft.label = mergedNames;
  });
}

/**
 * Given a set of ${@param tableData}, this function will merge any Locations that have duplicate codes and geographic
 * levels into combined Locations with a label derived from all of the distinct Location names.  E.g. if 2 Locations,
 * Provider 1 and Provider 2 share the same geographic level and code, this will merge those Locations into a single
 * Location with the label "Provider 1 / Provider 2", combining in alphabetical order.
 *
 * This will merge not only the Locations in the TableDataSubjectMeta but also the TableDataResults for those duplicate
 * Locations, merging duplicate rows into single rows with combined values derived form each duplicate Location.
 */
function mergeDuplicateLocationsInTableDataResponse(
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
  ).flatMap(locations => [generateMergedLocation(locations)]);

  const deduplicatedLocations = mergedLocations.filter(
    location => !tableData.subjectMeta.locations.includes(location),
  );

  const mergedResults = combineMeasuresWithDuplicateLocationCodes(
    tableData.results,
    deduplicatedLocations,
  );

  return produce(tableData, draft => {
    draft.subjectMeta.locations = mergedLocations;
    draft.results = mergedResults;
  });
}

/**
 * Given ${@param subjectMeta}, this function will merge any Locations that have duplicate codes and geographic
 * levels into combined Locations with a label derived from all of the distinct Location names.  E.g. if 2 Locations,
 * Provider 1 and Provider 2 share the same geographic level and code, this will merge those Locations into a single
 * Location with the label "Provider 1 / Provider 2", combining in alphabetical order.
 */
function mergeDuplicateLocationsInSubjectMeta(
  subjectMeta: SubjectMeta,
): SubjectMeta {
  const mergedLocations = mapValues(subjectMeta.locations, level => {
    const optionsGroupedByCode = groupBy(level.options, option => option.value);
    const mergedOptions = Object.values(
      optionsGroupedByCode,
    ).flatMap(locations => [generateMergedLocation(locations)]);

    return produce(level, draft => {
      draft.options = mergedOptions;
    });
  });

  return produce(subjectMeta, draft => {
    draft.locations = mergedLocations;
  });
}

const tableBuilderService = {
  listLatestReleaseSubjects(publicationId: string): Promise<Subject[]> {
    return dataApi.get(`/publications/${publicationId}/subjects`);
  },
  listLatestReleaseFeaturedTables(
    publicationId: string,
  ): Promise<FeaturedTable[]> {
    return dataApi.get(`/publications/${publicationId}/featured-tables`);
  },
  listReleaseSubjects(releaseId: string): Promise<Subject[]> {
    return dataApi.get(`/releases/${releaseId}/subjects`);
  },
  listReleaseFeaturedTables(releaseId: string): Promise<FeaturedTable[]> {
    return dataApi.get(`/releases/${releaseId}/featured-tables`);
  },
  async getSubjectMeta(subjectId: string): Promise<SubjectMeta> {
    return mergeDuplicateLocationsInSubjectMeta(
      await dataApi.get(`/meta/subject/${subjectId}`),
    );
  },
  async filterSubjectMeta(query: {
    subjectId: string;
    timePeriod?: TimePeriodQuery;
    geographicLevel?: string;
    locations?: Dictionary<string[]>;
  }): Promise<SubjectMeta> {
    return mergeDuplicateLocationsInSubjectMeta(
      await dataApi.post('/meta/subject', query),
    );
  },
  async getTableData({
    releaseId,
    ...query
  }: ReleaseTableDataQuery): Promise<TableDataResponse> {
    if (releaseId) {
      return mergeDuplicateLocationsInTableDataResponse(
        await dataApi.post(`/tablebuilder/release/${releaseId}`, query),
      );
    }
    return mergeDuplicateLocationsInTableDataResponse(
      await dataApi.post(`/tablebuilder`, query),
    );
  },
  async getDataBlockTableData(
    releaseId: string,
    dataBlockId: string,
  ): Promise<TableDataResponse> {
    return mergeDuplicateLocationsInTableDataResponse(
      await dataApi.get(
        `/tablebuilder/release/${releaseId}/data-block/${dataBlockId}`,
      ),
    );
  },
};

export default tableBuilderService;
