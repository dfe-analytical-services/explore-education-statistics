import { sleep } from 'k6';
import http, { RefinedResponse } from 'k6/http';
import HttpClient from './httpClient';
import TestData from '../tests/testData';

const applicationJsonHeaders = {
  'Content-Type': 'application/json',
};

type OverallStage =
  | 'Validating'
  | 'Complete'
  | 'Failed'
  | 'Invalid'
  | 'Scheduled'
  | 'Started'
  | 'Superseded';

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
        id?: string;
        level?: string;
        options?: {
          id: string;
        }[];
      }[];
    };
  };
}

interface Topic {
  id: string;
  title: string;
  themeId: string;
}

interface ThemeAndTopics {
  id: string;
  title: string;
  topics: Topic[];
}

interface Release {
  id: string;
  year: number;
  approvalStatus: string;
}

interface Publication {
  id: string;
  title: string;
}

type DataFileImportHandler = (
  adminService: AdminService,
  releaseId: string,
) => {
  response: RefinedResponse<'text'>;
  id: string;
};

export class AdminService {
  private readonly client: HttpClient;

  constructor(
    baseUrl: string,
    accessToken: string,
    checkResponseStatus = true,
  ) {
    this.client = new HttpClient({
      baseUrl,
      accessToken,
      checkResponseStatus,
    });
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

  getPublications({ topicId }: { topicId: string }): Publication[] {
    const { json } = this.client.get<Publication[]>(
      `/api/publications?topicId=${encodeURI(topicId)}`,
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
  }): Publication | undefined {
    return this.getPublications({ topicId }).find(
      publication => publication.title === title,
    );
  }

  getPublicationReleases({
    publicationId,
    live,
  }: {
    publicationId: string;
    live: boolean;
  }): Release[] {
    const { json } = this.client.get<{ results: Release[] }>(
      `/api/publication/${publicationId}/releases?live=${live}&pageSize=100&includePermissions=false`,
      applicationJsonHeaders,
    );
    return json.results;
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
        summary: `${title} summary`,
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
    year,
  }: {
    topicId: string;
    publicationTitle: string;
    year: number;
  }): Release | undefined {
    const publication = this.getPublication({
      topicId,
      title: publicationTitle,
    });
    if (!publication) {
      return undefined;
    }
    const liveReleases = this.getPublicationReleases({
      publicationId: publication.id,
      live: true,
    });

    const liveRelease = liveReleases.find(release => release.year === year);

    if (liveRelease) {
      return liveRelease;
    }

    const draftReleases = this.getPublicationReleases({
      publicationId: publication.id,
      live: false,
    });

    return draftReleases.find(release => release.year === year);
  }

  createRelease({
    publicationId,
    year,
    timePeriodCoverage,
  }: {
    publicationId: string;
    year: number;
    timePeriodCoverage: 'AY' | 'FY';
  }) {
    const { response, json } = this.client.post<{ id: string }>(
      `/api/publications/${publicationId}/releases`,
      JSON.stringify({
        publicationId,
        year,
        timePeriodCoverage: {
          value: timePeriodCoverage,
        },
        type: 'NationalStatistics',
      }),
      applicationJsonHeaders,
    );

    /* eslint-disable-next-line no-console */
    console.log(`Created Release ${year}`);

    return {
      id: json.id,
      response,
    };
  }

  getOrCreateRelease({
    topicId,
    publicationId,
    publicationTitle,
    year,
    timePeriodCoverage,
  }: {
    topicId: string;
    publicationId: string;
    publicationTitle: string;
    year: number;
    timePeriodCoverage: 'AY' | 'FY';
  }) {
    const existingRelease = this.getRelease({
      topicId,
      publicationTitle,
      year,
    });

    if (existingRelease) {
      return {
        id: existingRelease.id,
        approvalStatus: existingRelease.approvalStatus,
      };
    }
    return {
      id: this.createRelease({ publicationId, year, timePeriodCoverage })?.id,
      approvalStatus: 'Draft',
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

  uploadDataZipFile({
    title,
    releaseId,
    zipFile,
  }: {
    title: string;
    releaseId: string;
    zipFile: {
      file: ArrayBuffer;
      filename: string;
    };
  }) {
    return this.uploadDataFile({
      title,
      releaseId,
      dataFile: zipFile,
    });
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
    metaFile?: {
      file: ArrayBuffer;
      filename: string;
    };
  }) {
    const zipUpload = dataFile.filename.endsWith('.zip');

    if (!zipUpload && !metaFile) {
      throw new Error(
        `Data file ${dataFile.filename} must have a corresponding metafile if it is not a ZIP`,
      );
    }

    const uploadBody = {
      title,
      [zipUpload ? 'zipFile' : 'file']: http.file(
        dataFile.file,
        dataFile.filename,
      ),
      metaFile: !zipUpload
        ? http.file(metaFile?.file as ArrayBuffer, metaFile?.filename)
        : '',
    };

    const { response, json } = this.client.post<{ id: string }>(
      `/api/release/${releaseId}/${
        zipUpload ? 'zip-data' : 'data'
      }?title=${encodeURI(title)}`,
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
            return;
          }
          throw new Error(
            `Import of file ${fileId} for Release ${releaseId} failed`,
          );
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
    metaFile?: {
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

  getOrImportDataZipFile({
    title,
    releaseId,
    zipFile,
  }: {
    title: string;
    releaseId: string;
    zipFile: {
      file: ArrayBuffer;
      filename: string;
    };
  }) {
    return {
      id:
        this.getDataFile({ releaseId, dataFileName: zipFile.filename })?.id ??
        this.uploadDataFile({ title, releaseId, dataFile: zipFile })?.id,
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

  addDataGuidance({
    releaseId,
    content = '<p>Test Content</p>',
    subjects,
  }: {
    releaseId: string;
    content?: string;
    subjects: {
      id: string;
      content: string;
    }[];
  }) {
    const { response } = this.client.patch(
      `/api/release/${releaseId}/data-guidance`,
      JSON.stringify({
        content,
        subjects,
      }),
      applicationJsonHeaders,
    );
    return {
      response,
    };
  }

  approveRelease({
    releaseId,
    notifySubscribers = false,
    latestInternalReleaseNote = 'Approved',
  }: {
    releaseId: string;
    notifySubscribers?: boolean;
    latestInternalReleaseNote?: string;
  }) {
    const { response } = this.client.post(
      `/api/releases/${releaseId}/status`,
      JSON.stringify({
        notifySubscribers,
        latestInternalReleaseNote,
        approvalStatus: 'Approved',
        publishMethod: 'Immediate',
      }),
      applicationJsonHeaders,
    );
    return {
      response,
    };
  }

  getReleaseApprovalStatus({ releaseId }: { releaseId: string }) {
    const { response, json } = this.client.get<{ overallStage: OverallStage }>(
      `/api/releases/${releaseId}/stage-status`,
      applicationJsonHeaders,
    );
    return {
      status: json.overallStage,
      response,
    };
  }

  waitForReleaseToBePublished({
    releaseId,
    pollingDelaySeconds = 5,
    onStatusCheckFailed,
    onStatusReceived,
    onPublishingFailed,
    onPublishingCompleted,
    onPublishingExceededTimeout,
  }: {
    releaseId: string;
    pollingDelaySeconds?: number;
    onStatusCheckFailed?: (statusResponse: RefinedResponse<'text'>) => void;
    onStatusReceived?: (status: string) => void;
    onPublishingFailed?: (status: string) => void;
    onPublishingCompleted?: () => void;
    onPublishingExceededTimeout?: () => void;
  }) {
    const publishingStartTime = Date.now();
    const publishingExpireTime =
      publishingStartTime + TestData.maxPublishingWaitTimeMillis;

    while (Date.now() < publishingExpireTime) {
      const { status, response } = this.getReleaseApprovalStatus({ releaseId });

      switch (response.status) {
        case 204: {
          sleep(pollingDelaySeconds);
          break;
        }
        case 200: {
          if (onStatusReceived) {
            onStatusReceived(status);
          }

          const failureStates: OverallStage[] = ['Failed', 'Invalid'];

          if (failureStates.includes(status)) {
            if (onPublishingFailed) {
              onPublishingFailed(status);
              return;
            }
            throw new Error(`Publishing failed for Release ${releaseId}`);
          }

          if (status === 'Complete') {
            if (onPublishingCompleted) {
              onPublishingCompleted();
            }
            return;
          }

          sleep(pollingDelaySeconds);
          break;
        }
        default: {
          if (onStatusCheckFailed) {
            onStatusCheckFailed(response);
          }

          sleep(pollingDelaySeconds);
        }
      }
    }

    if (onPublishingExceededTimeout) {
      onPublishingExceededTimeout();
    }
  }
}

export function getDataFileUploadStrategy({
  filename,
}: {
  filename: string;
}): {
  filename: string;
  isZip: boolean;
  subjectName: string;
  getOrImportSubject: DataFileImportHandler;
} {
  const isZip = filename.endsWith('.zip');
  const subjectName = filename;

  /* eslint-disable no-restricted-globals */
  const zipFile = isZip ? open(`admin/import/assets/${filename}`, 'b') : null;
  const subjectFile = !isZip
    ? open(`admin/import/assets/${filename}`, 'b')
    : null;
  const subjectMetaFile = !isZip
    ? open(`admin/import/assets/${filename.replace('.csv', '.meta.csv')}`, 'b')
    : null;
  /* eslint-enable no-restricted-globals */

  return {
    isZip,
    filename,
    subjectName: filename,
    getOrImportSubject: (adminService, releaseId) =>
      isZip
        ? adminService.uploadDataZipFile({
            title: subjectName,
            releaseId,
            zipFile: {
              file: zipFile as ArrayBuffer,
              filename: `${subjectName}.zip`,
            },
          })
        : adminService.uploadDataFile({
            title: subjectName,
            releaseId,
            dataFile: {
              file: subjectFile as ArrayBuffer,
              filename: `${subjectName}.csv`,
            },
            metaFile: {
              file: subjectMetaFile as ArrayBuffer,
              filename: `${subjectName}.meta.csv`,
            },
          }),
  };
}

export default function createAdminService(
  baseUrl: string,
  accessToken: string,
  checkResponseStatus = true,
) {
  return new AdminService(baseUrl, accessToken, checkResponseStatus);
}
