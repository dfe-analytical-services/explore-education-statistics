import DummyPublicationsData, {
  ThemeAndTopics,
} from '@admin/pages/DummyPublicationsData';
import { contentApi } from '@admin/services/api';

export default {
  getThemesAndTopics(userId: string): Promise<ThemeAndTopics[]> {
    return process.env.API_TYPE === 'dummy'
      ? Promise.resolve(DummyPublicationsData.themesAndTopics)
      : contentApi.get(`Themes?userId=${userId}`);
  },
};
