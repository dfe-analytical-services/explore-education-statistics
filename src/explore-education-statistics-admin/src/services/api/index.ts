import Client from '@common/services/api/Client';
import axios from 'axios';
import { commaSeparated } from '@common/services/util/paramSerializers';
import mocks from './dashboard/mock/axios-mock';

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
    return new Client(axiosInstance);
  }),
};
