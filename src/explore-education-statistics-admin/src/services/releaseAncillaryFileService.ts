import client from '@admin/services/utils/service';
import { FileInfo } from '@common/services/types/file';
import downloadFile from '@common/utils/file/downloadFile';

interface AncillaryFileInfo extends FileInfo {
  summary: string;
  userName: string;
  created: string;
}

export interface AncillaryFile {
  id: string;
  title: string;
  summary: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
  userName: string;
  created: string;
  isDeleting?: boolean;
}

export interface UploadAncillaryFileRequest {
  title: string;
  summary: string;
  file: File;
}

export interface AncillaryFileUpdateRequest {
  title: string;
  summary: string;
}

function mapFile({ name, ...file }: AncillaryFileInfo): AncillaryFile {
  const [size, unit] = file.size.split(' ');

  return {
    ...file,
    title: name,
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
  getAncillaryFile(releaseId: string, fileId: string): Promise<AncillaryFile> {
    return client
      .get<AncillaryFileInfo>(`/release/${releaseId}/file/${fileId}`)
      .then(mapFile);
  },
  async uploadAncillaryFile(
    releaseId: string,
    request: UploadAncillaryFileRequest,
  ): Promise<AncillaryFile> {
    const data = new FormData();
    data.append('file', request.file);
    data.append('title', request.title);
    data.append('summary', request.summary);

    const file = await client.post<AncillaryFileInfo>(
      `/release/${releaseId}/ancillary`,
      data,
    );

    return mapFile(file);
  },
  deleteAncillaryFile(releaseId: string, fileId: string): Promise<void> {
    return client.delete<void>(`/release/${releaseId}/ancillary/${fileId}`);
  },
  downloadFile(releaseId: string, id: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/file/${id}/download`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
  updateFile(
    releaseId: string,
    fileId: string,
    data: AncillaryFileUpdateRequest,
  ): Promise<void> {
    return client.patch(`/release/${releaseId}/file/${fileId}`, data);
  },
};

export default releaseAncillaryFileService;
