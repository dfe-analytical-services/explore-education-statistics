import { AxiosInstance } from 'axios';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';
import MockAdapter from "axios-mock-adapter";

export default async (mock: MockAdapter) => {

  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  // getThemesAndTopics
  mock
    .onGet('/Themes', {
      params: { userId: PrototypeLoginService.getUserList()[0].id },
    })
    .reply(200, mockData.themesAndTopics);

  // getPublicationsByTopic
  mock
    .onGet('/Publications', {
      params: {
        topicId: '67c249de-1cca-446e-8ccb-dcdac542f460',
        userId: PrototypeLoginService.getUserList()[0].id,
      },
    })
    .reply(200, mockData.dashboardPublications);
};
