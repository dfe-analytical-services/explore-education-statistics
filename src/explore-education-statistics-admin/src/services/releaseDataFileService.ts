import { DeleteDataBlockPlan } from '@admin/services/dataBlockService';
import { FileInfo } from '@admin/services/types/file';
import client from '@admin/services/utils/service';
import getFileNameFromPath from './utils/file/getFileNameFromPath';
import downloadFile from './utils/file/downloadFile';

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

interface UploadDataFilesRequest {
  subjectTitle: string;
  dataFile: File;
  metadataFile: File;
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
  uploadDataFiles(
    releaseId: string,
    request: UploadDataFilesRequest,
  ): Promise<DataFile> {
    const data = new FormData();
    data.append('file', request.dataFile);
    data.append('metaFile', request.metadataFile);

    return client
      .post<DataFileInfo[]>(
        `/release/${releaseId}/data?name=${request.subjectTitle}`,
        data,
      )
      .then(response => {
        return response
          .filter(file => file.metaFileName.length > 0)
          .map(mapFile)[0];
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
