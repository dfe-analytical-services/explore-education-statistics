import { dataApi } from '@common/services/api';
import { FileInfo } from '@common/services/types/file';
import { ConfiguredTable } from '@common/services/types/table';
import { Dictionary } from '@common/types';
import { AxiosRequestConfig } from 'axios';
import { Feature, Geometry } from 'geojson';
import { ReleaseType } from '@common/services/types/releaseType';
import { FileFormat } from '@common/modules/table-tool/components/DownloadTable';

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

interface FilterHierarchyTier {
  level: number;
  filterId: string;
  childFilterId: string;
  hierarchy: Dictionary<string[]>;
}

export type SubjectMetaFilterHierarchy = FilterHierarchyTier[];

export interface SubjectMetaFilter {
  legend: string;
  hint?: string;
  id: string;
  options: GroupedFilterOptions;
  order: number;
  autoSelectFilterItemId?: string;
  name: string;
}

export interface SubjectMeta {
  filterHierarchies?: SubjectMetaFilterHierarchy[];
  filters: Dictionary<SubjectMetaFilter>;
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

export interface ReleaseTableDataQuery extends TableDataQuery {
  releaseVersionId?: string;
}

export interface TableDataQuery extends FullTableQuery {
  publicationId?: string;
}

export interface FullTableQuery {
  subjectId: string;
  locationIds: string[];
  timePeriod?: TimePeriodQuery;
  filters: string[];
  filterHierarchiesOptions?: Dictionary<string[][]>;
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
    autoSelectFilterItemId?: string;
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
  latestReleaseSlug: string;
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
  listReleaseSubjects(releaseVersionId: string): Promise<Subject[]> {
    return dataApi.get(`/releases/${releaseVersionId}/subjects`);
  },
  listReleaseFeaturedTables(
    releaseVersionId: string,
  ): Promise<FeaturedTable[]> {
    return dataApi.get(`/releases/${releaseVersionId}/featured-tables`);
  },
  async getSubjectMeta(
    subjectId: string,
    releaseVersionId?: string,
  ): Promise<SubjectMeta> {
    return releaseVersionId
      ? dataApi.get(`/release/${releaseVersionId}/meta/subject/${subjectId}`)
      : dataApi.get(`/meta/subject/${subjectId}`);
  },
  async filterSubjectMeta(
    query: LocationsOrTimePeriodsQuery,
    releaseVersionId?: string,
  ): Promise<SubjectMeta> {
    return releaseVersionId
      ? dataApi.post(`/release/${releaseVersionId}/meta/subject`, query)
      : dataApi.post('/meta/subject', query);
  },
  async getTableData(
    query: FullTableQuery,
    releaseVersionId?: string,
  ): Promise<TableDataResponse> {
    return releaseVersionId
      ? dataApi.post(`/tablebuilder/release/${releaseVersionId}`, query)
      : dataApi.post('/tablebuilder', query);
  },
  async getTableCsv({
    releaseVersionId,
    ...query
  }: ReleaseTableDataQuery): Promise<Blob> {
    const config: AxiosRequestConfig = {
      headers: {
        Accept: 'text/csv',
      },
      responseType: 'blob',
    };

    return releaseVersionId
      ? dataApi.post(`/tablebuilder/release/${releaseVersionId}`, query, config)
      : dataApi.post('/tablebuilder', query, config);
  },
  async getDataBlockTableData(
    releaseVersionId: string,
    dataBlockParentId: string,
  ): Promise<TableDataResponse> {
    return dataApi.get(
      `/tablebuilder/release/${releaseVersionId}/data-block/${dataBlockParentId}`,
    );
  },
  getFastTrackTableAndReleaseMeta(
    dataBlockParentId: string,
  ): Promise<FastTrackTableAndReleaseMeta> {
    return dataApi.get(`/tablebuilder/fast-track/${dataBlockParentId}`);
  },
  getDataBlockGeoJson(
    releaseVersionId: string,
    dataBlockParentId: string,
    boundaryLevelId: number,
  ): Promise<Dictionary<LocationGeoJsonOption[]>> {
    return dataApi.get(
      `/tablebuilder/release/${releaseVersionId}/data-block/${dataBlockParentId}/geojson`,
      { params: { boundaryLevelId } },
    );
  },
  recordDownload(payload: {
    dataSetName: string;
    publicationName: string;
    releaseVersionId: string;
    releasePeriodAndLabel: string;
    subjectId: string;
    query: ReleaseTableDataQuery;
    downloadFormat: FileFormat;
  }) {
    dataApi.post('tablebuilder/analytics', payload);
  },
};

export default tableBuilderService;
