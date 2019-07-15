import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';
import mocks from './mock/axios-mock';
import { AdminDashboardPublication, ThemeAndTopics } from './types';

const createClient = async () => {
  const baseUrl = process.env.CONTENT_API_BASE_URL;

  const axiosInstance = axios.create({
    baseURL: `${baseUrl}/api/`,
    paramsSerializer: commaSeparated,
  });

  const decoratedAxios =
    process.env.USE_MOCK_API === 'true'
      ? mocks.createMockContentApiAxiosInstance(axiosInstance)
      : Promise.resolve(axiosInstance);

  return decoratedAxios.then(decorated => new Client(decorated));
};

const apiClient = createClient();

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
