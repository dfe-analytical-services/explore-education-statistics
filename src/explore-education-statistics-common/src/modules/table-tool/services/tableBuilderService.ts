import { FullTable } from '@common/modules/table-tool/types/fullTable.js';
import { dataApi } from '@common/services/api';
import { Dictionary, PartialRecord } from '@common/types';

export interface FilterOption {
  label: string;
  value: string;
  filterGroup?: string;
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

export type TimeIdentifier =
  | 'AY'
  | 'AYQ1'
  | 'AYQ1Q2'
  | 'AYQ1Q3'
  | 'AYQ1Q4'
  | 'AYQ2'
  | 'AYQ2Q3'
  | 'AYQ2Q4'
  | 'AYQ3'
  | 'AYQ3Q4'
  | 'AYQ4'
  | 'CY'
  | 'CYQ1'
  | 'CYQ1Q2'
  | 'CYQ1Q3'
  | 'CYQ1Q4'
  | 'CYQ2'
  | 'CYQ2Q3'
  | 'CYQ2Q4'
  | 'CYQ3'
  | 'CYQ3Q4'
  | 'CYQ4'
  | 'FY'
  | 'FYQ1'
  | 'FYQ1Q2'
  | 'FYQ1Q3'
  | 'FYQ1Q4'
  | 'FYQ2'
  | 'FYQ2Q3'
  | 'FYQ2Q4'
  | 'FYQ3'
  | 'FYQ3Q4'
  | 'FYQ4'
  | 'TY'
  | 'TYQ1'
  | 'TYQ1Q2'
  | 'TYQ1Q3'
  | 'TYQ1Q4'
  | 'TYQ2'
  | 'TYQ2Q3'
  | 'TYQ2Q4'
  | 'TYQ3'
  | 'TYQ3Q4'
  | 'TYQ4'
  | 'HT5'
  | 'HT6'
  | 'EOM'
  | 'T1'
  | 'T1T2'
  | 'T2'
  | 'T3'
  | 'M1'
  | 'M2'
  | 'M3'
  | 'M4'
  | 'M5'
  | 'M6'
  | 'M7'
  | 'M8'
  | 'M9'
  | 'M10'
  | 'M11'
  | 'M12';

export interface TimePeriodQuery {
  startYear: number;
  startCode: TimeIdentifier;
  endYear: number;
  endCode: TimeIdentifier;
}

export type TableDataQuery = {
  publicationId?: string;
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
  getTableData(query: TableDataQuery, releaseId?: string): Promise<FullTable> {
    return dataApi.post(
      '/tablebuilder',
      query,
      releaseId
        ? {
            params: {
              releaseId,
            },
          }
        : undefined,
    );
  },
};
