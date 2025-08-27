import { useCallback } from 'react';
import getReleaseFileSecurely from '@admin/pages/release/data/components/utils/getReleaseFileSecurely';

export default function useGetChartFile(releaseVersionId: string) {
  return useCallback(
    (fileId: string) =>
      getReleaseFileSecurely({
        releaseVersionId,
        fileId,
      }),
    [releaseVersionId],
  );
}
