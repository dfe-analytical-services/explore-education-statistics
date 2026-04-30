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
});

export default publicationRolesQueries;
