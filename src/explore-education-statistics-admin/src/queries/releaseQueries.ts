import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseService from '@admin/services/releaseService';

const releaseQueries = createQueryKeys('release', {
  getReleases: {
    queryKey: null,
    queryFn: () => releaseService.getReleases(),
  },
});

export default releaseQueries;
