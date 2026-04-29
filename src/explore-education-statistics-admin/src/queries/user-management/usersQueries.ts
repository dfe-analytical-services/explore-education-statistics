import { createQueryKeys } from '@lukemorales/query-key-factory';
import userService from '@admin/services/user-management/usersService';

const userQueries = createQueryKeys('user', {
  getAllUsers: {
    queryKey: null,
    queryFn: () => userService.getAllUsers(),
  },
  getUser(userId: string) {
    return {
      queryKey: [userId],
      queryFn: () => userService.getUser(userId),
    };
  },
});

export default userQueries;
