import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseDataFileService from '@admin/services/releaseDataFileService';

const releaseDataFileQueries = createQueryKeys('releaseDataFile', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseDataFileService.getDataFiles(releaseId),
    };
  },
  getDataFile(releaseVersionId: string, dataFileId: string) {
    return {
      queryKey: [releaseVersionId, dataFileId],
      queryFn: () =>
        releaseDataFileService.getDataFile(releaseVersionId, dataFileId),
    };
  },
  listUploads(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseDataFileService.getDataSetUploads(releaseId),
    };
  },
  getDeleteFilePlan(releaseVersionId: string, dataFileId: string) {
    return {
      queryKey: [releaseVersionId, dataFileId],
      queryFn: () =>
        releaseDataFileService.getDeleteDataFilePlan(
          releaseVersionId,
          dataFileId,
        ),
    };
  },
});

export default releaseDataFileQueries;
