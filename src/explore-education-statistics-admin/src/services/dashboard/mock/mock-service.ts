import {DashboardService} from "@admin/services/dashboard/service";
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: DashboardService = {
    getThemesAndTopics: _ => Promise.resolve(mockData.themesAndTopics),
    getPublicationsByTopic: _ => Promise.resolve(mockData.dashboardPublications),
  };

  // getThemesAndTopics
  mock
    .onGet('/Themes', {
      params: { userId: PrototypeLoginService.getUserList()[0].id },
    })
    .reply(200, service.getThemesAndTopics(''));

  // getPublicationsByTopic
  mock
    .onGet('/Publications', {
      params: {
        topicId: '67c249de-1cca-446e-8ccb-dcdac542f460',
        userId: PrototypeLoginService.getUserList()[0].id,
      },
    })
    .reply(200, service.getPublicationsByTopic('', ''  ));
};
