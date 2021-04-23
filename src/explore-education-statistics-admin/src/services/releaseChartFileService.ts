import client from '@admin/services/utils/service';
import { FileInfo } from '@common/services/types/file';

export interface ChartFile {
  id: string;
  title: string;
  filename: string;
  fileSize: {
    size: number;
    unit: string;
  };
}

interface UploadChartFileRequest {
  file: File;
}

function mapFile(file: FileInfo): ChartFile {
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

const releaseChartFileService = {
  async uploadChartFile(
    releaseId: string,
    request: UploadChartFileRequest,
  ): Promise<ChartFile> {
    const data = new FormData();
    data.append('file', request.file);

    const file = await client.post<FileInfo>(
      `/release/${releaseId}/chart`,
      data,
    );

    return mapFile(file);
  },

  async deleteChartFile(releaseId: string, id: string): Promise<void> {
    return client.delete<void>(`/release/${releaseId}/chart/${id}`);
  },

  getChartFile(releaseId: string, id: string): Promise<Blob> {
    return client.get<Blob>(`/release/${releaseId}/file/${id}/download`, {
      responseType: 'blob',
    });
  },
};

export default releaseChartFileService;
