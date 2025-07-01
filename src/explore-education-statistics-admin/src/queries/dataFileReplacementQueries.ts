import { createQueryKeys } from '@lukemorales/query-key-factory';
import dataReplacementService from '@admin/services/dataReplacementService';

const dataFileReplacementQueries = createQueryKeys('user', {
  getReplacementPlan(releaseVersionId: string, dataFileId: string) {
    return {
      queryKey: [dataFileId],
      queryFn: () =>
        dataReplacementService.getReplacementPlan(releaseVersionId, dataFileId),
    };
  },
});

export default dataFileReplacementQueries;
