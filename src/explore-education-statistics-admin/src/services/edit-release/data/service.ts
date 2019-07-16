import { createClient } from '@admin/services/common/service';
import { DataFileView } from './types';
import mocks from './mock/axios-mock';

const apiClient = createClient({
  mockBehaviourRegistrar: mocks,
});

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
