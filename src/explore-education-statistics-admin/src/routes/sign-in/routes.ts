import {baseURL} from "@admin/services/util/service";

export default {
  signIn: '/sign-in',
  signOut: '/signed-out',
  signInViaApiLink: `${baseURL}signin`,
  signOutViaApiLink: `${baseURL}signout`,
};
