import { createQueryKeys } from '@lukemorales/query-key-factory';
import userInvitesService from '@admin/services/user-management/userInvitesService';

const userInvitesQueries = createQueryKeys('userInvite', {
  getPendingInvites: {
    queryKey: null,
    queryFn: () => userInvitesService.getPendingInvites(),
  },
});

export default userInvitesQueries;
