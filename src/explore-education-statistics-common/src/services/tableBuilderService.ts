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

export interface PublicationSubject {
  id: string;
  label: string;
}

export interface PublicationMeta {
  publicationId: string;
  subjects: PublicationSubject[];
}

export interface PublicationSubjectMeta {
  filters: {
    [key: string]: {
      legend: string;
      hint?: string;
      options: GroupedFilterOptions;
    };
  };
  indicators: {
    [key: string]: {
      label: string;
      options: IndicatorOption[];
    };
  };
  locations: {
    [key: string]: {
      legend: string;
      hint?: string;
      options: FilterOption[];
    };
  };
  timePeriod: {
    hint?: string;
    legend: string;
    options: TimePeriodOption[];
  };
}

export interface TableData {
  publicationId: string;
  subjectId: string;
  releaseId: string;
  releaseDate: string;
  geographicLevel: string;
  result: {
    year: number;
    timeIdentifier: string;
    measures: {
      [indicator: string]: string;
    };
    filters: string[];
  }[];
}

// TODO: Remove this
export interface CharacteristicsData {
  publicationId: string;
  releaseId: string;
  releaseDate: string;
  result: {
    timePeriod: number;
    schoolType: string;
    indicators: {
      [indicator: string]: string;
    };
    characteristic: {
      label: string;
      name: string;
      description?: string | null;
      name2?: string | null;
    };
  }[];
}

export type DataTableResult = CharacteristicsData['result'][0];

export default {
  getPublicationMeta(publicationUuid: string): Promise<PublicationMeta> {
    return dataApi.get(`/meta/publication/${publicationUuid}`);
  },
  getPublicationSubjectMeta(
    subjectId: string,
  ): Promise<PublicationSubjectMeta> {
    return dataApi.get(`/meta/subject/${subjectId}`);
  },
  getTableData(query: {
    subjectId: string;
    filters: string[];
    indicators: string[];
    startYear: number;
    endYear: number;
    geographicLevel: string;
    countries?: string[];
    regions?: string[];
    localAuthorities?: string[];
    localAuthorityDistricts?: string[];
  }): Promise<TableData> {
    return dataApi.post('/tablebuilder', query);
  },
};
