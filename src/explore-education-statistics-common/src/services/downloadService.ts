import { contentApi } from '@common/services/api';

export default {
  getFile(releaseId: string, fileId: string): Promise<Blob> {
    return contentApi.get<Blob>(`/releases/${releaseId}/files/${fileId}`, {
      responseType: 'blob',
    });
  },
};
