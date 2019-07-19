import { DashboardService } from '@admin/services/dashboard/service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: DashboardService = {
    getMyThemesAndTopics: () => Promise.resolve(mockData.themesAndTopics),
    getMyPublicationsByTopic: _ =>
      Promise.resolve(mockData.dashboardPublications),
  };

  // getMyThemesAndTopics
  mock.onGet('/me/themes').reply(200, service.getMyThemesAndTopics());

  // getMyPublicationsByTopic
  mock
    .onGet('/me/publications', {
      params: {
        topicId: '67c249de-1cca-446e-8ccb-dcdac542f460',
      },
    })
    .reply(200, service.getMyPublicationsByTopic(''));
};
