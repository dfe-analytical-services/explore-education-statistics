import { Dictionary, PartialRecord } from '@common/types';
import { SubjectMeta } from '@frontend/services/permalinkService';
import { dataApi } from './api';

export interface FilterOption {
  label: string;
  value: string;
}

export interface IndicatorOption extends FilterOption {
  unit: string;
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

export interface PublicationSubjectMeta {
  filters: Dictionary<{
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
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

export type LocationLevelKeys =
  | 'country'
  | 'institution'
  | 'localAuthority'
  | 'localAuthorityDistrict'
  | 'localEnterprisePartnership'
  | 'multiAcademyTrust'
  | 'mayoralCombinedAuthority'
  | 'opportunityArea'
  | 'parliamentaryConstituency'
  | 'region'
  | 'rscRegion'
  | 'sponsor'
  | 'ward';

export interface TableData {
  subjectMeta: SubjectMeta;
  results: {
    timePeriod: string;
    measures: Dictionary<string>;
    filters: string[];
    location: Dictionary<{
      code: string;
      name: string;
    }>;
  }[];
}

export interface TablePermalink {
  id: string;
  title: string;
  created: string;
  url: string;
}

interface TimePeriodQuery {
  startYear: number;
  startCode: string;
  endYear: number;
  endCode: string;
}

export type TableDataQuery = {
  subjectId: string;
  filters: string[];
  indicators: string[];
  timePeriod?: TimePeriodQuery;
  geographicLevel?: string;
} & PartialRecord<LocationLevelKeys, string[]>;

export default {
  getThemes(): Promise<ThemeMeta[]> {
    return dataApi.get(`/meta/themes`);
  },
  getPublicationMeta(publicationUuid: string): Promise<PublicationMeta> {
    return dataApi.get(`/meta/publication/${publicationUuid}`);
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
  getTableData(query: TableDataQuery): Promise<TableData> {
    return dataApi.post('/tablebuilder/new', query);
  },
  getTablePermalink(query: TableDataQuery): Promise<TablePermalink> {
    return dataApi.post('/permalink', query);
  },
};
