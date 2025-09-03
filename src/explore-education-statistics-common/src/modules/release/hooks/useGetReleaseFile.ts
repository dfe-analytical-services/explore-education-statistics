import downloadService from '@common/services/downloadService';
import { useCallback } from 'react';

// TODO - EES-6480 - rewrite to use token-based secure download?
// If so, we would need a Controller to support this in projects
// other than just Admin, or a separate Function App that handles
// secure Blob downloads and remove the SecureBlobDownloadController
// from Admin.
export default function useGetReleaseFile(releaseVersionId: string) {
  return useCallback(
    (fileId: string) => downloadService.getFile(releaseVersionId, fileId),
    [releaseVersionId],
  );
}
