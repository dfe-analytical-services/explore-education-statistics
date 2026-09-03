import dataBlockService from '@admin/services/dataBlockService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const dataBlockQueries = createQueryKeys('dataBlocks', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => dataBlockService.listDataBlocks(releaseId),
    };
  },
  getDeleteBlockPlan(releaseVersionId: string, dataBlockId: string) {
    return {
      queryKey: [releaseVersionId, dataBlockId],
      queryFn: () =>
        dataBlockService.getDeleteBlockPlan(releaseVersionId, dataBlockId),
    };
  },
});

export default dataBlockQueries;
