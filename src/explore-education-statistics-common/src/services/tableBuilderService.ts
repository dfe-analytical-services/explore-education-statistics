import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import { Feature, Geometry } from 'geojson';

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
  getSubjectMeta(subjectId: string): Promise<SubjectMeta> {
    return dataApi.get(`/meta/subject/${subjectId}`);
  },
  filterSubjectMeta(query: {
    subjectId: string;
    timePeriod?: TimePeriodQuery;
    geographicLevel?: string;
    locations?: Dictionary<string[]>;
  }): Promise<SubjectMeta> {
    return dataApi.post('/meta/subject', query);
  },
  getTableData({
    releaseId,
    ...query
  }: ReleaseTableDataQuery): Promise<TableDataResponse> {
    if (releaseId) {
      return dataApi.post(`/tablebuilder/release/${releaseId}`, query);
    }

    return dataApi.post(`/tablebuilder`, query);
  },
  getDataBlockTableData(
    releaseId: string,
    dataBlockId: string,
  ): Promise<TableDataResponse> {
    return dataApi.get(
      `/tablebuilder/release/${releaseId}/data-block/${dataBlockId}`,
    );
  },
};

export default tableBuilderService;
