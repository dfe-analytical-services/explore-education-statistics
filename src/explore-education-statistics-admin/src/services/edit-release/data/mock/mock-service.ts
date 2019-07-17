import {DataFile} from "@admin/services/edit-release/data/types";
import MockAdapter from 'axios-mock-adapter';

const generateRandomInteger = (max: number) =>
  Math.floor(Math.random() * Math.floor(max));
const generateRandomIntegerString = (max: number) =>
  generateRandomInteger(max).toString();

export interface DataFileView {
  dataFiles: DataFile[];
}

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  // getReleaseDataFiles
  mock.onGet(/\/release\/.*\/datafiles/).reply(({ url }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/datafiles/) : [''];
    return [
      200,
      mockData.getDataFilesForRelease(releaseIdMatch ? releaseIdMatch[1] : '').dataFiles,
    ];
  });

  // uploadDataFiles
  mock.onPost(/\/release\/.*\/datafiles\/upload/).reply(({ url, data }) => {
    const releaseIdMatch = url ? url.match(/\/release\/(.*)\/datafiles/) : [''];

    const dataFilesView = mockData.getDataFilesForRelease(
      releaseIdMatch ? releaseIdMatch[1] : '',
    );

    const formData = data as FormData;
    const subjectTitle = formData.get('subjectTitle') as string;
    const dataFile = formData.get('dataFile') as File;
    const metadataFile = formData.get('metadataFile') as File;

    // eslint-disable-next-line no-param-reassign
    dataFilesView.dataFiles = dataFilesView.dataFiles.concat([{
      title: subjectTitle,
      file: {
        id: generateRandomIntegerString(100000),
        fileName: dataFile.name,
      },
      metadataFile: {
        id: generateRandomIntegerString(100000),
        fileName: metadataFile.name,
      },
      fileSize: {
        size: generateRandomInteger(100),
        unit: 'Mb',
      },
      numberOfRows: generateRandomInteger(200000),
    }]);

    return [200];
  });

  // deleteDataFiles
  mock.onDelete(/\/release\/.*\/datafiles\/.*/).reply(({ url }) => {
    const idsMatch = url ? url.match(/\/release\/(.*)\/datafiles\/(.*)/) : [''];

    const [releaseId, dataFileId] = idsMatch ? idsMatch.slice(1) : ['', ''];

    const dataFiles = mockData.getDataFilesForRelease(releaseId);

    // eslint-disable-next-line no-param-reassign
    dataFiles.dataFiles = dataFiles.dataFiles.filter(
      file => file.file.id !== dataFileId,
    );

    return [204];
  });
};
