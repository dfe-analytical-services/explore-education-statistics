import { contentApi } from '@common/services/api';
import downloadFile from '@common/utils/file/downloadFile';
import qs from 'qs';

type FromPage =
  | 'ReleaseUsefulInfo'
  | 'ReleaseDownloads'
  | 'DataCatalogue'
  | 'TableToolCropped';

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

    downloadFile({
      file: `${contentApi.baseURL}/releases/${releaseVersionId}/files?${query}`,
    });
  },

  async downloadZip(
    releaseVersionId: string,
    fromPage: FromPage,
    fileId?: string,
  ): Promise<void> {
    if (fileId !== undefined) {
      downloadFile(
        // TODO EES-6034 this endpoint now only needs to accept a single fileId, not potential list of files
        {
          file: `${contentApi.baseURL}/releases/${releaseVersionId}/files?fromPage=${fromPage}&fileIds=${fileId}`,
        },
      );
    } else {
      // If no fileId is provided, download zip with all data sets for release version
      downloadFile({
        file: `${contentApi.baseURL}/releases/${releaseVersionId}/files?fromPage=${fromPage}`,
      });
    }
  },
};

export default downloadService;
