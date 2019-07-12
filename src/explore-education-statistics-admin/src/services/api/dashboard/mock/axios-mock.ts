import { AxiosInstance } from 'axios';
import { PrototypeLoginService } from '../../../PrototypeLoginService';

export default {
  createMockContentApiAxiosInstance: async (axiosInstance: AxiosInstance) => {
    const MockAdaptor = (await import(
      /* webpackChunkName: "axios-mock-adapter" */ 'axios-mock-adapter'
    )).default;

    const mockData = (await import(
      /* webpackChunkName: "mock-data" */ './mock-data'
    )).default;

    const mock = new MockAdaptor(axiosInstance);

    // getThemesAndTopics
    mock
      .onGet('/Themes', {
        params: { userId: PrototypeLoginService.getUserList()[0].id },
      })
      .reply(200, mockData.themesAndTopics);

    // getThemesAndTopics
    mock
      .onGet('/Publications', {
        params: {
          topicId: '67c249de-1cca-446e-8ccb-dcdac542f460',
          userId: PrototypeLoginService.getUserList()[0].id,
        },
      })
      .reply(200, mockData.dashboardPublications);

    return axiosInstance;
  },
};
