import { createQueryKeys } from '@lukemorales/query-key-factory';
import preReleaseUsersService from '@admin/services/user-management/preReleaseUsersService';

const preReleaseUsersQueries = createQueryKeys('preReleaseUser', {
  getAllPreReleaseUsers: {
    queryKey: null,
    queryFn: () => preReleaseUsersService.getAllPreReleaseUsers(),
  },
  getPreReleaseUsers(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () =>
        preReleaseUsersService.getPreReleaseUsers(releaseVersionId),
    };
  },
  getPreReleaseUsersInvitePlan(releaseVersionId: string, emails: string[]) {
    return {
      queryKey: [releaseVersionId, emails],
      queryFn: () =>
        preReleaseUsersService.getPreReleaseUsersInvitePlan(
          releaseVersionId,
          emails,
        ),
    };
  },
});

export default preReleaseUsersQueries;
