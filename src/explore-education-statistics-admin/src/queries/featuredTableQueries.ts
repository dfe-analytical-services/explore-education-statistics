import featuredTableService from '@admin/services/featuredTableService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const featuredTableQueries = createQueryKeys('featuredTable', {
  list(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => featuredTableService.listFeaturedTables(releaseId),
    };
  },
});

export default featuredTableQueries;
