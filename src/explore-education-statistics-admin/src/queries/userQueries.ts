import { createQueryKeys } from '@lukemorales/query-key-factory';
import userService from '@admin/services/userService';

const userQueries = createQueryKeys('user', {
  get(userId: string) {
    return {
      queryKey: [userId],
      queryFn: () => userService.getUser(userId),
    };
  },
  getRoles: {
    queryKey: null,
    queryFn: () => userService.getRoles(),
  },
  getResourceRoles: {
    queryKey: null,
    queryFn: () => userService.getResourceRoles(),
  },
  getReleases: {
    queryKey: null,
    queryFn: () => userService.getReleases(),
  },
});

export default userQueries;
