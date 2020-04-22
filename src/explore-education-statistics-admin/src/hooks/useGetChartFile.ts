import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import { useCallback } from 'react';

export default function useGetChartFile(releaseId: string) {
  return useCallback(
    (fileId: string) =>
      editReleaseDataService.downloadChartFile(releaseId, fileId),
    [releaseId],
  );
}
