import client from '@admin/services/utils/service';
import { FileInfo } from '@common/services/types/file';
import downloadFile from './utils/file/downloadFile';

interface AncillaryFileInfo extends FileInfo {
  metaFileName: string;
  rows: number;
  userName: string;
  created: string;
}

export interface AncillaryFile {
  id: string;
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
  isDeleting?: boolean;
}

export interface UploadAncillaryFileRequest {
  name: string;
  file: File;
}

function mapFile(file: AncillaryFileInfo): AncillaryFile {
  const [size, unit] = file.size.split(' ');

  return {
    id: file.id,
    title: file.name,
    filename: file.fileName,
    fileSize: {
      size: parseInt(size, 10),
      unit,
    },
  };
}

const releaseAncillaryFileService = {
  getAncillaryFiles(releaseId: string): Promise<AncillaryFile[]> {
    return client
      .get<AncillaryFileInfo[]>(`/release/${releaseId}/ancillary`)
      .then(response => response.map(mapFile));
  },
  async uploadAncillaryFile(
    releaseId: string,
    request: UploadAncillaryFileRequest,
  ): Promise<AncillaryFile> {
    const data = new FormData();
    data.append('file', request.file);

    const file = await client.post<AncillaryFileInfo>(
      `/release/${releaseId}/ancillary`,
      data,
      {
        params: {
          name: request.name,
        },
      },
    );

    return mapFile(file);
  },
  deleteAncillaryFile(releaseId: string, fileName: string): Promise<void> {
    return client.delete<void>(`/release/${releaseId}/ancillary/${fileName}`);
  },
  downloadFile(releaseId: string, id: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/file/${id}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
};

export default releaseAncillaryFileService;
