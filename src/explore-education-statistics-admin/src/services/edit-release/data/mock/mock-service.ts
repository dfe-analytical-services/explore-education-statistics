import {EditReleaseService} from "@admin/services/edit-release/data/service";
import MockAdapter from 'axios-mock-adapter';

const generateRandomInteger = (max: number) =>
  Math.floor(Math.random() * Math.floor(max));

const generateRandomIntegerString = (max: number) =>
  generateRandomInteger(max).toString();

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
      dataFilesView.dataFiles = dataFilesView.dataFiles.concat([{
        title: request.subjectTitle,
        file: {
          id: generateRandomIntegerString(100000),
          fileName: request.dataFile.name,
        },
        metadataFile: {
          id: generateRandomIntegerString(100000),
          fileName: request.metadataFile.name,
        },
        fileSize: {
          size: generateRandomInteger(100),
          unit: 'Mb',
        },
        numberOfRows: generateRandomInteger(200000),
      }]);

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

    createDownloadDataFileLink: releaseId => '',

    createDownloadDataMetadataFileLink: releaseId => '',

    getReleaseAdhocFiles: releaseId =>
      Promise.resolve(mockData.getAdhocFilesForRelease(releaseId).adhocFiles),

    uploadAdhocFile: (releaseId, request) => {

      const adhocFilesView = mockData.getAdhocFilesForRelease(releaseId);

      // eslint-disable-next-line no-param-reassign
      adhocFilesView.adhocFiles = adhocFilesView.adhocFiles.concat([{
        title: request.name,
        file: {
          id: generateRandomIntegerString(100000),
          fileName: request.file.name,
        },
        fileSize: {
          size: generateRandomInteger(100),
          unit: 'Mb',
        },
      }]);

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

    createDownloadAdhocFileLink: releaseId => '',
  };

  // getReleaseDataFiles
  mock.onGet(/\/release\/.*\/datafiles/).reply(({ url }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/datafiles/) : [''];
    return [
      200,
      service.getReleaseDataFiles(releaseIdMatch ? releaseIdMatch[1] : ''),
    ];
  });

  // uploadDataFiles
  mock.onPost(/\/release\/.*\/datafiles\/upload/).reply(({ url, data }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/datafiles\/upload/) : [''];

    const formData = data as FormData;
    const subjectTitle = formData.get('subjectTitle') as string;
    const dataFile = formData.get('dataFile') as File;
    const metadataFile = formData.get('metadataFile') as File;

    return [
      200,
      service.uploadDataFiles(releaseIdMatch ? releaseIdMatch[1] : '', {
        subjectTitle,
        dataFile,
        metadataFile,
      }),
    ];
  });

  // deleteDataFiles
  mock.onDelete(/\/release\/.*\/datafiles\/.*/).reply(({ url }) => {
    const idsMatch = url ? url.match(/\/release\/(.*)\/datafiles\/(.*)/) : [''];
    const [releaseId, dataFileId] = idsMatch ? idsMatch.slice(1) : ['', ''];
    return [
      204,
      service.deleteDataFiles(releaseId, dataFileId)
    ];
  });

  // getReleaseAdhocFiles
  mock.onGet(/\/release\/.*\/adhoc-files/).reply(({ url }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/adhoc-files/) : [''];
    return [
      200,
      service.getReleaseAdhocFiles(releaseIdMatch ? releaseIdMatch[1] : ''),
    ];
  });

  // uploadAdhocFile - `/release/${releaseId}/adhoc-files/upload`
  mock.onPost(/\/release\/.*\/adhoc-files\/upload/).reply(({ url, data }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/adhoc-files\/upload/) : [''];

    const formData = data as FormData;
    const name = formData.get('name') as string;
    const file = formData.get('file') as File;

    return [
      200,
      service.uploadAdhocFile(releaseIdMatch ? releaseIdMatch[1] : '', {
        name,
        file,
      }),
    ];
  });

  // deleteAdhocFile
  mock.onDelete(/\/release\/.*\/adhoc-files\/.*/).reply(({ url }) => {
    const idsMatch = url ? url.match(/\/release\/(.*)\/adhoc-files\/(.*)/) : [''];
    const [releaseId, fileId] = idsMatch ? idsMatch.slice(1) : ['', ''];
    return [
      204,
      service.deleteAdhocFile(releaseId, fileId)
    ];
  });
};
