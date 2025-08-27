import client from '@admin/services/utils/service';
import { FileInfo } from '@common/services/types/file';

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
  file?: File | null;
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
  listFiles(releaseId: string): Promise<AncillaryFile[]> {
    return client
      .get<AncillaryFileInfo[]>(`/release/${releaseId}/ancillary`)
      .then(response => response.map(mapFile));
  },
  getFile(releaseId: string, fileId: string): Promise<AncillaryFile> {
    return client
      .get<AncillaryFileInfo>(`/release/${releaseId}/file/${fileId}`)
      .then(mapFile);
  },
  async createFile(
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
  async updateFile(
    releaseId: string,
    fileId: string,
    request: AncillaryFileUpdateRequest,
  ): Promise<AncillaryFile> {
    const data = new FormData();
    data.append('title', request.title);
    data.append('summary', request.summary);
    if (request.file) {
      data.append('file', request.file);
    }

    const file = await client.put<AncillaryFileInfo>(
      `release/${releaseId}/ancillary/${fileId}`,
      data,
    );

    return mapFile(file);
  },
  deleteFile(releaseId: string, fileId: string): Promise<void> {
    return client.delete<void>(`/release/${releaseId}/ancillary/${fileId}`);
  },
};

export default releaseAncillaryFileService;
