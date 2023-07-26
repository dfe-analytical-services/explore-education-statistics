import { createQueryKeys } from '@lukemorales/query-key-factory';
import methodologyService from '@admin/services/methodologyService';

const methodologyQueries = createQueryKeys('methodology', {
  getMethodologyStatuses(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () => methodologyService.getMethodologyStatuses(methodologyId),
    };
  },
});

export default methodologyQueries;
