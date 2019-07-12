import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { AxiosInstance } from 'axios';

export default {
  createMockContentApiAxiosInstance: async (axiosInstance: AxiosInstance) => {
    const MockAdaptor = (await import(
      /* webpackChunkName: "axios-mock-adapter" */ 'axios-mock-adapter'
    )).default;
    const mock = new MockAdaptor(axiosInstance);

    mock
      .onGet('/Themes', { params: { userId: 'user1' } })
      .reply(200, DummyPublicationsData.themesAndTopics);

    return axiosInstance;
  },
};
