import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';
import mocks from './mock/axios-mock';
import { DataFileView } from './types';

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
  getReleaseDataFiles(releaseId: string): Promise<DataFileView> {
    return apiClient.then(client =>
      client.get<DataFileView>(`/release/${releaseId}/datafiles`),
    );
  },
  createDownloadDataFileLink(releaseId: string, fileId: string): string {
    return `/release/${releaseId}/datafile/${fileId}`;
  },
  createDownloadDataMetadataFileLink(releaseId: string, fileId: string): string {
    return `/release/${releaseId}/datafile/metadata/${fileId}`;
  },
};
