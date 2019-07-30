import { DashboardService } from '@admin/services/dashboard/service';
import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  const service: DashboardService = {
    getMyThemesAndTopics: () => Promise.resolve(mockData.themesAndTopics),
    getMyPublicationsByTopic: topicId =>
      Promise.resolve(mockData.dashboardPublicationsByTopicId[topicId] || []),
  };

  // getMyThemesAndTopics
  mock.onGet('/me/themes').reply(200, service.getMyThemesAndTopics());

  // getMyPublicationsByTopic
  mock.onGet('/me/publications').reply(config => {
    const { topicId } = config.params;
    return [200, service.getMyPublicationsByTopic(topicId)];
  });
};
