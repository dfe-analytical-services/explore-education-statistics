import { createQueryKeys } from '@lukemorales/query-key-factory';
import releaseVersionService from '@admin/services/releaseVersionService';

const releaseVersionQueries = createQueryKeys('releaseVersion', {
  listDraftReleaseVersions: {
    queryKey: null,
    queryFn: () => releaseVersionService.listDraftReleaseVersions(),
  },
  listScheduledReleaseVersions: {
    queryKey: null,
    queryFn: () => releaseVersionService.listScheduledReleaseVersions(),
  },
  listReleaseVersionsForApproval: {
    queryKey: null,
    queryFn: () => releaseVersionService.listReleaseVersionsForApproval(),
  },
  get(id: string) {
    return {
      queryKey: [id],
      queryFn: () => releaseVersionService.getReleaseVersion(id),
    };
  },
  getChecklist(id: string) {
    return {
      queryKey: [id],
      queryFn: () => releaseVersionService.getReleaseVersionChecklist(id),
    };
  },
});

export default releaseVersionQueries;
