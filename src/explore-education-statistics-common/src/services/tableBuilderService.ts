import { dataApi } from '@common/services/api';
import { Dictionary, PartialRecord } from '@common/types';
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

export interface ThemeMeta {
  id: string;
  title: string;
  slug: string;
  topics: {
    id: string;
    title: string;
    slug: string;
    publications: {
      id: string;
      title: string;
      slug: string;
    }[];
  }[];
}

export interface PublicationSubject {
  id: string;
  label: string;
}

export interface PublicationMeta {
  publicationId: string;
  subjects: PublicationSubject[];
}

export interface ReleaseMeta {
  releaseId: string;
  subjects: PublicationSubject[];
}

export interface PublicationSubjectMeta {
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

export const locationLevelKeys = [
  'country',
  'institution',
  'localAuthority',
  'localAuthorityDistrict',
  'localEnterprisePartnership',
  'mayoralCombinedAuthority',
  'multiAcademyTrust',
  'opportunityArea',
  'parliamentaryConstituency',
  'region',
  'rscRegion',
  'sponsor',
  'ward',
] as const;

export type LocationLevelKeys = typeof locationLevelKeys[number];

export interface TimePeriodQuery {
  startYear: number;
  startCode: string;
  endYear: number;
  endCode: string;
}

export type TableDataQuery = {
  publicationId?: string;
  subjectId: string;
  filters: string[];
  indicators: string[];
  timePeriod?: TimePeriodQuery;
  geographicLevel?: string;
  includeGeoJson?: boolean;
  boundaryLevel?: number;
} & PartialRecord<LocationLevelKeys, string[]>;

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

export default {
  getThemes(): Promise<ThemeMeta[]> {
    return dataApi.get(`/meta/themes`);
  },
  getPublicationMeta(publicationUuid: string): Promise<PublicationMeta> {
    return dataApi.get(`/meta/publication/${publicationUuid}`);
  },
  getReleaseMeta(releaseUuid: string): Promise<ReleaseMeta> {
    return dataApi.get(`/meta/release/${releaseUuid}`);
  },
  getPublicationSubjectMeta(
    subjectId: string,
  ): Promise<PublicationSubjectMeta> {
    return dataApi.get(`/meta/subject/${subjectId}`);
  },
  filterPublicationSubjectMeta(
    query: {
      subjectId: string;
      timePeriod?: TimePeriodQuery;
      geographicLevel?: string;
    } & PartialRecord<LocationLevelKeys, string[]>,
  ): Promise<PublicationSubjectMeta> {
    return dataApi.post('/meta/subject', query);
  },
  getTableData(query: TableDataQuery): Promise<TableDataResponse> {
    return dataApi.post('/tablebuilder', query);
  },
};
