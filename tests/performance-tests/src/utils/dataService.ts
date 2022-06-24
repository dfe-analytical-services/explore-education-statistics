import { sleep } from 'k6';
import http, { RefinedResponse } from 'k6/http';
import createClient from './httpClient';
import TestData from '../tests/testData';

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

type Topic = {
  id: string;
  title: string;
  themeId: string;
};

type ThemeAndTopics = {
  id: string;
  title: string;
  topics: Topic[];
};

type Release = {
  id: string;
  releaseName: string;
};

type PublicationAndReleases = {
  id: string;
  topicId: string;
  title: string;
  releases: Release[];
};

class DataService {
  client;

  constructor(
    adminUrl: string,
    accessToken: string,
    checkResponseStatus = true,
  ) {
    this.client = createClient(adminUrl, accessToken, checkResponseStatus);
  }

  getThemes() {
    const { json } = this.client.get<ThemeAndTopics[]>(
      `/api/themes`,
      applicationJsonHeaders,
    );
    return json;
  }

  getTheme({ title }: { title: string }): ThemeAndTopics | undefined {
    return this.getThemes().find(
      ({ title: existingTitle }) => existingTitle === title,
    );
  }

  createTheme({ title }: { title: string }) {
    const { response, json } = this.client.post<{ id: string }>(
      `/api/themes`,
      JSON.stringify({
        title,
      }),
      applicationJsonHeaders,
    );

    /* eslint-disable-next-line no-console */
    console.log(`Created Theme ${title}`);

    return {
      id: json.id,
      response,
    };
  }

  getOrCreateTheme(params: { title: string }) {
    return {
      id: this.getTheme(params)?.id ?? this.createTheme(params).id,
    };
  }

  deleteTheme({ themeId }: { themeId: string }) {
    return this.client.delete(`/api/themes/${themeId}`);
  }

  getTopic({
    themeId,
    title,
  }: {
    themeId: string;
    title: string;
  }): Topic | undefined {
    return this.getThemes()
      .flatMap(theme => theme.topics)
      .find(topic => topic.themeId === themeId && topic.title === title);
  }

  createTopic({ themeId, title }: { themeId: string; title: string }) {
    const { response, json } = this.client.post<{ id: string }>(
      `/api/topics`,
      JSON.stringify({
        themeId,
        title,
      }),
      applicationJsonHeaders,
    );

    /* eslint-disable-next-line no-console */
    console.log(`Created Topic ${title}`);

    return {
      id: json.id,
      response,
    };
  }

  getOrCreateTopic(params: { themeId: string; title: string }) {
    return {
      id: this.getTopic(params)?.id ?? this.createTopic(params).id,
    };
  }

  deleteTopic({ topicId }: { topicId: string }) {
    return this.client.delete(`/api/topics/${topicId}`);
  }

  getPublications({ topicId }: { topicId: string }): PublicationAndReleases[] {
    const { json } = this.client.get<PublicationAndReleases[]>(
      `/api/me/publications?topicId=${encodeURI(topicId)}`,
      applicationJsonHeaders,
    );
    return json;
  }

  getPublication({
    topicId,
    title,
  }: {
    topicId: string;
    title: string;
  }): PublicationAndReleases | undefined {
    return this.getPublications({ topicId }).find(
      publication =>
        publication.topicId === topicId && publication.title === title,
    );
  }

  createPublication({ topicId, title }: { topicId: string; title: string }) {
    const { response, json } = this.client.post<{ id: string }>(
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

    /* eslint-disable-next-line no-console */
    console.log(`Created Publication ${title}`);

    return {
      id: json.id,
      response,
    };
  }

  getOrCreatePublication(params: { topicId: string; title: string }) {
    return {
      id: this.getPublication(params)?.id ?? this.createPublication(params).id,
    };
  }

  getRelease({
    topicId,
    publicationTitle,
    releaseName,
  }: {
    topicId: string;
    publicationTitle: string;
    releaseName: string;
  }): Release | undefined {
    const publication = this.getPublication({
      topicId,
      title: publicationTitle,
    });
    return publication?.releases.find(
      release => release.releaseName === releaseName,
    );
  }

  createRelease({
    publicationId,
    releaseName,
    timePeriodCoverage,
  }: {
    publicationId: string;
    releaseName: string;
    timePeriodCoverage: 'AY' | 'FY';
  }) {
    const { response, json } = this.client.post<{ id: string }>(
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

    /* eslint-disable-next-line no-console */
    console.log(`Created Release ${releaseName}`);

    return {
      id: json.id,
      response,
    };
  }

  getOrCreateRelease({
    topicId,
    publicationId,
    publicationTitle,
    releaseName,
    timePeriodCoverage,
  }: {
    topicId: string;
    publicationId: string;
    publicationTitle: string;
    releaseName: string;
    timePeriodCoverage: 'AY' | 'FY';
  }) {
    return {
      id:
        this.getRelease({ topicId, publicationTitle, releaseName })?.id ??
        this.createRelease({ publicationId, releaseName, timePeriodCoverage })
          ?.id,
    };
  }

  getDataFile({
    releaseId,
    dataFileName,
  }: {
    releaseId: string;
    dataFileName: string;
  }) {
    const { json: releaseFiles } = this.client.get<
      { id: string; fileName: string }[]
    >(`/api/release/${releaseId}/data`, applicationJsonHeaders);
    return releaseFiles.find(file => file.fileName === dataFileName);
  }

  uploadDataFile({
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
  }) {
    const uploadBody = {
      title,
      file: http.file(dataFile.file, dataFile.filename),
      metaFile: http.file(metaFile.file, metaFile.filename),
    };

    const { response, json } = this.client.post<{ id: string }>(
      `/api/release/${releaseId}/data?title=${encodeURI(title)}`,
      uploadBody,
    );

    const { id: fileId } = json;

    return {
      id: fileId,
      response,
    };
  }

  waitForDataFileToImport({
    releaseId,
    fileId,
    pollingDelaySeconds = 5,
    onStatusCheckFailed,
    onStatusReceived,
    onImportFailed,
    onImportCompleted,
    onImportExceededTimeout,
  }: {
    releaseId: string;
    fileId: string;
    pollingDelaySeconds?: number;
    onStatusCheckFailed?: (statusResponse: RefinedResponse<'text'>) => void;
    onStatusReceived?: (importStatus: string) => void;
    onImportFailed?: (importStatus: string) => void;
    onImportCompleted?: () => void;
    onImportExceededTimeout?: () => void;
  }) {
    const importStartTime = Date.now();
    const importExpireTime = importStartTime + TestData.maxImportWaitTimeMillis;

    while (Date.now() < importExpireTime) {
      const { importStatus, response } = this.getImportStatus({
        releaseId,
        fileId,
      });

      if (response.status !== 200 && onStatusCheckFailed) {
        onStatusCheckFailed(response);
      } else {
        if (onStatusReceived) {
          onStatusReceived(importStatus);
        }

        if (['FAILED', 'CANCELLED'].includes(importStatus)) {
          if (onImportFailed) {
            onImportFailed(importStatus);
          }
          return;
        }

        if (['COMPLETE'].includes(importStatus)) {
          if (onImportCompleted) {
            onImportCompleted();
          }
          return;
        }
      }

      sleep(pollingDelaySeconds);
    }

    if (onImportExceededTimeout) {
      onImportExceededTimeout();
    }
  }

  getOrImportDataFile({
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
  }) {
    return {
      id:
        this.getDataFile({ releaseId, dataFileName: dataFile.filename })?.id ??
        this.uploadDataFile({ title, releaseId, dataFile, metaFile })?.id,
    };
  }

  getImportStatus({
    releaseId,
    fileId,
  }: {
    releaseId: string;
    fileId: string;
  }) {
    const { response, json } = this.client.get<{ status: string }>(
      `/api/release/${releaseId}/data/${fileId}/import/status`,
    );
    return {
      importStatus: json.status,
      response,
    };
  }

  getSubjects({ releaseId }: { releaseId: string }) {
    const { response, json } = this.client.get<{ id: string; name: string }[]>(
      `/api/data/releases/${releaseId}/subjects`,
    );
    return {
      subjects: json.map(subject => ({
        id: subject.id,
        name: subject.name,
      })),
      response,
    };
  }

  getSubjectMeta({
    releaseId,
    subjectId,
  }: {
    releaseId: string;
    subjectId: string;
  }) {
    const { response, json } = this.client.get<SubjectMeta>(
      `/api/data/release/${releaseId}/meta/subject/${subjectId}`,
    );
    return {
      subjectMeta: json,
      response,
    };
  }

  tableQuery({
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
  }) {
    const { response, json } = this.client.post<{ results: { id: string }[] }>(
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
  }
}

export default function createDataService(
  adminUrl: string,
  accessToken: string,
  checkResponseStatus = true,
) {
  return new DataService(adminUrl, accessToken, checkResponseStatus);
}
