import downloadService from '@common/services/downloadService';
import { useCallback } from 'react';

export default function useGetChartFile(
  publicationSlug: string,
  releaseSlug: string,
) {
  return useCallback(
    (fileId: string) =>
      downloadService.getChartFile(publicationSlug, releaseSlug, fileId),
    [publicationSlug, releaseSlug],
  );
}
