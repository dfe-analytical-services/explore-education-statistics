/* Code originally from the api-authorization folder from running
  "dotnet new react -o <output_directory_name> -au Individual"
*/
import React from 'react';
import { RouteProps } from 'react-router';
import { Dictionary } from '@admin/types';
import {
  ApplicationPaths,
  LoginActions,
  LogoutActions,
} from './ApiAuthorizationConstants';
import { Login } from './Login';
import { Logout } from './Logout';

function loginAction(name: string) {
  return <Login action={name} />;
}

function logoutAction(name: string) {
  return <Logout action={name} />;
}

const apiAuthorizationRouteList: Dictionary<RouteProps> = {
  login: {
    path: ApplicationPaths.Login,
    render: () => loginAction(LoginActions.Login),
  },
  loginFailed: {
    path: ApplicationPaths.LoginFailed,
    render: () => loginAction(LoginActions.LoginFailed),
  },
  loginCallback: {
    path: ApplicationPaths.LoginCallback,
    render: () => loginAction(LoginActions.LoginCallback),
  },
  profile: {
    path: ApplicationPaths.Profile,
    render: () => loginAction(LoginActions.Profile),
  },
  register: {
    path: ApplicationPaths.Register,
    render: () => loginAction(LoginActions.Register),
  },
  logOut: {
    path: ApplicationPaths.LogOut,
    render: () => logoutAction(LogoutActions.Logout),
  },
  logoutCallback: {
    path: ApplicationPaths.LogOutCallback,
    render: () => logoutAction(LogoutActions.LogoutCallback),
  },
  loggedOut: {
    path: ApplicationPaths.LoggedOut,
    render: () => logoutAction(LogoutActions.LoggedOut),
  },
};

export default apiAuthorizationRouteList;
