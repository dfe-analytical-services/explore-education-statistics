import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseService from '@admin/services/releaseService';

const releaseQueries = createQueryKeys('release', {
  listDraftReleases: {
    queryKey: null,
    queryFn: () => releaseService.listDraftReleases(),
  },
  listScheduledReleases: {
    queryKey: null,
    queryFn: () => releaseService.listScheduledReleases(),
  },
  listReleasesForApproval: {
    queryKey: null,
    queryFn: () => releaseService.listReleasesForApproval(),
  },
  get(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseService.getRelease(releaseId),
    };
  },
});

export default releaseQueries;
