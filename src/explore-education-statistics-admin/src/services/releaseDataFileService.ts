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
  publicApiDataSetId?: string;
  publicApiDataSetVersion?: string;
}

export interface DataSetAccoutrements {
  dataBlocks: { id: string; name: string }[];
  footnotes: { id: string; content: string }[];
}

export type UploadDataFilesRequest = {
  title: string;
  dataFile: File;
  metadataFile: File;
  replacingFileId?: string;
};

export type UploadZipDataFileRequest = {
  title: string;
  zipFile: File;
  replacingFileId?: string;
};

export type DataSetUploadResult = {
  id: string;
  dataSetTitle: string;
  dataFileName: string;
  metaFileName: string;
  status: string;
  screenerResult: object | undefined;
  replacingFileId?: string;
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
  async getDataFiles(releaseId: string): Promise<DataFile[]> {
    const response = await client.get<DataFileInfo[]>(
      `/releaseVersions/${releaseId}/data`,
    );
    const dataFiles = response.filter(file => file.metaFileName.length > 0);
    return dataFiles.map(mapFile);
  },
  async getDataFile(releaseId: string, fileId: string): Promise<DataFile> {
    const result = await client.get<DataFileInfo>(
      `/releaseVersions/${releaseId}/data/${fileId}`,
    );
    return mapFile(result);
  },
  getDataSetAccoutrementsSummary(
    releaseVersionId: string,
    fileId: string,
  ): Promise<DataSetAccoutrements> {
    return client.get<DataSetAccoutrements>(
      `/release/${releaseVersionId}/data/${fileId}/accoutrements-summary`,
    );
  },
  async uploadDataSetFilePairForReplacement(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<DataFile> {
    const { dataFile, metadataFile, title, replacingFileId } = request;

    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('title', title);
    data.append('dataFile', dataFile);
    data.append('metaFile', metadataFile);
    data.append('replacingFileId', replacingFileId ?? '');

    const file = await client.post<DataFileInfo>(
      '/releaseVersions/replacement-data',
      data,
    );

    return mapFile(file);
  },
  async uploadZippedDataSetFilePairForReplacement(
    releaseId: string,
    request: UploadZipDataFileRequest,
  ): Promise<DataFile> {
    const { zipFile, title, replacingFileId } = request;

    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('title', title);
    data.append('zipFile', zipFile);
    data.append('replacingFileId', replacingFileId ?? '');

    const file = await client.post<DataFileInfo>(
      '/releaseVersions/replacement-zip-data',
      data,
    );

    return mapFile(file);
  },
  async uploadDataSetFilePair(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<DataSetUploadResult[]> {
    const { dataFile, metadataFile, title, replacingFileId } = request;

    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('title', title);
    data.append('dataFile', dataFile);
    data.append('metaFile', metadataFile);
    data.append('replacingFileId', replacingFileId ?? '');

    return client.post<DataSetUploadResult[]>('/releaseVersions/data', data);
  },
  async uploadZippedDataSetFilePair(
    releaseId: string,
    request: UploadZipDataFileRequest,
  ): Promise<DataSetUploadResult[]> {
    const { zipFile, title, replacingFileId } = request;

    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('title', title);
    data.append('zipFile', zipFile);
    data.append('replacingFileId', replacingFileId ?? '');

    return client.post<DataSetUploadResult[]>(
      '/releaseVersions/zip-data',
      data,
    );
  },
  async uploadBulkZipDataSetFile(
    releaseId: string,
    zipFile: File,
  ): Promise<DataSetUploadResult[]> {
    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('zipFile', zipFile);

    return client.post<DataSetUploadResult[]>(
      'releaseVersions/upload-bulk-zip-data',
      data,
    );
  },
  async importDataSets(
    releaseId: string,
    dataSetUploadIds: string[],
  ): Promise<DataFile[]> {
    const files = await client.post<DataFileInfo[]>(
      `/releaseVersions/${releaseId}/import-data-sets`,
      dataSetUploadIds,
    );

    return files.map(file => mapFile(file));
  },
  async updateDataFilesOrder(
    releaseId: string,
    order: string[],
  ): Promise<DataFile[]> {
    const response = await client.put<DataFileInfo[]>(
      `/release/${releaseId}/data/order`,
      order,
    );
    const dataFiles = response.filter(file => file.metaFileName.length > 0);
    return dataFiles.map(mapFile);
  },
  getDataFileImportStatus(
    releaseId: string,
    dataFile: DataFile,
  ): Promise<DataFileImportStatus> {
    return client.get<DataFileImportStatus>(
      `/release/${releaseId}/data/${dataFile.id}/import/status`,
    );
  },

  getDeleteDataFilePlan(
    releaseId: string,
    dataFileId: string,
  ): Promise<DeleteDataFilePlan> {
    return client.get<DeleteDataFilePlan>(
      `/release/${releaseId}/data/${dataFileId}/delete-plan`,
    );
  },
  deleteDataFiles(releaseId: string, fileId: string): Promise<void> {
    return client.delete<void>(`/release/${releaseId}/data/${fileId}`);
  },
  async downloadFile(
    releaseId: string,
    id: string,
    fileName: string,
  ): Promise<void> {
    const response = await client.get<Blob>(
      `/release/${releaseId}/file/${id}/download`,
      {
        responseType: 'blob',
      })
      .then(response => downloadFile({ file: response, fileName }));
  },
  updateFile(
    releaseId: string,
    fileId: string,
    data: DataFileUpdateRequest,
  ): Promise<void> {
    return client.patch(`/release/${releaseId}/data/${fileId}`, data);
  },
  cancelImport(releaseId: string, fileId: string): Promise<void> {
    return client.post(`/release/${releaseId}/data/${fileId}/import/cancel`);
  },
};

export default releaseDataFileService;
