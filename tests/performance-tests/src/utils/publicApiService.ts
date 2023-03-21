import { RefinedResponse } from 'k6/http';
import HttpClient from './httpClient';

const applicationJsonHeaders = {
  'Content-Type': 'application/json',
};

export const datSetQueryNodeTypes = ['criteria', 'and', 'or', 'not'] as const;

export type DataSetQueryNodeType = typeof datSetQueryNodeTypes[number];

export const dataSetQueryIdOperators = ['eq', 'notEq', 'in', 'notIn'] as const;

export type DataSetQueryIdOperator = typeof dataSetQueryIdOperators[number];

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

export type DataSetQueryComparableOperator = typeof dataSetQueryComparableOperators[number];

export interface Publication {
  id: string;
  title: string;
  slug: string;
}

interface DataSet {
  id: string;
  title: string;
  description: string;
  timePeriods: {
    start: string;
    end: string;
  };
  geographicLevels: string[];
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

interface Location {
  id: string;
  level: string;
  options: GeographicLevelLocations[];
  label: string;
  code: string;
}

interface GeographicLevelLocations {
  [level: string]: Location[];
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
          isAggregate: boolean;
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
  locations: GeographicLevelLocations;
  timePeriods: (TimePeriod & { label: string })[];
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
  year: number;
}

export interface DataSetQueryRequest {
  facets: DataSetQueryNode;
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

// Some Publications and their data sets are currently not discoverable via
// the API, so we can include them manually here.
const hiddenPublications: Publication[] = [
  {
    id: '1681557f-510f-446e-bc9a-f2c7a59d1cfa',
    slug: 'benchmark-publication',
    title: 'Benchmark Publication',
  },
];

const hiddenDataSets: { [publicationId: string]: { dataSets: DataSet[] } } = {
  '1681557f-510f-446e-bc9a-f2c7a59d1cfa': {
    dataSets: [
      {
        id: 'a96044e5-2310-4890-a601-8ca0b67d2964',
        title: 'QUA01',
        description: '',
        geographicLevels: ['National'],
        timePeriods: {
          start: '2013/14',
          end: '2018/19',
        },
      },
    ],
  },
};

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
    const publications: Publication[] = [
      ...json.results,
      ...hiddenPublications,
    ];
    return {
      publications,
      response,
    };
  }

  listDataSets(publicationId: string) {
    let response: RefinedResponse<'text'> | undefined;
    let json: DataSet[];

    if (Object.keys(hiddenDataSets).includes(publicationId)) {
      json = hiddenDataSets[publicationId].dataSets;
    } else {
      const result = this.client.get<DataSet[]>(
        `/v1/publications/${publicationId}/data-sets`,
      );
      response = result.response;
      json = result.json;
    }
    return {
      dataSets: json,
      response,
    };
  }

  getDataSetsMeta(dataSetId: string) {
    const { response, json } = this.client.get<DataSetMeta>(
      `/v1/data-sets/${dataSetId}/meta`,
    );
    return {
      meta: json,
      response,
    };
  }

  getDataSetQueryUrl({ dataSetId, page }: { dataSetId: string; page: number }) {
    return `/v1/data-sets/${dataSetId}/query?page=${page}&pageSize=10000`;
  }

  queryDataSet({
    dataSetId,
    query,
    page = 1,
  }: {
    dataSetId: string;
    query: DataSetQueryRequest;
    page?: number;
  }) {
    const url = this.getDataSetQueryUrl({
      dataSetId,
      page,
    });
    const { response, json } = this.client.post<Paged<DataSetQueryResponse>>(
      url,
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
