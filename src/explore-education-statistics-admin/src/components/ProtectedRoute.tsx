import { QueryParameterNames } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import ErrorBoundary, {
  ErrorControlContext,
} from '@admin/components/ErrorBoundary';
import LoginContext from '@admin/components/Login';
import signInService from '@admin/services/sign-in/service';
import { User } from '@admin/services/sign-in/types';
import React, { useContext } from 'react';
import { Redirect, Route, RouteProps } from 'react-router';
import ProtectedRoutes from './ProtectedRoutes';

interface ProtectedRouteProps extends RouteProps {
  allowAnonymousUsers?: boolean;
  protectionAction?: (user: User) => boolean;
}

const basicAccessCheck = (user: User) => user.permissions.canAccessSystem;

const AuthenticationCheckingComponent = ({
  component,
  allowAnonymousUsers = false,
  protectionAction,
  ...props
}: ProtectedRouteProps) => {
  const { user } = useContext(LoginContext);

  const { handleManualErrors } = useContext(ErrorControlContext);

  let protectedByAction = false;

  if (user) {
    const accessCheck = protectionAction || basicAccessCheck;
    protectedByAction = !accessCheck(user);
  } else {
    const denyAccessToNonLoggedInUsers = !allowAnonymousUsers;
    protectedByAction = denyAccessToNonLoggedInUsers;
  }

  if (!component) {
    return null;
  }

  if (!allowAnonymousUsers && (!user || user.validToken === false)) {
    return (
      <Redirect
        to={`${signInService.getSignInLink()}?${
          QueryParameterNames.ReturnUrl
        }=${encodeURI(window.location.href)}`}
      />
    );
  }

  if (typeof protectedByAction !== 'undefined' && protectedByAction) {
    handleManualErrors.forbidden();
    return null;
  }

  if (typeof protectedByAction !== 'undefined' && !protectedByAction) {
    return React.createElement(component, props);
  }

  return null;
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
  allowAnonymousUsers = false,
  protectionAction,
  ...rest
}: ProtectedRouteProps) => {
  const routeComponent = (props: any) => (
    <ProtectedRoutes>
      <ErrorBoundary>
        <AuthenticationCheckingComponent
          component={component}
          allowAnonymousUsers={allowAnonymousUsers}
          protectionAction={protectionAction}
          {...props}
        />
      </ErrorBoundary>
    </ProtectedRoutes>
  );

  return <Route {...rest} component={routeComponent} />;
};

export default ProtectedRoute;
