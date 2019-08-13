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

type ListDataFilesResponse = [{
  extension: string;
  name: string;
  path: string;
  size: string;
  metaFileName: string;
}];

const getFileNameFromPath = (path: string) =>
  path.substring(path.lastIndexOf('/') + 1);

const service: EditReleaseService = {
  getReleaseDataFiles(releaseId: string): Promise<DataFile[]> {
    return client.
      get<ListDataFilesResponse>(`/release/${releaseId}/data`).
      then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(dataFile => {
          const associatedMetadataFile = response.find(file => file.path.endsWith(`/${dataFile.metaFileName}`));
          return {
            title: dataFile.name,
            file: {
              id: dataFile.path,
              fileName: getFileNameFromPath(dataFile.path),
            },
            numberOfRows: 777777,
            fileSize: {
              size: parseInt(dataFile.size.split(' ')[0], 10),
              unit: dataFile.size.split(' ')[1],
            },
            metadataFile: {
              id: associatedMetadataFile ? associatedMetadataFile.path : '',
              fileName: associatedMetadataFile ? getFileNameFromPath(associatedMetadataFile.path) : '',
            }
          };
        });
      });
  },
  uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<null> {
    const data = new FormData();
    data.append('file', request.dataFile);
    data.append('metaFile', request.metadataFile);
    return client.post<null>(`/release/${releaseId}/data?name=${request.subjectTitle}`, data);
  },
  deleteDataFiles(releaseId: string, dataFileId: string): Promise<null> {
    return client.delete<null>(`/release/${releaseId}/data/${dataFileId}`);
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
