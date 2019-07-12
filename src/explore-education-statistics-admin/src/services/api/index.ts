import { ThemeAndTopics } from '@admin/pages/DummyPublicationsData';
import Client from '@admin/services/api/Client';
import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import mocks from './mock/axios-mock';

const createContentApiAxios = async () => {
  const baseUrl = process.env.CONTENT_API_BASE_URL;

  const axiosInstance = axios.create({
    baseURL: `${baseUrl}/api/`,
    paramsSerializer: commaSeparated,
  });

  return process.env.USE_MOCK_API === 'true'
    ? mocks.createMockContentApiAxiosInstance(axiosInstance)
    : axiosInstance;
};

export default {
  contentApi: createContentApiAxios().then(axiosInstance => {
    const client = new Client(axiosInstance);
    return {
      getThemesAndTopics: (userId: string) =>
        client.get<ThemeAndTopics[]>('/Themes', { params: { userId } }),
    };
  }),
};
