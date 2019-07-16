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

  const decoratedAxios =
    process.env.USE_MOCK_API === 'true'
      ? mocks.createMockContentApiAxiosInstance(axiosInstance)
      : Promise.resolve(axiosInstance);

  return decoratedAxios.then(decorated => new Client(decorated));
};

const apiClient = createClient();

export interface UploadDataFilesRequest {
  subjectTitle: string,
  dataFile: File,
  metadataFile: File,
}

export default {
  getReleaseDataFiles(releaseId: string): Promise<DataFileView> {
    return apiClient.then(client =>
      client.get<DataFileView>(`/release/${releaseId}/datafiles`),
    );
  },
  uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest): Promise<null> {

    return apiClient.then(client => {
      const data = new FormData();
      data.append('subjectTitle', request.subjectTitle);
      data.append('dataFile', request.dataFile);
      data.append('metadataFile', request.metadataFile);
      return client.post<null>(`/release/${releaseId}/datafiles/upload`, data);
    });
  },
  deleteDataFiles(releaseId: string, dataFileId: string): Promise<null> {
    return apiClient.then(client =>
      client.delete<null>(`/release/${releaseId}/datafiles/${dataFileId}`),
    );
  },
  createDownloadDataFileLink(releaseId: string, fileId: string): string {
    return `/release/${releaseId}/datafile/${fileId}`;
  },
  createDownloadDataMetadataFileLink(releaseId: string, fileId: string): string {
    return `/release/${releaseId}/datafile/metadata/${fileId}`;
  },
};
