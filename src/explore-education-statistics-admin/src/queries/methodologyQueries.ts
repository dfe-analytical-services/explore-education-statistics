import { createQueryKeys } from '@lukemorales/query-key-factory';
import methodologyService from '@admin/services/methodologyService';

const methodologyQueries = createQueryKeys('methodology', {
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
  get(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () => methodologyService.getMethodology(methodologyId),
    };
  },
});

export default methodologyQueries;
