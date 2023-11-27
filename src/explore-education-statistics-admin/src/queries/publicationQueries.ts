import { createQueryKeys } from '@lukemorales/query-key-factory';
import publicationService from '@admin/services/publicationService';

const publicationQueries = createQueryKeys('publication', {
  get(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () => publicationService.getPublication(publicationId),
    };
  },
});

export default publicationQueries;
