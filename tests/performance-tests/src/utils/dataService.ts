import http from 'k6/http';
import createClient from './httpClient';

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

export default function createDataService(
  adminUrl: string,
  accessToken: string,
  checkResponseStatus = true,
) {
  const client = createClient(adminUrl, accessToken, checkResponseStatus);

  return {
    createTheme: ({ title, summary }: { title: string; summary?: string }) => {
      const { response, json } = client.post<{ id: string }>(
        `/api/themes`,
        JSON.stringify({
          title,
          summary,
        }),
        applicationJsonHeaders,
      );
      return {
        themeId: json.id,
        response,
      };
    },

    deleteTheme: ({ themeId }: { themeId: string }) => {
      return client.delete(`/api/themes/${themeId}`);
    },

    createTopic: ({
      themeId,
      title,
      summary,
    }: {
      themeId: string;
      title: string;
      summary?: string;
    }) => {
      const { response, json } = client.post<{ id: string }>(
        `/api/topics`,
        JSON.stringify({
          themeId,
          title,
          summary,
        }),
        applicationJsonHeaders,
      );
      return {
        topicId: json.id,
        response,
      };
    },

    deleteTopic: ({ topicId }: { topicId: string }) => {
      return client.delete(`/api/topics/${topicId}`);
    },

    createPublication: ({
      topicId,
      title,
    }: {
      topicId: string;
      title: string;
    }) => {
      const { response, json } = client.post<{ id: string }>(
        `/api/publications`,
        JSON.stringify({
          topicId,
          title,
          contact: {
            contactName: 'Team Contact',
            contactTelNo: '12345',
            teamEmail: 'team@example.com',
            teamName: 'Team',
          },
        }),
        applicationJsonHeaders,
      );
      return {
        publicationId: json.id,
        response,
      };
    },

    createRelease: ({
      publicationId,
      releaseName,
      timePeriodCoverage,
    }: {
      publicationId: string;
      releaseName: string;
      timePeriodCoverage: 'AY' | 'FY';
    }) => {
      const { response, json } = client.post<{ id: string }>(
        `/api/publications/${publicationId}/releases`,
        JSON.stringify({
          publicationId,
          releaseName,
          timePeriodCoverage: {
            value: timePeriodCoverage,
          },
          type: 'NationalStatistics',
        }),
        applicationJsonHeaders,
      );
      return {
        releaseId: json.id,
        response,
      };
    },

    importDataFile: ({
      title,
      releaseId,
      dataFile,
      metaFile,
    }: {
      title: string;
      releaseId: string;
      dataFile: {
        file: ArrayBuffer;
        filename: string;
      };
      metaFile: {
        file: ArrayBuffer;
        filename: string;
      };
    }) => {
      const uploadBody = {
        title,
        file: http.file(dataFile.file, dataFile.filename),
        metaFile: http.file(metaFile.file, metaFile.filename),
      };

      const { response, json } = client.post<{ id: string; status: string }>(
        `/api/release/${releaseId}/data?title=${encodeURI(title)}`,
        uploadBody,
      );

      return {
        fileId: json.id,
        importStatus: json.status,
        response,
      };
    },

    getImportStatus: ({
      releaseId,
      fileId,
    }: {
      releaseId: string;
      fileId: string;
    }) => {
      const { response, json } = client.get<{ status: string }>(
        `/api/release/${releaseId}/data/${fileId}/import/status`,
      );
      return {
        importStatus: json.status,
        response,
      };
    },

    getSubjects: ({ releaseId }: { releaseId: string }) => {
      const { response, json } = client.get<{ id: string; name: string }[]>(
        `/api/data/releases/${releaseId}/subjects`,
      );
      return {
        subjects: json.map(subject => ({
          id: subject.id,
          name: subject.name,
        })),
        response,
      };
    },

    getSubjectMeta: ({
      releaseId,
      subjectId,
    }: {
      releaseId: string;
      subjectId: string;
    }) => {
      const { response, json } = client.get<SubjectMeta>(
        `/api/data/release/${releaseId}/meta/subject/${subjectId}`,
      );
      return {
        subjectMeta: json,
        response,
      };
    },

    tableQuery: ({
      releaseId,
      subjectId,
      filterIds,
      indicatorIds,
      locationIds,
      startYear,
      startCode,
      endYear,
      endCode,
    }: {
      releaseId: string;
      subjectId: string;
      filterIds: string[];
      indicatorIds: string[];
      locationIds: string[];
      startYear: number;
      startCode: string;
      endYear: number;
      endCode: string;
    }) => {
      const { response, json } = client.post<{ results: { id: string }[] }>(
        `/api/data/tablebuilder/release/${releaseId}`,
        JSON.stringify({
          filters: filterIds,
          includeGeoJson: false,
          indicators: indicatorIds,
          locationIds,
          subjectId,
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
    },
  };
}
