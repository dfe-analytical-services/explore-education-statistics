import { DeleteDataBlockPlan } from '@admin/services/dataBlockService';
import { DataFilePermissions } from '@admin/services/permissionService';
import client from '@admin/services/utils/service';
import { FileInfo } from '@common/services/types/file';
import downloadFile from '@common/utils/file/downloadFile';

interface DataFileInfo extends FileInfo {
  type: 'Data';
  metaFileId: string;
  metaFileName: string;
  rows?: number;
  userName: string;
  created: string;
  status: ImportStatusCode;
  replacedBy?: string;
  permissions: DataFilePermissions;
}

export interface DeleteDataFilePlan {
  deleteDataBlockPlan: DeleteDataBlockPlan;
  footnoteIds: string[];
}

export interface DataFile {
  id: string;
  title: string;
  fileName: string;
  fileSize: {
    size: number;
    unit: string;
  };
  rows?: number;
  metaFileId: string;
  metaFileName: string;
  userName: string;
  status: ImportStatusCode;
  replacedBy?: string;
  created?: string;
  isDeleting?: boolean;
  isCancelling?: boolean;
  permissions: DataFilePermissions;
}

export type UploadDataFilesRequest =
  | {
      title: string;
      dataFile: File;
      metadataFile: File;
    }
  | {
      replacingFileId: string;
      dataFile: File;
      metadataFile: File;
    };

export type UploadZipDataFileRequest =
  | {
      title: string;
      zipFile: File;
    }
  | {
      replacingFileId: string;
      zipFile: File;
    };

export interface DataFileUpdateRequest {
  title: string;
}

export type ImportStatusCode =
  | 'COMPLETE'
  | 'QUEUED'
  | 'UPLOADING'
  | 'PROCESSING_ARCHIVE_FILE'
  | 'STAGE_1'
  | 'STAGE_2'
  | 'STAGE_3'
  | 'NOT_FOUND'
  | 'FAILED'
  | 'CANCELLING'
  | 'CANCELLED';

export interface DataFileImportStatus {
  status: ImportStatusCode;
  percentageComplete: number;
  stagePercentageComplete: number;
  errors?: string[];
  totalRows: number;
}

function mapFile({ name, ...file }: DataFileInfo): DataFile {
  const [size, unit] = file.size.split(' ');

  return {
    ...file,
    title: name,
    fileSize: {
      size: parseInt(size, 10),
      unit,
    },
  };
}

const releaseDataFileService = {
  getDataFiles(releaseVersionId: string): Promise<DataFile[]> {
    return client
      .get<DataFileInfo[]>(`/release/${releaseVersionId}/data`)
      .then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(mapFile);
      });
  },
  getDataFile(releaseVersionId: string, fileId: string): Promise<DataFile> {
    return client
      .get<DataFileInfo>(`/release/${releaseVersionId}/data/${fileId}`)
      .then(mapFile);
  },
  async uploadDataFiles(
    releaseVersionId: string,
    request: UploadDataFilesRequest,
  ): Promise<DataFile> {
    const { dataFile, metadataFile, ...params } = request;

    const data = new FormData();
    data.append('file', dataFile);
    data.append('metaFile', metadataFile);

    const file = await client.post<DataFileInfo>(
      `/release/${releaseVersionId}/data`,
      data,
      {
        params,
      },
    );

    return mapFile(file);
  },
  async uploadZipDataFile(
    releaseVersionId: string,
    request: UploadZipDataFileRequest,
  ): Promise<DataFile> {
    const { zipFile, ...params } = request;

    const data = new FormData();
    data.append('zipFile', zipFile);

    const file = await client.post<DataFileInfo>(
      `/release/${releaseVersionId}/zip-data`,
      data,
      {
        params,
      },
    );

    return mapFile(file);
  },
  async uploadBulkZipDataFile(
    releaseId: string,
    zipFile: File,
  ): Promise<DataFile[]> {
    const data = new FormData();
    data.append('zipFile', zipFile);

    const files = await client.post<DataFileInfo[]>(
      `/release/${releaseId}/bulk-zip-data`,
      data,
    );

    return files.map(file => mapFile(file));
  },
  updateDataFilesOrder(
    releaseVersionId: string,
    order: string[],
  ): Promise<DataFile[]> {
    return client
      .put<DataFileInfo[]>(`/release/${releaseVersionId}/data/order`, order)
      .then(response => {
        const dataFiles = response.filter(file => file.metaFileName.length > 0);
        return dataFiles.map(mapFile);
      });
  },
  getDataFileImportStatus(
    releaseVersionId: string,
    dataFile: DataFile,
  ): Promise<DataFileImportStatus> {
    return client.get<DataFileImportStatus>(
      `/release/${releaseVersionId}/data/${dataFile.id}/import/status`,
    );
  },

  getDeleteDataFilePlan(
    releaseVersionId: string,
    dataFile: DataFile,
  ): Promise<DeleteDataFilePlan> {
    return client.get<DeleteDataFilePlan>(
      `/release/${releaseVersionId}/data/${dataFile.id}/delete-plan`,
    );
  },
  deleteDataFiles(releaseVersionId: string, fileId: string): Promise<void> {
    return client.delete<void>(`/release/${releaseVersionId}/data/${fileId}`);
  },
  downloadFile(
    releaseVersionId: string,
    id: string,
    fileName: string,
  ): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseVersionId}/file/${id}/download`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
  updateFile(
    releaseVersionId: string,
    fileId: string,
    data: DataFileUpdateRequest,
  ): Promise<void> {
    return client.patch(`/release/${releaseVersionId}/data/${fileId}`, data);
  },
  cancelImport(releaseVersionId: string, fileId: string): Promise<void> {
    return client.post(
      `/release/${releaseVersionId}/data/${fileId}/import/cancel`,
    );
  },
};

export default releaseDataFileService;
