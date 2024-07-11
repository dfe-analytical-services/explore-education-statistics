import { createQueryKeys } from '@lukemorales/query-key-factory';
import boundaryDataService from '@admin/services/boundaryDataService';

const boundaryDataQueries = createQueryKeys('release', {
  getBoundaryLevels: {
    queryKey: null,
    queryFn: () => boundaryDataService.getBoundaryLevels(),
  },
  getBoundaryLevel(id: string) {
    return {
      queryKey: [id],
      queryFn: () => boundaryDataService.getBoundaryLevel(id),
    };
  },
});

export default boundaryDataQueries;
