import { createQueryKeys } from '@lukemorales/query-key-factory';
import methodologyService from '@admin/services/methodologyService';

const methodologyQueries = createQueryKeys('methodology', {
  get(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () => methodologyService.getMethodology(methodologyId),
    };
  },
  getMethodologyStatuses(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () => methodologyService.getMethodologyStatuses(methodologyId),
    };
  },
  listMethodologiesForApproval: {
    queryKey: null,
    queryFn: () => methodologyService.listMethodologiesForApproval(),
  },
  listLatestMethodologyVersions(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () =>
        methodologyService.listLatestMethodologyVersions(publicationId),
    };
  },
});

export default methodologyQueries;
