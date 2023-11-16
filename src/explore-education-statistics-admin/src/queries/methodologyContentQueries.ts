import { createQueryKeys } from '@lukemorales/query-key-factory';
import methodologyContentService from '@admin/services/methodologyContentService';

const methodologyContentQueries = createQueryKeys('methodologyContent', {
  get(methodologyId: string) {
    return {
      queryKey: [methodologyId],
      queryFn: () =>
        methodologyContentService.getMethodologyContent(methodologyId),
    };
  },
});

export default methodologyContentQueries;
