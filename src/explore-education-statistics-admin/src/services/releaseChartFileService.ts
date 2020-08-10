import client from '@admin/services/utils/service';
import { FileInfo } from './types/file';

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
    id: string,
    request: UploadChartFileRequest,
  ): Promise<ChartFile> {
    const data = new FormData();
    data.append('file', request.file);
    let file;

    if (id) {
      file = await client.put<FileInfo>(
        `/release/${releaseId}/chart/${id}`,
        data,
      );
    } else {
      file = await client.post<FileInfo>(`/release/${releaseId}/chart`, data);
    }
    return mapFile(file);
  },

  async deleteChartFile(
    releaseId: string,
    subjectName: string,
    id: string,
  ): Promise<void> {
    return client.delete<void>(
      `/release/${releaseId}/chart/${subjectName}/${id}`,
    );
  },

  getChartFile(releaseId: string, fileName: string): Promise<Blob> {
    return client.get<Blob>(`/release/${releaseId}/chart/${fileName}`, {
      responseType: 'blob',
    });
  },
};

export default releaseChartFileService;
