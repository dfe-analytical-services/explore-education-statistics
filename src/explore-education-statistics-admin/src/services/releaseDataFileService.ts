import { DeleteDataBlockPlan } from '@admin/services/dataBlockService';
import client from '@admin/services/utils/service';
import getFileNameFromPath from './utils/file/getFileNameFromPath';
import downloadFile from './utils/file/downloadFile';

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

export interface DeleteDataFilePlan {
  deleteDataBlockPlan: DeleteDataBlockPlan;
  footnoteIds: string[];
}

export interface DataFile {
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
  rows: number;
  metadataFilename: string;
  userName: string;
  created: Date;
  canDelete?: boolean;
  isDeleting?: boolean;
}

interface UploadDataFilesRequest {
  subjectTitle: string;
  dataFile: File;
  metadataFile: File;
}

const releaseDataFileService = {
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
  getDeleteDataFilePlan(
    releaseId: string,
    dataFile: DataFile,
  ): Promise<DeleteDataFilePlan> {
    return client.get<DeleteDataFilePlan>(
      `/release/${releaseId}/data/${dataFile.filename}/delete-plan?name=${dataFile.title}`,
    );
  },
  deleteDataFiles(releaseId: string, dataFile: DataFile): Promise<null> {
    return client.delete<null>(
      `/release/${releaseId}/data/${dataFile.filename}?name=${dataFile.title}`,
    );
  },
  downloadDataFile(releaseId: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/data/${fileName}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
  downloadFile(path: string): Promise<void> {
    return client
      .get<Blob>(`/release/${path}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, getFileNameFromPath(path)));
  },
};

export default releaseDataFileService;
