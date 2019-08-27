import {baseURL} from "@admin/services/util/service";

export default {
  signIn: '/sign-in',
  signOut: '/sign-out',
  signInViaApiLink: `${baseURL}/signin`,
  signOutViaApiLink: `${baseURL}/signout`,
};
