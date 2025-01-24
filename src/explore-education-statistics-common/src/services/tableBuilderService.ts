import { dataApi } from '@common/services/api';
import { FileInfo } from '@common/services/types/file';
import { ConfiguredTable } from '@common/services/types/table';
import { Dictionary } from '@common/types';
import { AxiosRequestConfig } from 'axios';
import { Feature, Geometry } from 'geojson';
import { ReleaseType } from '@common/services/types/releaseType';

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

export interface FilterOptionGroup {
  id: string;
  label: string;
  options: FilterOption[];
  order: number;
}

export interface GroupedFilterOptions {
  [groupKey: string]: FilterOptionGroup;
}

export interface BoundaryLevel {
  id: number;
  label: string;
}

export interface GeoJsonFeatureProperties {
  // these are what is required - Note the casing here.
  Code: string;
  Name: string;
  LONG: number;
  LAT: number;

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
  geoJson?: GeoJsonFeature;
}

export interface LocationGeoJsonOption extends LocationOption {
  geoJson: GeoJsonFeature;
}

export interface LocationLeafOption {
  id?: string;
  label: string;
  value: string;
  geoJson?: GeoJsonFeature;
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
  filters: string[];
  indicators: string[];
  lastUpdated: string;
}

export interface FeaturedTable {
  id: string;
  name: string;
  description?: string;
  subjectId: string;
  dataBlockId: string;
  dataBlockParentId: string;
  order: number;
}

export interface SubjectMeta {
  filters: Dictionary<{
    legend: string;
    hint?: string;
    id: string;
    options: GroupedFilterOptions;
    order: number;
    totalValue?: string;
    name: string;
  }>;
  indicators: Dictionary<{
    id: string;
    label: string;
    options: IndicatorOption[];
    order: number;
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

export interface TableDataQuery extends FullTableQuery {
  publicationId?: string;
  boundaryLevel?: number;
}

export interface ReleaseTableDataQuery extends TableDataQuery {
  releaseId?: string;
}

export interface FullTableQuery {
  subjectId: string;
  locationIds: string[];
  timePeriod?: TimePeriodQuery;
  filters: string[];
  indicators: string[];
}

export interface LocationsOrTimePeriodsQuery {
  subjectId: string;
  locationIds: string[];
  timePeriod?: TimePeriodQuery;
}

export interface TableDataSubjectMeta {
  publicationName: string;
  subjectName: string;
  locations: Dictionary<LocationOption[] | LocationGeoJsonOption[]>;
  boundaryLevels: BoundaryLevel[];
  timePeriodRange: TimePeriodOption[];
  filters: Dictionary<{
    name: string;
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    order: number;
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

export interface FastTrackTable extends ConfiguredTable {
  query: TableDataQuery & {
    publicationId: string;
  };
}

export interface FastTrackTableAndReleaseMeta extends FastTrackTable {
  releaseId: string;
  releaseSlug: string;
  releaseType: ReleaseType;
  latestData: boolean;
  latestReleaseTitle: string;
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
  async getSubjectMeta(
    subjectId: string,
    releaseId?: string,
  ): Promise<SubjectMeta> {
    return releaseId
      ? dataApi.get(`/release/${releaseId}/meta/subject/${subjectId}`)
      : dataApi.get(`/meta/subject/${subjectId}`);
  },
  async filterSubjectMeta(
    query: LocationsOrTimePeriodsQuery,
    releaseId?: string,
  ): Promise<SubjectMeta> {
    return releaseId
      ? dataApi.post(`/release/${releaseId}/meta/subject`, query)
      : dataApi.post('/meta/subject', query);
  },
  async getTableData(
    query: FullTableQuery,
    releaseId?: string,
    boundaryLevelId?: number,
  ): Promise<TableDataResponse> {
    if (releaseId && boundaryLevelId) {
      return dataApi.post(
        `/tablebuilder/release/${releaseId}?boundaryLevelId=${boundaryLevelId}`,
        query,
      );
    }

    return releaseId && !boundaryLevelId
      ? dataApi.post(`/tablebuilder/release/${releaseId}`, query)
      : dataApi.post('/tablebuilder', query);
  },
  async getTableCsv({
    releaseId,
    ...query
  }: ReleaseTableDataQuery): Promise<Blob> {
    const config: AxiosRequestConfig = {
      headers: {
        Accept: 'text/csv',
      },
      responseType: 'blob',
    };

    return releaseId
      ? dataApi.post(`/tablebuilder/release/${releaseId}`, query, config)
      : dataApi.post('/tablebuilder', query, config);
  },
  async getDataBlockTableData(
    releaseId: string,
    dataBlockParentId: string,
  ): Promise<TableDataResponse> {
    return dataApi.get(
      `/tablebuilder/release/${releaseId}/data-block/${dataBlockParentId}`,
    );
  },
  getFastTrackTableAndReleaseMeta(
    dataBlockParentId: string,
  ): Promise<FastTrackTableAndReleaseMeta> {
    return dataApi.get(`/tablebuilder/fast-track/${dataBlockParentId}`);
  },
  getDataBlockGeoJson(
    releaseId: string,
    dataBlockParentId: string,
    boundaryLevelId: number,
  ): Promise<Dictionary<LocationGeoJsonOption[]>> {
    return dataApi.get(
      `/tablebuilder/release/${releaseId}/data-block/${dataBlockParentId}/geojson`,
      { params: { boundaryLevelId } },
    );
  },
};

export default tableBuilderService;
