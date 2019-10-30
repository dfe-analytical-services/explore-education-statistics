import signInRoutes from '@admin/routes/sign-in/routes';
import { User } from '@admin/services/sign-in/types';
import client from '@admin/services/util/service';
import authService from '@admin/components/api-authorization/AuthorizeService';

export interface LoginService {
  getUserDetails: () => Promise<User>;
  getSignInLink: () => string;
  getSignOutLink: () => string;
}

const service: LoginService = {
  getUserDetails: async () => {
    const token = await authService.getAccessToken();
    return client.get('/users/mydetails', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  },
  getSignInLink: () => signInRoutes.signInViaApiLink,
  getSignOutLink: () => signInRoutes.signOutViaApiLink,
};

export default service;
