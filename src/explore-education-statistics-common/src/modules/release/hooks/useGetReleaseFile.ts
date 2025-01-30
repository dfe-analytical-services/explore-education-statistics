import downloadService from '@common/services/downloadService';
import { useCallback } from 'react';

export default function useGetReleaseFile(releaseVersionId: string) {
  return useCallback(
    (fileId: string) => downloadService.getFile(releaseVersionId, fileId),
    [releaseVersionId],
  );
}
