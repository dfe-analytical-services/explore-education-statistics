import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseService from '@admin/services/releaseService';

const releaseQueries = createQueryKeys('release', {
  listDraftReleases: {
    queryKey: null,
    queryFn: () => releaseService.getDraftReleases(),
  },
  listScheduledReleases: {
    queryKey: null,
    queryFn: () => releaseService.getScheduledReleases(),
  },
});

export default releaseQueries;
