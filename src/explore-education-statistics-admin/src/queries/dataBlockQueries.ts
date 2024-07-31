import dataBlockService from '@admin/services/dataBlockService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const dataBlockQueries = createQueryKeys('dataBlocks', {
  list(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => dataBlockService.listDataBlocks(releaseVersionId),
    };
  },
});

export default dataBlockQueries;
