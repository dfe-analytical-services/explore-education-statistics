import {
  AdminDashboardPublication,
  ThemeAndTopics,
} from '@admin/services/api/dashboard/types';
import apis from '../index';

export default {
  getThemesAndTopics(userId: string): Promise<ThemeAndTopics[]> {
    return apis.contentApi.then(api =>
      api.get<ThemeAndTopics[]>('/Theme', { params: { userId } }),
    );
  },
  getPublicationsForAdminDashboard(
    userId: string,
  ): Promise<AdminDashboardPublication[]> {
    return apis.contentApi.then(api =>
      api.get<AdminDashboardPublication[]>('/Publication', {
        params: { userId },
      }),
    );
  },
};
