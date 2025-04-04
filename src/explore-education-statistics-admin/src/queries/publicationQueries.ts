import { createQueryKeys } from '@lukemorales/query-key-factory';
import publicationService from '@admin/services/publicationService';

const publicationQueries = createQueryKeys('publication', {
  get(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () => publicationService.getPublication(publicationId),
    };
  },
  getReleaseSeries(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () => publicationService.getReleaseSeries(publicationId),
    };
  },
  getPublicationSummaries: {
    queryKey: null,
    queryFn: () => publicationService.getPublicationSummaries(),
  },
});

export default publicationQueries;
