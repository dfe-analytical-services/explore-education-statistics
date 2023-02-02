import { UseQueryOptions } from '@tanstack/react-query';
import releasePermissionService, {
  UserReleaseInvite,
  UserReleaseRole,
} from '@admin/services/releasePermissionService';

const releasePermissionQueries = {
  listRoles(releaseId: string): UseQueryOptions<UserReleaseRole[]> {
    return {
      queryKey: ['listReleaseRoles', releaseId],
      queryFn: () => releasePermissionService.listRoles(releaseId),
    };
  },
  listInvites(releaseId: string): UseQueryOptions<UserReleaseInvite[]> {
    return {
      queryKey: ['listReleaseInvites', releaseId],
      queryFn: () => releasePermissionService.listInvites(releaseId),
    };
  },
} as const;

export default releasePermissionQueries;
