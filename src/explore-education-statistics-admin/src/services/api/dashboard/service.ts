import {
  AdminDashboardPublication,
  ThemeAndTopics,
} from '@admin/services/api/dashboard/types';
import apis from '../index';

export default {
  getThemesAndTopics(userId: string): Promise<ThemeAndTopics[]> {
    return apis.contentApi.then(api =>
      api.get<ThemeAndTopics[]>('/Themes', { params: { userId } }),
    );
  },
  getPublicationsByTopic(
    topicId: string,
    userId: string,
  ): Promise<AdminDashboardPublication[]> {
    return apis.contentApi.then(api =>
      api.get<AdminDashboardPublication[]>('/Publications', {
        params: { topicId, userId },
      }),
    );
  },
};
