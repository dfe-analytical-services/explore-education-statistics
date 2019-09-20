import client, { baseURL } from '@admin/services/util/service';

import {
  AncillaryFile,
  DataFile,
  ChartFile,
  UploadAncillaryFileRequest,
  UploadDataFilesRequest,
  UploadChartFileRequest,
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
    request: UploadAncillaryFileRequest,
  ) => Promise<null>;
  deleteAncillaryFile: (releaseId: string, fileId: string) => Promise<null>;
  createDownloadAncillaryFileLink: (
    releaseId: string,
    fileId: string,
  ) => string;

  getChartFiles: (releaseId: string) => Promise<ChartFile[]>;
  uploadChartFile: (
    releaseId: string,
    request: UploadChartFileRequest,
  ) => Promise<null>;

  createDownloadChartFileLink: (releaseId: string, fileName: string) => string;
}

interface GetFileResponse {
  extension: string;
  name: string;
  path: string;
  size: string;
  metaFileName: string;
  rows: number;
}

const getFileNameFromPath = (path: string) =>
  path.substring(path.lastIndexOf('/') + 1);

/**
 * A temporary step to provide a row count to the front end whilst it does not yet exist in the API.
 */

const service: EditReleaseService = {
  getReleaseDataFiles(releaseId: string): Promise<DataFile[]> {
    return client
      .get<GetFileResponse[]>(`/release/${releaseId}/data`)
      .then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(dataFile => {
          const associatedMetadataFile = response.find(file =>
            file.path.endsWith(`/${dataFile.metaFileName}`),
          );
          return {
            title: dataFile.name,
            filename: getFileNameFromPath(dataFile.path),
            rows: dataFile.rows || 0,
            fileSize: {
              size: parseInt(dataFile.size.split(' ')[0], 10),
              unit: dataFile.size.split(' ')[1],
            },
            metadataFilename: associatedMetadataFile
              ? getFileNameFromPath(associatedMetadataFile.path)
              : '',
            canDelete: true,
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
    return `${baseURL}release/${releaseId}/data/${fileName}`;
  },
  createDownloadDataMetadataFileLink(
    releaseId: string,
    fileName: string,
  ): string {
    return `${baseURL}release/${releaseId}/data/${fileName}`;
  },
  getAncillaryFiles(releaseId: string): Promise<AncillaryFile[]> {
    return client
      .get<GetFileResponse[]>(`/release/${releaseId}/ancillary`)
      .then(response =>
        response.map(file => ({
          title: file.name,
          filename: getFileNameFromPath(file.path),
          fileSize: {
            size: parseInt(file.size.split(' ')[0], 10),
            unit: file.size.split(' ')[1],
          },
        })),
      );
  },
  uploadAncillaryFile(
    releaseId: string,
    request: UploadAncillaryFileRequest,
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
    return `${baseURL}release/${releaseId}/ancillary/${fileName}`;
  },

  async getChartFiles(releaseId: string) {
    const response = await client.get<GetFileResponse[]>(
      `/release/${releaseId}/chart`,
    );

    return response.map<ChartFile>(({ name, path, size }) => ({
      title: name,
      filename: getFileNameFromPath(path),
      fileSize: {
        size: parseInt(size.split(' ')[0], 10),
        unit: size.split(' ')[1],
      },
    }));
  },

  async uploadChartFile(releaseId: string, request: UploadChartFileRequest) {
    const data = new FormData();
    data.append('file', request.file);
    return client.post<null>(
      `/release/${releaseId}/chart?name=${request.name}`,
      data,
    );
  },

  createDownloadChartFileLink(releaseId: string, fileName: string): string {
    return `/api/release/${releaseId}/chart/${fileName}`;
  },
};

export default service;
