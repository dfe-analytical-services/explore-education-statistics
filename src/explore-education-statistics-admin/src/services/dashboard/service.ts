import publicationPollyfilla from '@admin/services/dashboard/polyfillas';
import client from '@admin/services/util/service';

import { AdminDashboardPublication, ThemeAndTopics } from './types';

export interface DashboardService {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]>;
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]>;
}

const service: DashboardService = {
  getMyThemesAndTopics(): Promise<ThemeAndTopics[]> {
    return client.get<ThemeAndTopics[]>('/me/themes');
  },
  getMyPublicationsByTopic(
    topicId: string,
  ): Promise<AdminDashboardPublication[]> {
    return client
      .get<AdminDashboardPublication[]>('/me/publications', {
        params: { topicId },
      })
      .then(publications => publications.map(publicationPollyfilla));
  },
};

export default service;
