import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import { Feature, Geometry } from 'geojson';
import { groupBy, mapValues, uniq } from 'lodash';
import combineMeasuresWithDuplicateLocationCodes from '@common/modules/table-tool/components/utils/combineMeasuresWithDuplicateLocationCodes';

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

export interface TableHighlight {
  id: string;
  name: string;
  description?: string;
}

export interface SubjectsAndHighlights {
  subjects: Subject[];
  highlights: TableHighlight[];
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

function mergeDuplicateLocationsInTableDataResponse(
  tableData: TableDataResponse,
): TableDataResponse {
  const locationsGroupedByLevelAndCode = groupBy(
    tableData.subjectMeta.locations,
    location => `${location.level}_${location.value}`,
  );

  const mergedLocations: LocationOption[] = Object.values(
    locationsGroupedByLevelAndCode,
  ).flatMap(locations => {
    const distinctLocationNames = uniq(locations.map(l => l.label));
    const mergedNames = distinctLocationNames.sort().join(' / ');
    return [
      {
        ...locations[0],
        label: mergedNames,
      },
    ];
  });

  return {
    ...tableData,
    subjectMeta: {
      ...tableData.subjectMeta,
      locations: mergedLocations,
    },
    results: combineMeasuresWithDuplicateLocationCodes(
      tableData.results,
      mergedLocations,
    ),
  };
}

function mergeDuplicateLocationsInSubjectMeta(
  subjectMeta: SubjectMeta,
): SubjectMeta {
  const mergedLocations = mapValues(subjectMeta.locations, level => {
    const optionsGroupedByCode = groupBy(level.options, option => option.value);
    const mergedOptions = Object.values(optionsGroupedByCode).flatMap(
      locations => {
        const distinctLocationNames = uniq(locations.map(l => l.label));
        const mergedNames = distinctLocationNames.sort().join(' / ');
        return [
          {
            ...locations[0],
            label: mergedNames,
          },
        ];
      },
    );
    return {
      ...level,
      options: mergedOptions,
    };
  });

  return {
    ...subjectMeta,
    locations: mergedLocations,
  };
}

const tableBuilderService = {
  getPublicationSubjectsAndHighlights(
    publicationId: string,
  ): Promise<SubjectsAndHighlights> {
    return dataApi.get(`/publications/${publicationId}`);
  },
  getReleaseSubjectsAndHighlights(
    releaseId: string,
  ): Promise<SubjectsAndHighlights> {
    return dataApi.get(`/releases/${releaseId}`);
  },
  async getSubjectMeta(subjectId: string): Promise<SubjectMeta> {
    const response: SubjectMeta = await dataApi.get(
      `/meta/subject/${subjectId}`,
    );
    return mergeDuplicateLocationsInSubjectMeta(response);
  },
  async filterSubjectMeta(query: {
    subjectId: string;
    timePeriod?: TimePeriodQuery;
    geographicLevel?: string;
    locations?: Dictionary<string[]>;
  }): Promise<SubjectMeta> {
    const response: SubjectMeta = await dataApi.post('/meta/subject', query);
    return mergeDuplicateLocationsInSubjectMeta(response);
  },
  async getTableData({
    releaseId,
    combineDuplicateLocations = true,
    ...query
  }: ReleaseTableDataQuery & {
    combineDuplicateLocations?: boolean;
  }): Promise<TableDataResponse> {
    if (releaseId) {
      const response: TableDataResponse = await dataApi.post(
        `/tablebuilder/release/${releaseId}`,
        query,
      );
      return combineDuplicateLocations
        ? mergeDuplicateLocationsInTableDataResponse(response)
        : response;
    }
    const response: TableDataResponse = await dataApi.post(
      `/tablebuilder`,
      query,
    );
    return combineDuplicateLocations
      ? mergeDuplicateLocationsInTableDataResponse(response)
      : response;
  },
  async getDataBlockTableData(
    releaseId: string,
    dataBlockId: string,
  ): Promise<TableDataResponse> {
    const response: TableDataResponse = await dataApi.get(
      `/tablebuilder/release/${releaseId}/data-block/${dataBlockId}`,
    );
    return mergeDuplicateLocationsInTableDataResponse(response);
  },
};

export default tableBuilderService;
