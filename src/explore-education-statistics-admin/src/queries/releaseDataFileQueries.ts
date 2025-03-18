import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';

const releaseDataFileQueries = createQueryKeys('releaseDataFile', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseDataFileService.getDataFiles(releaseId),
    };
  },
  getDeleteFilePlan(releaseVersionId: string, dataFile: DataFile) {
    return {
      queryKey: [releaseVersionId, dataFile],
      queryFn: () =>
        releaseDataFileService.getDeleteDataFilePlan(
          releaseVersionId,
          dataFile,
        ),
    };
  },
});

export default releaseDataFileQueries;
