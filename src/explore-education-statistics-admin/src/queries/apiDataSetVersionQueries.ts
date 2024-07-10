import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const apiDataSetVersionQueries = createQueryKeys('apiDataSetVersionQueries', {
  getLocationsMapping(versionId: string) {
    return {
      queryKey: [versionId],
      queryFn: () => apiDataSetVersionService.getLocationsMapping(versionId),
    };
  },
});

export default apiDataSetVersionQueries;
