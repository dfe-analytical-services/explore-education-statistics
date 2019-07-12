import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';
import { AxiosInstance } from 'axios';

export default {
  createMockContentApiAxiosInstance: async (axiosInstance: AxiosInstance) => {
    const MockAdaptor = (await import(
      /* webpackChunkName: "axios-mock-adapter" */ 'axios-mock-adapter'
    )).default;

    const mock = new MockAdaptor(axiosInstance);

    // getThemesAndTopics
    mock
      .onGet('/Theme', {
        params: { userId: PrototypeLoginService.getUserList()[0].id },
      })
      .reply(200, DummyPublicationsData.themesAndTopics);

    return axiosInstance;
  },
};
