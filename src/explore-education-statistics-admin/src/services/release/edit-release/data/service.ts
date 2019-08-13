import { Polyfilla } from '@admin/services/util/polyfilla';
import client from '@admin/services/util/service';

import {
  AncillaryFile,
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
  getAncillaryFiles: (releaseId: string) => Promise<AncillaryFile[]>;
  uploadAncillaryFile: (
    releaseId: string,
    request: UploadAdhocFileRequest,
  ) => Promise<null>;
  deleteAncillaryFile: (releaseId: string, fileId: string) => Promise<null>;
  createDownloadAncillaryFileLink: (
    releaseId: string,
    fileId: string,
  ) => string;
}

interface GetFileResponse {
  extension: string;
  name: string;
  path: string;
  size: string;
  metaFileName: string;
  rows?: number;
}

const getFileNameFromPath = (path: string) =>
  path.substring(path.lastIndexOf('/') + 1);

/**
 * A temporary step to provide a row count to the front end whilst it does not yet exist in the API.
 */
const dataFilePolyfilla: Polyfilla<GetFileResponse[]> = response =>
  response.map(file => ({
    ...file,
    rows: 777777,
  }));

const service: EditReleaseService = {
  getReleaseDataFiles(releaseId: string): Promise<DataFile[]> {
    return client
      .get<GetFileResponse[]>(`/release/${releaseId}/data`)
      .then(dataFilePolyfilla)
      .then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(dataFile => {
          const associatedMetadataFile = response.find(file =>
            file.path.endsWith(`/${dataFile.metaFileName}`),
          );
          return {
            title: dataFile.name,
            file: {
              fileName: getFileNameFromPath(dataFile.path),
            },
            numberOfRows: dataFile.rows || 0,
            fileSize: {
              size: parseInt(dataFile.size.split(' ')[0], 10),
              unit: dataFile.size.split(' ')[1],
            },
            metadataFile: {
              id: associatedMetadataFile ? associatedMetadataFile.path : '',
              fileName: associatedMetadataFile
                ? getFileNameFromPath(associatedMetadataFile.path)
                : '',
            },
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
    return client.post<null>(
      `/release/${releaseId}/data?name=${request.subjectTitle}`,
      data,
    );
  },
  deleteDataFiles(releaseId: string, dataFileName: string): Promise<null> {
    return client.delete<null>(`/release/${releaseId}/data/${dataFileName}`);
  },
  createDownloadDataFileLink(releaseId: string, fileName: string): string {
    return `/release/${releaseId}/data/${fileName}`;
  },
  createDownloadDataMetadataFileLink(
    releaseId: string,
    fileName: string,
  ): string {
    return `/release/${releaseId}/data/metadata/${fileName}`;
  },
  getAncillaryFiles(releaseId: string): Promise<AncillaryFile[]> {
    return client
      .get<GetFileResponse[]>(`/release/${releaseId}/ancillary`)
      .then(response =>
        response.map(file => ({
          title: file.name,
          file: {
            id: file.path,
            fileName: getFileNameFromPath(file.path),
          },
          fileSize: {
            size: parseInt(file.size.split(' ')[0], 10),
            unit: file.size.split(' ')[1],
          },
        })),
      );
  },
  uploadAncillaryFile(
    releaseId: string,
    request: UploadAdhocFileRequest,
  ): Promise<null> {
    const data = new FormData();
    data.append('file', request.file);
    return client.post<null>(
      `/release/${releaseId}/ancillary?name=${request.name}`,
      data,
    );
  },
  deleteAncillaryFile(releaseId: string, fileName: string): Promise<null> {
    return client.delete<null>(`/release/${releaseId}/ancillary/${fileName}`);
  },
  createDownloadAncillaryFileLink(releaseId: string, fileName: string): string {
    return `/release/${releaseId}/ancillary/${fileName}`;
  },
};

export default service;
