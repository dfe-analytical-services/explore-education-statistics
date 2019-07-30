import {LoginContext} from "@admin/components/Login";
import loginService from "@admin/services/sign-in/service";
import React from 'react';
import {useCookies} from 'react-cookie';
import {Redirect, Route, RouteProps} from 'react-router';

/**
 * Creates a <Route> that firstly checks the user's authentication
 * status and then renders the protected component if the user has been
 * successfully authenticated, or redirects the user to the sign-in page
 * if in need of authentication.
 *
 * @param component
 * @param rest
 * @constructor
 */
/* eslint-disable @typescript-eslint/no-explicit-any */
const ProtectedRoute = ({component, location, ...rest}: RouteProps) => {

  const [cookies] = useCookies(['DFEUserDetails']);

  const authCookie = cookies.DFEUserDetails;

  function createProtectedComponent(props: any) {
    return component && (
      <LoginContext.Provider value={loginService.setLoggedInUser(authCookie)}>
        {React.createElement(component, props)}
      </LoginContext.Provider>
    );
  }

  const redirect = (
    <Redirect
      to={{
        pathname: '/sign-in',
        state: { from: location },
      }}
    />
  );

  const routeComponent = (props: any) => (
    authCookie
      ? createProtectedComponent(props)
      : redirect
  );

  return (
    <Route
      {...rest}
      render={routeComponent}
    />
  );
};

export default ProtectedRoute;