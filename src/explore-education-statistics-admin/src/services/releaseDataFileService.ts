import { DeleteDataBlockPlan } from '@admin/services/dataBlockService';
import { FileInfo } from '@admin/services/types/file';
import client from '@admin/services/utils/service';
import { Overwrite } from '@common/types';
import downloadFile from './utils/file/downloadFile';
import getFileNameFromPath from './utils/file/getFileNameFromPath';

interface DataFileInfo extends FileInfo {
  metaFileId: string;
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
  id: string;
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
  rows: number;
  metaFileId: string;
  metadataFilename: string;
  userName: string;
  created?: string;
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
  | 'PROCESSING_ARCHIVE_FILE'
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
    id: file.id,
    title: file.name,
    filename: file.fileName,
    rows: file.rows || 0,
    fileSize: {
      size: parseInt(size, 10),
      unit,
    },
    metaFileId: file.metaFileId,
    metadataFilename: file.metaFileName,
    canDelete: true,
    userName: file.userName,
    created: file.created,
  };
}

const releaseDataFileService = {
  getDataFiles(releaseId: string): Promise<DataFile[]> {
    return client
      .get<DataFileInfo[]>(`/release/${releaseId}/data`)
      .then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(mapFile);
      });
  },
  getDataFile(releaseId: string, fileId: string): Promise<DataFile> {
    return client
      .get<DataFileInfo>(`/release/${releaseId}/data/${fileId}`)
      .then(mapFile);
  },
  async uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<DataFile> {
    const data = new FormData();
    data.append('file', request.dataFile);
    data.append('metaFile', request.metadataFile);

    const file = await client.post<DataFileInfo>(
      `/release/${releaseId}/data`,
      data,
      {
        params: {
          name: request.name,
        },
      },
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
      `/release/${releaseId}/zip-data`,
      data,
      {
        params: {
          name: request.name,
        },
      },
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
      `/release/${releaseId}/data/${dataFile.id}/delete-plan`,
    );
  },
  deleteDataFiles(releaseId: string, fileId: string): Promise<void> {
    return client.delete<void>(`/release/${releaseId}/data/${fileId}`);
  },
  downloadFile(releaseId: string, id: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/file/${id}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
};

export default releaseDataFileService;
