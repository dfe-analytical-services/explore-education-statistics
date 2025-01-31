import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseDataFileService from '@admin/services/releaseDataFileService';

const releaseDataFileQueries = createQueryKeys('releaseDataFile', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseDataFileService.getDataFiles(releaseId),
    };
  },
});

export default releaseDataFileQueries;
