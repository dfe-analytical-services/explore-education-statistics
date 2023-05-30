import HttpClient from './httpClient';
import { SubjectMeta, TableQuery } from './types';

const applicationJsonHeaders = {
  'Content-Type': 'application/json',
};

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

  tableQuery(query: TableQuery) {
    const { response, json } = this.client.post<{ results: { id: string }[] }>(
      '/tablebuilder',
      JSON.stringify(query),
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
