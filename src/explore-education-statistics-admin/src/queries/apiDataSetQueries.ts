import apiDataSetService from '@admin/services/apiDataSetService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const apiDataSetQueries = createQueryKeys('apiDataSetQueries', {
  list(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () => apiDataSetService.listDataSets(publicationId),
    };
  },
});

export default apiDataSetQueries;
