import { createQueryKeys } from '@lukemorales/query-key-factory';
import preReleaseUserService from '@admin/services/user-management/preReleaseUsersService';

const preReleaseUsersQueries = createQueryKeys('preReleaseUser', {
  getAllPreReleaseUsers: {
    queryKey: null,
    queryFn: () => preReleaseUserService.getAllPreReleaseUsers(),
  },
  getPreReleaseUsers(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => preReleaseUserService.getPreReleaseUsers(releaseVersionId),
    };
  },
  getPreReleaseUsersInvitePlan(releaseVersionId: string, emails: string[]) {
    return {
      queryKey: [releaseVersionId, emails],
      queryFn: () =>
        preReleaseUserService.getPreReleaseUsersInvitePlan(
          releaseVersionId,
          emails,
        ),
    };
  },
});

export default preReleaseUsersQueries;
