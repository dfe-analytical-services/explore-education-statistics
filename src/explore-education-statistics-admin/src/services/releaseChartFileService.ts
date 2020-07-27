import client from '@admin/services/utils/service';
import { FileInfo } from './types/file';

export interface ChartFile {
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
    title: file.name,
    filename: file.fileName,
    fileSize: {
      size: parseInt(size, 10),
      unit,
    },
  };
}

const releaseChartFileService = {
  async getChartFiles(releaseId: string): Promise<ChartFile[]> {
    const response = await client.get<FileInfo[]>(
      `/release/${releaseId}/chart`,
    );

    return response.map(mapFile);
  },

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

  async deleteChartFile(
    releaseId: string,
    subjectName: string,
    fileName: string,
  ): Promise<boolean> {
    return client.delete<boolean>(
      `/release/${releaseId}/chart/${subjectName}/${fileName}`,
    );
  },

  getChartFile(releaseId: string, fileName: string): Promise<Blob> {
    return client.get<Blob>(`/release/${releaseId}/chart/${fileName}`, {
      responseType: 'blob',
    });
  },
};

export default releaseChartFileService;
