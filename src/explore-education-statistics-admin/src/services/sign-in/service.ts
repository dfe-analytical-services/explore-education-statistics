import {ApplicationPaths} from '@admin/components/api-authorization/ApiAuthorizationConstants';
import authService from '@admin/components/api-authorization/AuthorizeService';
import {User} from '@admin/services/sign-in/types';
import client from '@admin/services/util/service';

export interface LoginService {
  getUserDetails: () => Promise<User>;
  getSignInLink: () => string;
  getSignOutLink: () => {
    pathname: string;
    state: {
      local: boolean;
    }
  };
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
  getSignInLink: () => ApplicationPaths.Login,
  getSignOutLink: () => ({
    pathname: ApplicationPaths.LogOut,
    state: {
      local: true,
    }
  }),
};

export default service;
