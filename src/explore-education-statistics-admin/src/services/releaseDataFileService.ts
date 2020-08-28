import { DeleteDataBlockPlan } from '@admin/services/dataBlockService';
import { FileInfo } from '@admin/services/types/file';
import client from '@admin/services/utils/service';
import { Overwrite } from '@common/types';
import downloadFile from './utils/file/downloadFile';
import getFileNameFromPath from './utils/file/getFileNameFromPath';

interface DataFileInfo extends FileInfo {
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

export interface UploadDataFilesRequest {
  name: string;
  dataFile: File;
  metadataFile: File;
}

export interface UploadZipDataFileRequest {
  name: string;
  zipFile: File;
}

export type ImportStatusCode =
  | 'COMPLETE'
  | 'QUEUED'
  | 'UPLOADING'
  | 'RUNNING_PHASE_1'
  | 'RUNNING_PHASE_2'
  | 'RUNNING_PHASE_3'
  | 'NOT_FOUND'
  | 'FAILED';

export interface Errors {
  Message: string;
}

export interface DataFileImportStatus {
  status: ImportStatusCode;
  percentageComplete?: string;
  errors?: Errors[];
  numberOfRows: number;
}

function mapFile(file: DataFileInfo): DataFile {
  const [size, unit] = file.size.split(' ');

  return {
    title: file.name,
    filename: file.fileName,
    rows: file.rows || 0,
    fileSize: {
      size: parseInt(size, 10),
      unit,
    },
    metadataFilename: file.metaFileName,
    canDelete: true,
    userName: file.userName,
    created: new Date(file.created),
  };
}

const releaseDataFileService = {
  getReleaseDataFiles(releaseId: string): Promise<DataFile[]> {
    return client
      .get<DataFileInfo[]>(`/release/${releaseId}/data`)
      .then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(mapFile);
      });
  },
  async uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<DataFile> {
    const data = new FormData();
    data.append('file', request.dataFile);
    data.append('metaFile', request.metadataFile);

    const file = await client.post<DataFileInfo>(
      `/release/${releaseId}/data?name=${request.name}`,
      data,
    );

    return mapFile(file);
  },
  async uploadZipDataFile(
    releaseId: string,
    request: UploadZipDataFileRequest,
  ): Promise<DataFile> {
    const data = new FormData();
    data.append('zipFile', request.zipFile);

    const file = await client.post<DataFileInfo>(
      `/release/${releaseId}/zip-data?name=${request.name}`,
      data,
    );

    return mapFile(file);
  },
  getDataFileImportStatus(
    releaseId: string,
    dataFileName: string,
  ): Promise<DataFileImportStatus> {
    return client
      .get<
        Overwrite<
          DataFileImportStatus,
          {
            errors?: string;
          }
        >
      >(`/release/${releaseId}/data/${dataFileName}/import/status`)
      .then(importStatus => {
        return {
          ...importStatus,
          errors: JSON.parse(importStatus.errors || '[]').map(
            ({ Message }: Errors) => Message,
          ),
        };
      });
  },

  getDeleteDataFilePlan(
    releaseId: string,
    dataFile: DataFile,
  ): Promise<DeleteDataFilePlan> {
    return client.get<DeleteDataFilePlan>(
      `/release/${releaseId}/data/${dataFile.filename}/delete-plan?name=${dataFile.title}`,
    );
  },
  deleteDataFiles(releaseId: string, dataFile: DataFile): Promise<void> {
    return client.delete<void>(
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
