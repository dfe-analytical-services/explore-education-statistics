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
      .onGet('/Theme', {
        params: { userId: PrototypeLoginService.getUserList()[0].id },
      })
      .reply(200, mockData.themesAndTopics);

    return axiosInstance;
  },
};
