import { QueryParameterNames } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useErrorControl } from '@admin/contexts/ErrorControlContext';
import signInService from '@admin/services/sign-in/service';
import { User } from '@admin/services/sign-in/types';
import React from 'react';
import { Redirect, Route, RouteProps } from 'react-router';

interface ProtectedRouteProps extends RouteProps {
  allowAnonymousUsers?: boolean;
  protectionAction?: (user: User) => boolean;
}

const basicAccessCheck = (user: User) => user.permissions.canAccessSystem;

/**
 * Creates a <Route> that firstly checks the user's authentication
 * status and then renders the protected component if the user has been
 * successfully authorized, redirects the user to the sign-in page
 * if in need of authentication, or renders a Forbidden page if not
 * authorized.
 */
const ProtectedRoute = ({
  component,
  allowAnonymousUsers = false,
  protectionAction,
  ...rest
}: ProtectedRouteProps) => {
  const { user } = useAuthContext();

  const { handleManualErrors } = useErrorControl();

  let accessDenied = false;

  if (user) {
    const accessCheck = protectionAction || basicAccessCheck;
    accessDenied = !accessCheck(user);
  } else {
    const denyAccessToNonLoggedInUsers = !allowAnonymousUsers;
    accessDenied = denyAccessToNonLoggedInUsers;
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

  if (accessDenied) {
    handleManualErrors.forbidden();
    return null;
  }

  return <Route component={component} {...rest} />;
};

export default ProtectedRoute;
