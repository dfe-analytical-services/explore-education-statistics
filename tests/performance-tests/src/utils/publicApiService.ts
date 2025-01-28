import HttpClient from './httpClient';

const applicationJsonHeaders = {
  'Content-Type': 'application/json',
};

export const datSetQueryNodeTypes = ['criteria', 'and', 'or', 'not'] as const;

export type DataSetQueryNodeType = (typeof datSetQueryNodeTypes)[number];

export const dataSetQueryIdOperators = ['eq', 'notEq', 'in', 'notIn'] as const;

export type DataSetQueryIdOperator = (typeof dataSetQueryIdOperators)[number];

export const dataSetQueryComparableOperators = [
  'eq',
  'notEq',
  'in',
  'notIn',
  'gt',
  'gte',
  'lt',
  'lte',
] as const;

export type DataSetQueryComparableOperator =
  (typeof dataSetQueryComparableOperators)[number];

export interface Publication {
  id: string;
  title: string;
  slug: string;
}

interface DataSet {
  id: string;
  title: string;
  summary: string;
  latestVersion: {
    totalResults: number;
    timePeriods: {
      start: string;
      end: string;
    };
    geographicLevels: string[];
  };
}

export interface Paged<T> {
  paging: {
    page: number;
    pageSize: number;
    totalResults: number;
    totalPages: number;
  };
  results: T[];
}

interface GeographicLevelLocations {
  level: {
    code: string;
    label: string;
  };
  options: {
    id: string;
  }[];
}

export interface DataSetMeta {
  totalResults: number;
  filters: [
    {
      id: string;
      hint: string;
      label: string;
      options: [
        {
          id: string;
          label: string;
          isAutoSelect: boolean;
        },
      ];
      name: string;
    },
  ];
  indicators: [
    {
      id: string;
      label: string;
      unit: string;
      name: string;
      decimalPlaces: 0;
    },
  ];
  locations: GeographicLevelLocations[];
  timePeriods: TimePeriod[];
}

export interface DataSetQueryIdPredicate {
  eq?: string;
  notEq?: string;
  in?: string[];
  notIn?: string[];
}

export interface DataSetQueryComparablePredicate<T> {
  eq?: T;
  notEq?: T;
  gte?: T;
  gt?: T;
  lte?: T;
  lt?: T;
  in?: T[];
  notIn?: T[];
}

export interface DataSetQueryCriteria {
  filters?: DataSetQueryIdPredicate;
  locations?: DataSetQueryIdPredicate;
  geographicLevels?: DataSetQueryIdPredicate;
  timePeriods?: DataSetQueryComparablePredicate<TimePeriod>;
}

export interface DataSetQueryConditionAnd {
  and: DataSetQueryNode[];
}

export interface DataSetQueryConditionOr {
  or: DataSetQueryNode[];
}

export interface DataSetQueryConditionNot {
  not: DataSetQueryNode[];
}

export type DataSetQueryNode =
  | DataSetQueryConditionAnd
  | DataSetQueryConditionOr
  | DataSetQueryConditionNot
  | DataSetQueryCriteria;

interface TimePeriod {
  code: string;
  period: string;
}

export interface DataSetQueryRequest {
  criteria: DataSetQueryNode;
  indicators?: string[];
  sort?: [
    {
      name: string;
      order: string;
    },
  ];
}

export interface DataSetQueryResponse {
  footnotes: [
    {
      id: string;
      content: string;
    },
  ];
  paging: {
    page: 0;
    pageSize: 0;
    totalResults: 0;
    totalPages: 0;
  };
  results: {
    id: string;
  }[];
}

class PublicApiService {
  private readonly client: HttpClient;

  constructor(baseUrl: string, checkResponseStatus = true) {
    this.client = new HttpClient({
      baseUrl,
      checkResponseStatus,
    });
  }

  listPublications() {
    const { response, json } = this.client.get<Paged<Publication>>(
      '/v1/publications?pageSize=40',
    );
    return {
      publications: json.results,
      response,
    };
  }

  listDataSets(publicationId: string) {
    const { response, json } = this.client.get<Paged<DataSet>>(
      `/v1/publications/${publicationId}/data-sets`,
    );
    return {
      dataSets: json.results,
      response,
    };
  }

  getDataSetMeta(dataSetId: string) {
    const { response, json } = this.client.get<DataSetMeta>(
      `/v1/data-sets/${dataSetId}/meta`,
    );
    return {
      meta: json,
      response,
    };
  }

  queryDataSet({
    dataSetId,
    query,
  }: {
    dataSetId: string;
    query: DataSetQueryRequest;
  }) {
    const { response, json } = this.client.post<Paged<DataSetQueryResponse>>(
      `/v1/data-sets/${dataSetId}/query`,
      JSON.stringify(query),
      applicationJsonHeaders,
    );
    return {
      results: json,
      response,
    };
  }
}

export default function createPublicApiService(
  baseUrl: string,
  checkResponseStatus = true,
) {
  return new PublicApiService(baseUrl, checkResponseStatus);
}
