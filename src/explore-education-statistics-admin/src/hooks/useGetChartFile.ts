import { useCallback } from 'react';
import streamReleaseFileSecurely from '@admin/pages/release/data/components/utils/streamReleaseFileSecurely';

export default function useGetChartFile(releaseVersionId: string) {
  return useCallback(
    (fileId: string) =>
      streamReleaseFileSecurely({
        releaseVersionId,
        fileId,
      }),
    [releaseVersionId],
  );
}
