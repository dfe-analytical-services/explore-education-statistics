import releaseChartFileService from '@admin/services/releaseChartFileService';
import { useCallback } from 'react';

export default function useGetChartFile(releaseId: string) {
  return useCallback(
    (fileId: string) => releaseChartFileService.getChartFile(releaseId, fileId),
    [releaseId],
  );
}
