import { baseURL } from '@admin/services/util/service';

export default {
  signIn: '/authentication/login',
  signedOut: '/signed-out',
  signInViaApiLink: 'Identity/Account/Login?ReturnUrl=%2F',
  signOutViaApiLink: `${baseURL}signout`,
};
