import { createClient } from '@admin/services/util/service';
import mocks from './mock/mock-service';
import { AdminDashboardPublication, ThemeAndTopics } from './types';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export interface DashboardService {
  getThemesAndTopics(userId: string): Promise<ThemeAndTopics[]>;
  getPublicationsByTopic(
    topicId: string,
    userId: string,
  ): Promise<AdminDashboardPublication[]>;
}

const service: DashboardService = {
  getThemesAndTopics(userId: string): Promise<ThemeAndTopics[]> {
    return apiClient.then(client =>
      client.get<ThemeAndTopics[]>('/Themes', { params: { userId } }),
    );
  },
  getPublicationsByTopic(
    topicId: string,
    userId: string,
  ): Promise<AdminDashboardPublication[]> {
    return apiClient.then(client =>
      client.get<AdminDashboardPublication[]>('/Publications', {
        params: { topicId, userId },
      }),
    );
  },
};

export default service;
