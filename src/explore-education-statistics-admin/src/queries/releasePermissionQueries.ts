import { createQueryKeys } from '@lukemorales/query-key-factory';
import releasePermissionService from '@admin/services/releasePermissionService';

const releasePermissionQueries = createQueryKeys('releasePermission', {
  listRoles(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => releasePermissionService.listRoles(releaseVersionId),
    };
  },
  listInvites(releaseVersionId: string) {
    return {
      queryKey: [releaseVersionId],
      queryFn: () => releasePermissionService.listInvites(releaseVersionId),
    };
  },
});

export default releasePermissionQueries;
