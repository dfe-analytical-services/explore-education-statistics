import {LoginContext} from "@admin/components/Login";
import signInService from '@admin/services/sign-in/service';
import React, { useContext} from "react";
import {Redirect, Route, RouteProps} from "react-router";

interface ProtectedRouteProps extends RouteProps {
  redirectIfNotLoggedIn?: boolean;
}

const AuthenticationCheckingComponent = ({
  component,
  redirectIfNotLoggedIn,
  ...props
}: ProtectedRouteProps) => {

  const { user } = useContext(LoginContext);

  if (!component) {
    return null;
  }

  if (redirectIfNotLoggedIn && !user) {
    return <Redirect to={signInService.getSignInLink()} />;
  }

  return (
    <LoginContext.Provider value={{ user }}>
      {React.createElement(component, props)}
    </LoginContext.Provider>
  );
};

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
const ProtectedRoute = ({
  component,
  redirectIfNotLoggedIn = true,
  ...rest
}: ProtectedRouteProps) => {

  const routeComponent = (props: any) => (
    <AuthenticationCheckingComponent
      component={component}
      redirectIfNotLoggedIn={redirectIfNotLoggedIn}
      {...props}
    />
  );

  return <Route {...rest} component={routeComponent} />;
};

export default ProtectedRoute;