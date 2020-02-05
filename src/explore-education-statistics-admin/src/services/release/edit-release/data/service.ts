import client from '@admin/services/util/service';

import {
  AncillaryFile,
  ChartFile,
  DataFile,
  UploadAncillaryFileRequest,
  UploadChartFileRequest,
  UploadDataFilesRequest,
} from './types';

interface GetFileResponse {
  extension: string;
  name: string;
  path: string;
  size: string;
  metaFileName: string;
  rows: number;
  userName: string;
  created: string;
}

const getFileNameFromPath = (path: string) =>
  path.substring(path.lastIndexOf('/') + 1);

const downloadFile = (blob: Blob, fileName: string) => {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', fileName);
  document.body.appendChild(link);
  link.click();
  link.remove();
};

/**
 * A temporary step to provide a row count to the front end whilst it does not yet exist in the API.
 */

const service = {
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
            userName: dataFile.userName,
            created: new Date(dataFile.created),
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
  deleteDataFiles(releaseId: string, dataFile: DataFile): Promise<null> {
    return client.delete<null>(
      `/release/${releaseId}/data/${dataFile.filename}/${dataFile.title}`,
    );
  },
  downloadDataFile(releaseId: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/data/${fileName}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
  downloadDataMetadataFile(releaseId: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/data/${fileName}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
  downloadFile(path: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${path}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
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
  downloadAncillaryFile(releaseId: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/ancillary/${fileName}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
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
  downloadChartFile(releaseId: string, fileName: string): Promise<Blob> {
    return client.get<Blob>(`/release/${releaseId}/chart/${fileName}`, {
      responseType: 'blob',
    });
  },
};

export default service;
