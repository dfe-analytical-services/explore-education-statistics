import { ApplicationPaths } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import { User } from '@admin/services/sign-in/types';
import client from '@admin/services/util/service';

export interface LoginService {
  getUserDetails: () => Promise<User>;
  getSignInLink: () => string;
  getSignOutLink: () => {
    pathname: string;
    state: {
      local: boolean;
    };
  };
}

const service: LoginService = {
  getUserDetails: async () => {
    return client.get('/users/mydetails');
  },
  getSignInLink: () => ApplicationPaths.Login,
  getSignOutLink: () => ({
    pathname: ApplicationPaths.LogOut,
    state: {
      local: true,
    },
  }),
};

export default service;
