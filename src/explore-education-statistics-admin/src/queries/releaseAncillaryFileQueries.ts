import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const releaseAncillaryFileQueries = createQueryKeys('releaseAncillaryFile', {
  get(releaseVersionId: string, fileId: string) {
    return {
      queryKey: [releaseVersionId, fileId],
      queryFn: () =>
        releaseAncillaryFileService.getFile(releaseVersionId, fileId),
    };
  },
  list(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => releaseAncillaryFileService.listFiles(releaseVersionId),
    };
  },
});

export default releaseAncillaryFileQueries;
