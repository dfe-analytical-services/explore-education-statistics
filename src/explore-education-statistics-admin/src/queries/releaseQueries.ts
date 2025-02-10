import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseVersionService from '@admin/services/releaseVersionService';

const releaseQueries = createQueryKeys('release', {
  listDraftReleases: {
    queryKey: null,
    queryFn: () => releaseVersionService.listDraftReleases(),
  },
  listScheduledReleases: {
    queryKey: null,
    queryFn: () => releaseVersionService.listScheduledReleases(),
  },
  listReleasesForApproval: {
    queryKey: null,
    queryFn: () => releaseVersionService.listReleasesForApproval(),
  },
  get(releaseId: string) {
    return {
      queryKey: [releaseId],
      queryFn: () => releaseVersionService.getRelease(releaseId),
    };
  },
});

export default releaseQueries;
