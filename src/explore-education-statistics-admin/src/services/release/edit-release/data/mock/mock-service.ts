import { EditReleaseService } from '@admin/services/release/edit-release/data/service';
import {
  generateRandomInteger,
  generateRandomIntegerString,
  getCaptureGroups,
} from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: EditReleaseService = {
    getReleaseDataFiles: releaseId =>
      Promise.resolve(mockData.getDataFilesForRelease(releaseId).dataFiles),

    uploadDataFiles: (releaseId, request) => {
      const dataFilesView = mockData.getDataFilesForRelease(releaseId);

      // eslint-disable-next-line no-param-reassign
      dataFilesView.dataFiles = dataFilesView.dataFiles.concat([
        {
          title: request.subjectTitle,
          file: {
            id: generateRandomIntegerString(),
            fileName: request.dataFile.name,
          },
          metadataFile: {
            id: generateRandomIntegerString(),
            fileName: request.metadataFile.name,
          },
          fileSize: {
            size: generateRandomInteger(100),
            unit: 'Mb',
          },
          numberOfRows: generateRandomInteger(200000),
        },
      ]);

      return Promise.resolve(null);
    },

    deleteDataFiles: (releaseId, dataFileId) => {
      const dataFiles = mockData.getDataFilesForRelease(releaseId);

      // eslint-disable-next-line no-param-reassign
      dataFiles.dataFiles = dataFiles.dataFiles.filter(
        file => file.file.id !== dataFileId,
      );

      return Promise.resolve(null);
    },

    createDownloadDataFileLink: _ => '',

    createDownloadDataMetadataFileLink: _ => '',

    getReleaseAdhocFiles: releaseId =>
      Promise.resolve(mockData.getAdhocFilesForRelease(releaseId).adhocFiles),

    uploadAdhocFile: (releaseId, request) => {
      const adhocFilesView = mockData.getAdhocFilesForRelease(releaseId);

      // eslint-disable-next-line no-param-reassign
      adhocFilesView.adhocFiles = adhocFilesView.adhocFiles.concat([
        {
          title: request.name,
          file: {
            id: generateRandomIntegerString(),
            fileName: request.file.name,
          },
          fileSize: {
            size: generateRandomInteger(100),
            unit: 'Mb',
          },
        },
      ]);

      return Promise.resolve(null);
    },

    deleteAdhocFile: (releaseId, fileId) => {
      const adhocFiles = mockData.getAdhocFilesForRelease(releaseId);

      // eslint-disable-next-line no-param-reassign
      adhocFiles.adhocFiles = adhocFiles.adhocFiles.filter(
        file => file.file.id !== fileId,
      );

      return Promise.resolve(null);
    },

    createDownloadAdhocFileLink: _ => '',
  };

  const getReleaseDataFilesUrl = /\/release\/(.*)\/datafiles/;
  const uploadDataFilesUrl = /\/release\/(.*)\/datafiles\/upload/;
  const deleteDataFilesUrl = /\/release\/(.*)\/datafiles\/(.*)/;
  const getReleaseAdhocFilesUrl = /\/release\/(.*)\/adhoc-files/;
  const uploadAdhocFileUrl = /\/release\/(.*)\/adhoc-files\/upload/;
  const deleteAdhocFileUrl = /\/release\/(.*)\/adhoc-files\/(.*)/;

  mock.onGet(getReleaseDataFilesUrl).reply(({ url }) => {
    const [releaseId] = getCaptureGroups(getReleaseDataFilesUrl, url);
    return [200, service.getReleaseDataFiles(releaseId)];
  });

  mock.onPost(uploadDataFilesUrl).reply(({ url, data }) => {
    const [releaseId] = getCaptureGroups(uploadDataFilesUrl, url);

    const formData = data as FormData;
    const subjectTitle = formData.get('subjectTitle') as string;
    const dataFile = formData.get('dataFile') as File;
    const metadataFile = formData.get('metadataFile') as File;

    return [
      200,
      service.uploadDataFiles(releaseId, {
        subjectTitle,
        dataFile,
        metadataFile,
      }),
    ];
  });

  mock.onDelete(deleteDataFilesUrl).reply(({ url }) => {
    const [releaseId, dataFileId] = getCaptureGroups(deleteDataFilesUrl, url);
    return [204, service.deleteDataFiles(releaseId, dataFileId)];
  });

  mock.onGet(getReleaseAdhocFilesUrl).reply(({ url }) => {
    const [releaseId] = getCaptureGroups(getReleaseAdhocFilesUrl, url);
    return [200, service.getReleaseAdhocFiles(releaseId)];
  });

  mock.onPost(uploadAdhocFileUrl).reply(({ url, data }) => {
    const [releaseId] = getCaptureGroups(uploadAdhocFileUrl, url);

    const formData = data as FormData;
    const name = formData.get('name') as string;
    const file = formData.get('file') as File;

    return [
      200,
      service.uploadAdhocFile(releaseId, {
        name,
        file,
      }),
    ];
  });

  mock.onDelete(deleteAdhocFileUrl).reply(({ url }) => {
    const [releaseId, fileId] = getCaptureGroups(deleteAdhocFileUrl, url);
    return [204, service.deleteAdhocFile(releaseId, fileId)];
  });
};
