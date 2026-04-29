import { createQueryKeys } from '@lukemorales/query-key-factory';
import globalRolesService from '@admin/services/user-management/globalRolesService';

const globalRolesQueries = createQueryKeys('globalRoles', {
  getRoles: {
    queryKey: null,
    queryFn: () => globalRolesService.getRoles(),
  },
});

export default globalRolesQueries;
