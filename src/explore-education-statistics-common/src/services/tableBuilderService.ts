import { dataApi } from '@common/services/api';
import { FileInfo } from '@common/services/types/file';
import { Dictionary } from '@common/types';
import { Feature, Geometry } from 'geojson';

export interface FilterOption {
  label: string;
  value: string;
  id?: string; // EES-1243 - not optional when backend done?
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

export interface FilterOptionGroup {
  id?: string; // EES-1243 - not optional when backend done?
  label: string;
  options: FilterOption[];
  order?: number; // EES-1243 - not optional when backend done?
}

export interface GroupedFilterOptions {
  [groupKey: string]: FilterOptionGroup;
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
  id?: string;
  label: string;
  value: string;
  level?: string;
  options?: LocationLeafOption[];
  geoJson?: GeoJsonFeature[];
}

export interface LocationLeafOption {
  id?: string;
  label: string;
  value: string;
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
  file: FileInfo;
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
    id?: string; // EES-1243 - not optional when backend done?
    options: GroupedFilterOptions;
    order?: number; // EES-1243 - not optional when backend done?
    totalValue?: string;
    name: string;
  }>;
  indicators: Dictionary<{
    id?: string; // EES-1243 - not optional when backend done?
    label: string;
    options: IndicatorOption[];
    order?: number; // EES-1243 - not optional when backend done?
  }>;
  locations: Dictionary<{
    legend: string;
    options: LocationOption[];
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
  locationIds: string[];
  includeGeoJson?: boolean;
  boundaryLevel?: number;
}

export interface ReleaseTableDataQuery extends TableDataQuery {
  releaseId?: string;
}

export interface TableDataSubjectMeta {
  publicationName: string;
  subjectName: string;
  locations: Dictionary<LocationOption[]>;
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
  locationId?: string;
  location?: Dictionary<{
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
    return dataApi.get(`/meta/subject/${subjectId}`);
  },
  async filterSubjectMeta(query: {
    subjectId: string;
    timePeriod?: TimePeriodQuery;
    locationIds?: string[];
  }): Promise<SubjectMeta> {
    return dataApi.post('/meta/subject', query);
  },
  async getTableData({
    releaseId,
    ...query
  }: ReleaseTableDataQuery): Promise<TableDataResponse> {
    return releaseId
      ? dataApi.post(`/tablebuilder/release/${releaseId}`, query)
      : dataApi.post('/tablebuilder', query);
  },
  async getDataBlockTableData(
    releaseId: string,
    dataBlockId: string,
  ): Promise<TableDataResponse> {
    return dataApi.get(
      `/tablebuilder/release/${releaseId}/data-block/${dataBlockId}`,
    );
  },
};

export default tableBuilderService;
