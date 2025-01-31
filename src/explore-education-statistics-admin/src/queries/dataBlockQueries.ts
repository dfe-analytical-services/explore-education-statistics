import dataBlockService from '@admin/services/dataBlockService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const dataBlockQueries = createQueryKeys('dataBlocks', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => dataBlockService.listDataBlocks(releaseId),
    };
  },
});

export default dataBlockQueries;
