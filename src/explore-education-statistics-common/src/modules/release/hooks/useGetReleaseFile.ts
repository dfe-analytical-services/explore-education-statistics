import downloadService from '@common/services/downloadService';
import { useCallback } from 'react';

// TODO EES-6359 - not sure if we can rewrite this. Used in common codebase.
export default function useGetReleaseFile(releaseVersionId: string) {
  return useCallback(
    (fileId: string) => downloadService.getFile(releaseVersionId, fileId),
    [releaseVersionId],
  );
}
