import { dataApi } from '@common/services/api';
import { FileInfo } from '@common/services/types/file';
import { Dictionary } from '@common/types';
import { Feature, Geometry } from 'geojson';
import {
  deduplicateSubjectMetaLocations,
  deduplicateTableDataLocations,
} from './util/tableBuilderServiceUtils';

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
  level?: string;
  options?: LocationLeafOption[];
  geoJson?: GeoJsonFeature[];
}

export interface LocationLeafOption {
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
    return deduplicateSubjectMetaLocations(
      await dataApi.get(`/meta/subject/${subjectId}`),
    );
  },
  async filterSubjectMeta(query: {
    subjectId: string;
    timePeriod?: TimePeriodQuery;
    geographicLevel?: string;
    locations?: Dictionary<string[]>;
  }): Promise<SubjectMeta> {
    return deduplicateSubjectMetaLocations(
      await dataApi.post('/meta/subject', query),
    );
  },
  async getTableData({
    releaseId,
    ...query
  }: ReleaseTableDataQuery): Promise<TableDataResponse> {
    return deduplicateTableDataLocations(
      releaseId
        ? await dataApi.post(`/tablebuilder/release/${releaseId}`, query)
        : await dataApi.post('/tablebuilder', query),
    );
  },
  async getDataBlockTableData(
    releaseId: string,
    dataBlockId: string,
  ): Promise<TableDataResponse> {
    return deduplicateTableDataLocations(
      await dataApi.get(
        `/tablebuilder/release/${releaseId}/data-block/${dataBlockId}`,
      ),
    );
  },
};

export default tableBuilderService;
