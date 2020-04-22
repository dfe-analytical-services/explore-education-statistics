import { contentApi } from '@common/services/api';

export default {
  downloadChartFile(releaseId: string, fileName: string): Promise<Blob> {
    return contentApi.get<Blob>(`/release/${releaseId}/chart/${fileName}`, {
      responseType: 'blob',
    });
  },
};
