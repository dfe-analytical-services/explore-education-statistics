import { GetFileResponse } from '@admin/services/types/file';
import client from '@admin/services/utils/service';
import getFileNameFromPath from './utils/file/getFileNameFromPath';

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

const releaseChartFileService = {
  async getChartFiles(releaseId: string) {
    const response = await client.get<GetFileResponse[]>(
      `/release/${releaseId}/chart`,
    );

    return response.map<ChartFile>(({ name, path, size }) => ({
      title: name,
      filename: getFileNameFromPath(path),
      fileSize: {
        size: parseInt(size.split(' ')[0], 10),
        unit: size.split(' ')[1],
      },
    }));
  },

  async uploadChartFile(releaseId: string, request: UploadChartFileRequest) {
    const data = new FormData();
    data.append('file', request.file);
    return client.post<null>(`/release/${releaseId}/chart`, data);
  },

  async deleteChartFile(
    releaseId: string,
    subjectName: string,
    fileName: string,
  ): Promise<null> {
    return client.delete<null>(
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
