import { ThemeAndTopics } from '@admin/pages/DummyPublicationsData';
import apis from '@admin/services/api';

export default {
  getThemesAndTopics(userId: string): Promise<ThemeAndTopics[]> {
    return apis.contentApi.then(api => api.getThemesAndTopics(userId));
  },
};
