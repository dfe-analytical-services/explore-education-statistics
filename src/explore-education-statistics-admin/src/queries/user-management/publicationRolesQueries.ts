import { createQueryKeys } from '@lukemorales/query-key-factory';
import publicationRolesService from '@admin/services/user-management/publicationRolesService';

const publicationRolesQueries = createQueryKeys('publicationRole', {
  listPublicationRoles(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () =>
        publicationRolesService.listPublicationRoles(publicationId),
    };
  },
  listPublicationRoleInvites(publicationId: string) {
    return {
      queryKey: ['Invites', publicationId],
      queryFn: () =>
        publicationRolesService.listPublicationRoleInvites(publicationId),
    };
  },
});

export default publicationRolesQueries;
