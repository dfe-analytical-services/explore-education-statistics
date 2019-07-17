import { createClient } from '@admin/services/common/service';
import mocks from './mock/axios-mock';
import { AdminDashboardPublication, ThemeAndTopics } from './types';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export default {
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
