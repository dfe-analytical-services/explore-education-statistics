import releaseChartFileService from '@admin/services/releaseChartFileService';
import { useCallback } from 'react';

export default function useGetChartFile(releaseVersionId: string) {
  return useCallback(
    (fileId: string) =>
      releaseChartFileService.getChartFile(releaseVersionId, fileId),
    [releaseVersionId],
  );
}
