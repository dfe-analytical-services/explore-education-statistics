import { createQueryKeys } from '@lukemorales/query-key-factory';
import releasePermissionService from '@admin/services/releasePermissionService';

const releasePermissionQueries = createQueryKeys('releasePermission', {
  listRoles(releaseId: string) {
    return {
      queryKey: ['listReleaseRoles', releaseId],
      queryFn: () => releasePermissionService.listRoles(releaseId),
    };
  },
  listInvites(releaseId: string) {
    return {
      queryKey: ['listReleaseInvites', releaseId],
      queryFn: () => releasePermissionService.listInvites(releaseId),
    };
  },
});

export default releasePermissionQueries;
