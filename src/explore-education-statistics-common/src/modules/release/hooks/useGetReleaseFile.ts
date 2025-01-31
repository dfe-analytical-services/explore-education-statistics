import downloadService from '@common/services/downloadService';
import { useCallback } from 'react';

export default function useGetReleaseFile(releaseId: string) {
  return useCallback(
    (fileId: string) => downloadService.getFile(releaseId, fileId),
    [releaseId],
  );
}
