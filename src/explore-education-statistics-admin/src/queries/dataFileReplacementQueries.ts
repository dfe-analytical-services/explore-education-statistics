import { createQueryKeys } from '@lukemorales/query-key-factory';
import dataReplacementService from '@admin/services/dataReplacementService';

const dataFileReplacementQueries = createQueryKeys('user', {
  getReplacementPlan(
    releaseVersionId: string,
    dataFileId: string,
    replacementDataFileId: string,
  ) {
    return {
      queryKey: [replacementDataFileId],
      queryFn: () =>
        dataReplacementService.getReplacementPlan(
          releaseVersionId,
          dataFileId,
          replacementDataFileId,
        ),
    };
  },
});

export default dataFileReplacementQueries;
