import {ReleaseSetupDetails, ReleaseSetupDetailsUpdateRequest} from "@admin/services/edit-release/setup/types";
import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';
import mocks from './mock/axios-mock';

const createClient = async () => {
  const baseUrl = process.env.CONTENT_API_BASE_URL;

  const axiosInstance = axios.create({
    baseURL: `${baseUrl}/api/`,
    paramsSerializer: commaSeparated,
  });

  axios.interceptors.request.use(request => {
    // eslint-disable-next-line no-console
    console.log('Starting Request', request);
    return request;
  });

  const decoratedAxios =
    process.env.USE_MOCK_API === 'true'
      ? mocks.createMockContentApiAxiosInstance(axiosInstance)
      : Promise.resolve(axiosInstance);

  return decoratedAxios.then(decorated => new Client(decorated));
};

const apiClient = createClient();

export default {
  getReleaseSetupDetails(releaseId: string): Promise<ReleaseSetupDetails> {
    return apiClient.then(client =>
      client.get<ReleaseSetupDetails>(`/release/${releaseId}/setup`),
    );
  },
  updateReleaseSetupDetails(updatedRelease: ReleaseSetupDetailsUpdateRequest): Promise<void> {
    return apiClient.then(client =>
      client.post(`/release/${updatedRelease.id}/setup`, updatedRelease),
    );
  }
};
