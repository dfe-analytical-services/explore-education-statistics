import { contentApi } from '@common/services/api';

export default {
  getChartFile(
    publicationSlug: string,
    releaseSlug: string,
    fileName: string,
  ): Promise<Blob> {
    return contentApi.get<Blob>(
      `/download/${publicationSlug}/${releaseSlug}/chart/${fileName}`,
      {
        responseType: 'blob',
      },
    );
  },
};
