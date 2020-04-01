import { dataApi } from '@common/services/api';

export default {
  getChartFile(
    publicationSlug: string,
    releaseSlug: string,
    fileName: string,
  ): Promise<Blob> {
    return dataApi.get<Blob>(
      `/download/${publicationSlug}/${releaseSlug}/chart/${fileName}`,
      {
        responseType: 'blob',
      },
    );
  },
};
