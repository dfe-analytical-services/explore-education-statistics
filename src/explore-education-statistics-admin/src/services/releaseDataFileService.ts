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
  replacedBy?: string; // the fileId of the replacement file, if it exists
  replacedByDataFile?: ReplacementDataFileInfo; // additional info about the replacement file - although not always returned by the backend, even if it exists!
  permissions: DataFilePermissions;
}

interface ReplacementDataFileInfo extends DataFileInfo {
  hasValidReplacementPlan?: boolean;
}

export interface DataSetInfo {
  id: string | undefined;
  extension: string;
  summary: string | undefined;
  dataSetTitle: string;
  dataFileId: string;
  dataFileName: string;
  dataFileSize: number;
  rows: number | undefined;
  metaFileId: string;
  metaFileName: string;
  metaFileSize: number;
  status: ImportStatusCode;
  permissions: DataFilePermissions;
  publicApiDataSetId: string | undefined;
  publicApiDataSetVersion: string | undefined;
  dataSetUpload: DataSetUpload | undefined;
  created: string;
  userName: string;
  replacedBy?: string;
}

export interface DataSetUpload {
  id: string;
  dataSetTitle: string;
  dataFileName: string;
  dataFileSize: string;
  metaFileName: string;
  metaFileSize: string;
  status: DataSetUploadStatus;
  screenerResult?: ScreenerResult; // Nullable if screening fails
  created: Date;
  uploadedBy: string;
  replacingFileId?: string;
}

export interface ScreenerResult {
  overallResult: ScreenerOverallResult;
  message: string;
  testResults: ScreenerTestSummary[];
}

export interface ScreenerTestSummary {
  id: string;
  testFunctionName: string;
  result: ScreenerTestResult;
  notes: string | undefined;
  stage: ScreenerTestStage;
}

export interface DeleteDataFilePlan {
  deleteDataBlockPlan: DeleteDataBlockPlan;
  footnoteIds: string[];
}

// Mapped from DataFileInfo - see mapFile function below
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
  replacedByDataFile?: ReplacementDataFile;
  created?: string;
  isDeleting?: boolean;
  isCancelling?: boolean;
  permissions: DataFilePermissions;
  publicApiDataSetId?: string;
  publicApiDataSetVersion?: string;
}

export interface ReplacementDataFile extends DataFile {
  hasValidReplacementPlan?: boolean;
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

export type FileType = 'data' | 'metadata';

export interface DataFileUpdateRequest {
  title: string;
}

export type ImportStatusCode =
  | 'COMPLETE'
  | 'QUEUED'
  | 'STAGE_1'
  | 'STAGE_2'
  | 'STAGE_3'
  | 'NOT_FOUND'
  | 'FAILED'
  | 'CANCELLING'
  | 'CANCELLED';

export type DataSetUploadStatus =
  | 'UPLOADING'
  | 'SCREENING'
  | 'FAILED_SCREENING'
  | 'PENDING_REVIEW'
  | 'PENDING_IMPORT';

export type ScreenerOverallResult = 'Passed' | 'Failed';

export type ScreenerTestResult = 'PASS' | 'FAIL' | 'WARNING';

export type ScreenerTestStage =
  | 'InitialFileValidation'
  | 'PreScreening1'
  | 'PreScreening2'
  | 'FullChecks'
  | 'Passed';

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
    replacedByDataFile:
      file.replacedByDataFile === undefined
        ? undefined
        : mapFile(file.replacedByDataFile),
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
  async getDataSetUploads(releaseId: string): Promise<DataSetUpload[]> {
    return client.get<DataSetUpload[]>(`/releaseVersions/${releaseId}/uploads`);
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
  ): Promise<void> {
    const { dataFile, metadataFile, title, replacingFileId } = request;

    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('title', title);
    data.append('dataFile', dataFile);
    data.append('metaFile', metadataFile);
    data.append('replacingFileId', replacingFileId ?? '');

    return client.post('/releaseVersions/data', data);
  },
  async uploadZippedDataSetFilePair(
    releaseId: string,
    request: UploadZipDataFileRequest,
  ): Promise<void> {
    const { zipFile, title, replacingFileId } = request;

    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('title', title);
    data.append('zipFile', zipFile);
    data.append('replacingFileId', replacingFileId ?? '');

    return client.post('/releaseVersions/zip-data', data);
  },
  async uploadBulkZipDataSetFile(
    releaseId: string,
    zipFile: File,
  ): Promise<void> {
    const data = new FormData();
    data.append('releaseVersionId', releaseId);
    data.append('zipFile', zipFile);

    return client.post('releaseVersions/upload-bulk-zip-data', data);
  },
  async deleteDataSetUpload(
    releaseId: string,
    uploadId: string,
  ): Promise<void> {
    await client.delete(`/releaseVersions/${releaseId}/upload/${uploadId}`);
  },
  async importDataSets(
    releaseId: string,
    dataSetUploadIds: string[],
  ): Promise<void> {
    await client.post<DataFileInfo[]>(
      `/releaseVersions/${releaseId}/import-data-sets`,
      dataSetUploadIds,
    );
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
    dataFileId: string,
  ): Promise<DataFileImportStatus> {
    return client.get<DataFileImportStatus>(
      `/release/${releaseId}/data/${dataFileId}/import/status`,
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
    await client
      .get<Blob>(`/release/${releaseId}/file/${id}/download`, {
        responseType: 'blob',
      })
      .then(response => downloadFile({ file: response, fileName }));
  },
  async downloadTemporaryFile(
    releaseId: string,
    fileType: FileType,
    dataSetUploadId: string,
    fileName: string,
  ): Promise<void> {
    await client
      .get<Blob>(
        `/releaseVersions/${releaseId}/${fileType}/${dataSetUploadId}/download`,
        {
          responseType: 'blob',
        },
      )
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
