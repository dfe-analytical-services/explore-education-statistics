import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseDataGuidanceService from '@admin/services/releaseDataGuidanceService';

const dataGuidanceQueries = createQueryKeys('dataGuidance', {
  getDataGuidance(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseDataGuidanceService.getDataGuidance(releaseId),
    };
  },
});

export default dataGuidanceQueries;
