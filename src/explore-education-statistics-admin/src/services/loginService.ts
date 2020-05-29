import { ApplicationPaths } from '@admin/components/api-authorization/ApiAuthorizationConstants';

export interface LoginService {
  getSignInLink: () => string;
  getSignOutLink: () => {
    pathname: string;
    state: {
      local: boolean;
    };
  };
}

const loginService: LoginService = {
  getSignInLink: () => ApplicationPaths.Login,
  getSignOutLink: () => ({
    pathname: ApplicationPaths.LogOut,
    state: {
      local: true,
    },
  }),
};

export default loginService;
