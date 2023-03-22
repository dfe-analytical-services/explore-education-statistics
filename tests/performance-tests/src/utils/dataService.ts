import HttpClient from './httpClient';

const applicationJsonHeaders = {
  'Content-Type': 'application/json',
};

export interface SubjectMeta {
  filters: {
    [filter: string]: {
      options: {
        [filterGroup: string]: {
          options: {
            value: string;
          }[];
        };
      };
    };
  };
  indicators: {
    [indicatorGroup: string]: {
      options: {
        value: string;
      }[];
    };
  };
  timePeriod: {
    options: {
      code: string;
      year: number;
    }[];
  };
  locations: {
    [geographicLevel: string]: {
      options: {
        id: string;
      }[];
    };
  };
}

class DataService {
  private readonly client: HttpClient;

  constructor(baseUrl: string, checkResponseStatus = true) {
    this.client = new HttpClient({
      baseUrl,
      checkResponseStatus,
    });
  }

  getSubjectMeta({
    releaseId,
    subjectId,
  }: {
    releaseId: string;
    subjectId: string;
  }) {
    const { response, json } = this.client.get<SubjectMeta>(
      `/data/release/${releaseId}/meta/subject/${subjectId}`,
    );
    return {
      subjectMeta: json,
      response,
    };
  }

  tableQuery({
    publicationId,
    subjectId,
    filterIds,
    indicatorIds,
    locationIds,
    startYear,
    startCode,
    endYear,
    endCode,
  }: {
    publicationId: string;
    subjectId: string;
    filterIds: string[];
    indicatorIds: string[];
    locationIds: string[];
    startYear: number;
    startCode: string;
    endYear: number;
    endCode: string;
  }) {
    const { response, json } = this.client.post<{ results: { id: string }[] }>(
      '/tablebuilder',
      JSON.stringify({
        filters: filterIds,
        includeGeoJson: false,
        indicators: indicatorIds,
        locationIds,
        subjectId,
        publicationId,
        timePeriod: {
          startYear,
          startCode,
          endYear,
          endCode,
        },
      }),
      applicationJsonHeaders,
    );
    return {
      results: json.results,
      response,
    };
  }
}

export default function createDataService(
  baseUrl: string,
  checkResponseStatus = true,
) {
  return new DataService(baseUrl, checkResponseStatus);
}
