import { AxiosInstance } from 'axios';

const generateRandomInteger = (max: number) => Math.floor(Math.random() * Math.floor(max));
const generateRandomIntegerString = (max: number) => generateRandomInteger(max).toString();

export default {
  createMockContentApiAxiosInstance: async (axiosInstance: AxiosInstance) => {
    const MockAdaptor = (await import(
      /* webpackChunkName: "axios-mock-adapter" */ 'axios-mock-adapter'
    )).default;

    const mockData = (await import(
      /* webpackChunkName: "mock-data" */ './mock-data'
    )).default;

    const mock = new MockAdaptor(axiosInstance);

    // getReleaseDataFiles
    mock.onGet(/\/release\/.*\/datafiles/).reply(({ url }) => {
      const releaseIdMatch = url
        ? url.match(/\/release\/(.*)\/datafiles/)
        : [''];
      return [
        200,
        mockData.getDataFilesForRelease(
          releaseIdMatch ? releaseIdMatch[1] : '',
        ),
      ];
    });

    // uploadDataFiles
    mock.onPost(/\/release\/.*\/datafiles\/upload/).reply(({ url, data }) => {
      const releaseIdMatch = url
        ? url.match(/\/release\/(.*)\/datafiles/)
        : [''];

      const dataFilesView = mockData.getDataFilesForRelease(releaseIdMatch ? releaseIdMatch[1] : '');

      const formData = data as FormData;
      const subjectTitle = formData.get('subjectTitle') as string;
      const dataFile = formData.get('dataFile') as File;
      const metadataFile = formData.get('metadataFile') as File;

      dataFilesView.dataFiles.push({
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
      });

      return [200];
    });

    // deleteDataFiles
    mock.onDelete(/\/release\/.*\/datafiles\/.*/).reply(({ url }) => {
      const idsMatch = url
        ? url.match(/\/release\/(.*)\/datafiles\/(.*)/)
        : [''];

      const [releaseId, dataFileId] = idsMatch ? idsMatch.slice(1) : ['', ''];

      const dataFilesView = mockData.getDataFilesForRelease(releaseId);

      // eslint-disable-next-line no-param-reassign
      dataFilesView.dataFiles = dataFilesView.dataFiles.filter(file => file.file.id !== dataFileId);

      return [204];
    });

    return axiosInstance;
  },
};
