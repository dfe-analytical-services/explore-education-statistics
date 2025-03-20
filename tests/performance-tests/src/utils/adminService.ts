import { sleep } from 'k6';
import http, { RefinedResponse } from 'k6/http';
import HttpClient from './httpClient';
import TestData from '../tests/testData';
import {
  Publication,
  Release,
  Theme,
  SubjectMeta,
  FullTableQuery,
  OverallStage,
} from './types';

const applicationJsonHeaders = {
  'Content-Type': 'application/json',
};

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
    const { json } = this.client.get<Theme[]>(
      `/api/themes`,
      applicationJsonHeaders,
    );
    return json;
  }

  getTheme({ title }: { title: string }): Theme | undefined {
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

  getPublications({ themeId }: { themeId: string }): Publication[] {
    const { json } = this.client.get<Publication[]>(
      `/api/publications?themeId=${encodeURI(themeId)}`,
      applicationJsonHeaders,
    );
    return json;
  }

  getPublication({
    themeId,
    title,
  }: {
    themeId: string;
    title: string;
  }): Publication | undefined {
    return this.getPublications({ themeId }).find(
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

  createPublication({ themeId, title }: { themeId: string; title: string }) {
    const { response, json } = this.client.post<{ id: string }>(
      `/api/publications`,
      JSON.stringify({
        themeId,
        title,
        contact: {
          contactName: 'Team Contact',
          contactTelNo: '01234 567890',
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

  getOrCreatePublication(params: { themeId: string; title: string }) {
    return {
      id: this.getPublication(params)?.id ?? this.createPublication(params).id,
    };
  }

  getReleaseVersion({
    themeId,
    publicationTitle,
    year,
  }: {
    themeId: string;
    publicationTitle: string;
    year: number;
  }): Release | undefined {
    const publication = this.getPublication({
      themeId,
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
        type: 'AccreditedOfficialStatistics',
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
    themeId,
    publicationId,
    publicationTitle,
    year,
    timePeriodCoverage,
  }: {
    themeId: string;
    publicationId: string;
    publicationTitle: string;
    year: number;
    timePeriodCoverage: 'AY' | 'FY';
  }) {
    const existingRelease = this.getReleaseVersion({
      themeId,
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

  uploadDataZipFile({
    title,
    releaseVersionId,
    zipFile,
  }: {
    title: string;
    releaseVersionId: string;
    zipFile: {
      file: ArrayBuffer;
      filename: string;
    };
  }) {
    return this.uploadDataFile({
      title,
      releaseVersionId,
      dataFile: zipFile,
    });
  }

  uploadDataFile({
    title,
    releaseVersionId,
    dataFile,
    metaFile,
  }: {
    title: string;
    releaseVersionId: string;
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
      `/api/release/${releaseVersionId}/${
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
    releaseVersionId,
    fileId,
    pollingDelaySeconds = 5,
    onStatusCheckFailed,
    onStatusReceived,
    onImportFailed,
    onImportCompleted,
    onImportExceededTimeout,
  }: {
    releaseVersionId: string;
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
        releaseVersionId,
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
            `Import of file ${fileId} for Release ${releaseVersionId} failed`,
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

  getImportStatus({
    releaseVersionId,
    fileId,
  }: {
    releaseVersionId: string;
    fileId: string;
  }) {
    const { response, json } = this.client.get<{ status: string }>(
      `/api/release/${releaseVersionId}/data/${fileId}/import/status`,
    );
    return {
      importStatus: json.status,
      response,
    };
  }

  getSubjects({ releaseVersionId }: { releaseVersionId: string }) {
    const { response, json } = this.client.get<{ id: string; name: string }[]>(
      `/api/data/releases/${releaseVersionId}/subjects`,
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
    releaseVersionId,
    subjectId,
  }: {
    releaseVersionId: string;
    subjectId: string;
  }) {
    const { response, json } = this.client.get<SubjectMeta>(
      `/api/data/release/${releaseVersionId}/meta/subject/${subjectId}`,
    );
    return {
      subjectMeta: json,
      response,
    };
  }

  tableQuery({
    releaseVersionId,
    query,
  }: {
    releaseVersionId: string;
    query: FullTableQuery;
  }) {
    const { response, json } = this.client.post<{ results: { id: string }[] }>(
      `/api/data/tablebuilder/release/${releaseVersionId}`,
      JSON.stringify(query),
      applicationJsonHeaders,
    );
    return {
      results: json.results,
      response,
    };
  }

  addDataGuidance({
    releaseVersionId,
    content = '<p>Test Content</p>',
    subjects,
  }: {
    releaseVersionId: string;
    content?: string;
    subjects: {
      id: string;
      content: string;
    }[];
  }) {
    const { response } = this.client.patch(
      `/api/release/${releaseVersionId}/data-guidance`,
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
    releaseVersionId,
    notifySubscribers = false,
    latestInternalReleaseNote = 'Approved',
  }: {
    releaseVersionId: string;
    notifySubscribers?: boolean;
    latestInternalReleaseNote?: string;
  }) {
    const { response } = this.client.post(
      `/api/releases/${releaseVersionId}/status`,
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

  getReleaseApprovalStatus({ releaseVersionId }: { releaseVersionId: string }) {
    const { response, json } = this.client.get<{ overallStage: OverallStage }>(
      `/api/releases/${releaseVersionId}/stage-status`,
      applicationJsonHeaders,
    );
    return {
      status: json.overallStage,
      response,
    };
  }

  waitForReleaseToBePublished({
    releaseVersionId,
    pollingDelaySeconds = 5,
    onStatusCheckFailed,
    onStatusReceived,
    onPublishingFailed,
    onPublishingCompleted,
    onPublishingExceededTimeout,
  }: {
    releaseVersionId: string;
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
      const { status, response } = this.getReleaseApprovalStatus({
        releaseVersionId,
      });

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
            throw new Error(
              `Publishing failed for Release ${releaseVersionId}`,
            );
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

export function getDataFileUploadStrategy({ filename }: { filename: string }): {
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
    getOrImportSubject: (adminService, releaseVersionId) =>
      isZip
        ? adminService.uploadDataZipFile({
            title: subjectName,
            releaseVersionId,
            zipFile: {
              file: zipFile as ArrayBuffer,
              filename: `${subjectName}.zip`,
            },
          })
        : adminService.uploadDataFile({
            title: subjectName,
            releaseVersionId,
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
