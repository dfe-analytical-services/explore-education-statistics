import { GetFileResponse } from '@admin/services/types/file';
import getFileNameFromPath from '@admin/services/utils/file/getFileNameFromPath';
import client from '@admin/services/utils/service';
import downloadFile from './utils/file/downloadFile';

export interface AncillaryFile {
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
  isDeleting?: boolean;
}

interface UploadAncillaryFileRequest {
  name: string;
  file: File;
}

const releaseAncillaryFileService = {
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
};

export default releaseAncillaryFileService;
