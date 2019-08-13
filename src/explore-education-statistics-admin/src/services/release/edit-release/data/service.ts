import client from '@admin/services/util/service';

import {
  AdhocFile,
  DataFile,
  UploadAdhocFileRequest,
  UploadDataFilesRequest,
} from './types';

export interface EditReleaseService {
  getReleaseDataFiles: (releaseId: string) => Promise<DataFile[]>;
  uploadDataFiles: (
    releaseId: string,
    request: UploadDataFilesRequest,
  ) => Promise<null>;
  deleteDataFiles: (releaseId: string, dataFileId: string) => Promise<null>;
  createDownloadDataFileLink: (releaseId: string, fileId: string) => string;
  createDownloadDataMetadataFileLink: (
    releaseId: string,
    fileId: string,
  ) => string;
  getReleaseAdhocFiles: (releaseId: string) => Promise<AdhocFile[]>;
  uploadAdhocFile: (
    releaseId: string,
    request: UploadAdhocFileRequest,
  ) => Promise<null>;
  deleteAdhocFile: (releaseId: string, fileId: string) => Promise<null>;
  createDownloadAdhocFileLink: (releaseId: string, fileId: string) => string;
}

const service: EditReleaseService = {
  getReleaseDataFiles(releaseId: string): Promise<DataFile[]> {
    return client.get<DataFile[]>(`/release/${releaseId}/datafiles`);
  },
  uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<null> {
    const data = new FormData();
    data.append('subjectTitle', request.subjectTitle);
    data.append('dataFile', request.dataFile);
    data.append('metadataFile', request.metadataFile);
    return client.post<null>(`/release/${releaseId}/datafiles/upload`, data);
  },
  deleteDataFiles(releaseId: string, dataFileId: string): Promise<null> {
    return client.delete<null>(`/release/${releaseId}/datafiles/${dataFileId}`);
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
  getReleaseAdhocFiles(releaseId: string): Promise<AdhocFile[]> {
    return client.get<AdhocFile[]>(`/release/${releaseId}/adhoc-files`);
  },
  uploadAdhocFile(
    releaseId: string,
    request: UploadAdhocFileRequest,
  ): Promise<null> {
    const data = new FormData();
    data.append('name', request.name);
    data.append('file', request.file);
    return client.post<null>(`/release/${releaseId}/adhoc-files/upload`, data);
  },
  deleteAdhocFile(releaseId: string, fileId: string): Promise<null> {
    return client.delete<null>(`/release/${releaseId}/adhoc-files/${fileId}`);
  },
  createDownloadAdhocFileLink(releaseId: string, fileId: string): string {
    return `/release/${releaseId}/adhoc-file/${fileId}`;
  },
};

export default service;
