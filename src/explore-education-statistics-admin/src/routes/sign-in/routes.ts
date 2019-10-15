import { baseURL } from '@admin/services/util/service';

export default {
  signIn: '/authentication/login',
  signOut: '/signed-out',
  signInViaApiLink: `Identity/Account/Login?ReturnUrl=%2F`,
  signOutViaApiLink: `${baseURL}signout`,
};
