import { contentApi } from '@common/services/api';
import downloadFile from '@common/utils/file/downloadFile';
import qs from 'qs';

type FromPage = 'ReleaseUsefulInfo' | 'ReleaseDownloads' | 'DataCatalogue';

const downloadService = {
  getFile(releaseVersionId: string, fileId: string): Promise<Blob> {
    return contentApi.get<Blob>(
      `/releases/${releaseVersionId}/files/${fileId}`,
      {
        responseType: 'blob',
      },
    );
  },
  async downloadFiles(
    releaseVersionId: string,
    fileIds: string[],
  ): Promise<void> {
    const query = qs.stringify({ fileIds }, { arrayFormat: 'comma' });

    downloadFile(
      `${contentApi.baseURL}/releases/${releaseVersionId}/files?${query}`,
    );
  },
};

export default downloadService;
