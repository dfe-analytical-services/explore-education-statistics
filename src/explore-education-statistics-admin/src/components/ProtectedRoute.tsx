import { LoginContext } from '@admin/components/Login';
import loginService, { Authentication } from '@admin/services/sign-in/service';
import React, { useEffect, useState } from 'react';
import { Redirect, Route, RouteProps } from 'react-router';

const AuthenticationCheckingComponent = ({
  component,
  location,
  ...props
}: RouteProps) => {
  const redirect = () => (
    <Redirect
      to={{
        pathname: '/sign-in',
        state: { from: location },
      }}
    />
  );

  const [authentication, setAuthentication] = useState<Authentication>();

  useEffect(() => {
    loginService
      .getUserDetails()
      .then(user => setAuthentication({ user }))
      .catch(_ => setAuthentication({}));
  }, []);

  if (!authentication) {
    return null;
  }

  if (!authentication.user || !component) {
    return redirect();
  }

  return (
    <LoginContext.Provider value={authentication}>
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
const ProtectedRoute = ({ component, location, ...rest }: RouteProps) => {
  const routeComponent = (props: any) => (
    <AuthenticationCheckingComponent
      component={component}
      location={location}
      {...props}
    />
  );

  return <Route {...rest} render={routeComponent} />;
};

export default ProtectedRoute;
