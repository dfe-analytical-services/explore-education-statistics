import { AxiosInstance } from 'axios';

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
          id: '1234',
          fileName: dataFile.name,
        },
        metadataFile: {
          id: '5678',
          fileName: metadataFile.name,
        },
        fileSize: {
          size: dataFile.size,
          unit: 'Mb',
        },
        numberOfRows: 102005,
      });

      return [
        200,
        null,
      ];
    });

    return axiosInstance;
  },
};
