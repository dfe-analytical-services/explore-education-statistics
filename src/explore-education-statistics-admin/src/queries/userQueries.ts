import { createQueryKeys } from '@lukemorales/query-key-factory';
import userService from '@admin/services/user-management/usersService';

const userQueries = createQueryKeys('user', {
  get(userId: string) {
    return {
      queryKey: [userId],
      queryFn: () => userService.getUser(userId),
    };
  },
  getReleases: {
    queryKey: null,
    queryFn: () => userService.getReleases(),
  },
  deleteUser(email: string) {
    return {
      queryKey: [email],
      queryFn: () => userService.deleteUser(email),
    };
  },
  getUsers: {
    queryKey: null,
    queryFn: () => userService.getUsers(),
  },
  getPreReleaseUsers: {
    queryKey: null,
    queryFn: () => userService.getPreReleaseUsers(),
  },
});

export default userQueries;
