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

    return axiosInstance;
  },
};
