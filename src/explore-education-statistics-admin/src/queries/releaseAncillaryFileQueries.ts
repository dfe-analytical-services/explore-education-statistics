import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const releaseAncillaryFileQueries = createQueryKeys('releaseAncillaryFile', {
  get(releaseId: string, fileId: string) {
    return {
      queryKey: [releaseId, fileId],
      queryFn: () => releaseAncillaryFileService.getFile(releaseId, fileId),
    };
  },
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseAncillaryFileService.listFiles(releaseId),
    };
  },
});

export default releaseAncillaryFileQueries;
