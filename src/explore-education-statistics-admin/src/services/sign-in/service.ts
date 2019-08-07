import signInRoutes from '@admin/routes/sign-in/routes';
import { User } from '@admin/services/sign-in/types';
import client from '@admin/services/util/service';

export interface LoginService {
  getUserDetails: () => Promise<User>;
  getSignInLink: () => string;
  getSignOutLink: () => string;
}

const service: LoginService = {
  getUserDetails: () => client.get('/users/mydetails'),
  getSignInLink: () => signInRoutes.signInViaApiLink,
  getSignOutLink: () => signInRoutes.signOutViaApiLink,
};

export default service;
