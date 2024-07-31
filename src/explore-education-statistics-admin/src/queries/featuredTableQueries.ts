import featuredTableService from '@admin/services/featuredTableService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const featuredTableQueries = createQueryKeys('featuredTable', {
  list(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => featuredTableService.listFeaturedTables(releaseVersionId),
    };
  },
});

export default featuredTableQueries;
