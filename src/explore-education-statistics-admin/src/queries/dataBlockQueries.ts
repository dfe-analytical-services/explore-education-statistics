import dataBlockService from '@admin/services/dataBlockService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const dataBlockQueries = createQueryKeys('dataBlocks', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => dataBlockService.listDataBlocks(releaseId),
    };
  },
  getDeleteBlockPlan(releaseVersionId: string, dataBlockVersionId: string) {
    return {
      queryKey: [releaseVersionId, dataBlockVersionId],
      queryFn: () =>
        dataBlockService.getDeleteBlockPlan(
          releaseVersionId,
          dataBlockVersionId,
        ),
    };
  },
});

export default dataBlockQueries;
