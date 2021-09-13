import { contentApi } from '@common/services/api';
import downloadFile from '@common/utils/file/downloadFile';
import qs from 'qs';

export default {
  getFile(releaseId: string, fileId: string): Promise<Blob> {
    return contentApi.get<Blob>(`/releases/${releaseId}/files/${fileId}`, {
      responseType: 'blob',
    });
  },
  async downloadFiles(releaseId: string, fileIds: string[]): Promise<void> {
    const query = qs.stringify({ fileIds }, { arrayFormat: 'comma' });

    downloadFile(
      `${process.env.CONTENT_API_BASE_URL}/releases/${releaseId}/files?${query}`,
    );
  },
};
