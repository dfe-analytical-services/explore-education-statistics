import {createClient} from '@admin/services/util/service';
import mocks from './mock/axios-mock';
import {DataFile, UploadDataFilesRequest} from './types';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

export default {
  getReleaseDataFiles(releaseId: string): Promise<DataFile[]> {
    return apiClient.then(client =>
      client.get<DataFile[]>(`/release/${releaseId}/datafiles`),
    );
  },
  uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<null> {
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
  createDownloadDataMetadataFileLink(
    releaseId: string,
    fileId: string,
  ): string {
    return `/release/${releaseId}/datafile/metadata/${fileId}`;
  },
};
