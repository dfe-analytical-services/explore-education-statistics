import React from 'react';
import { QueryParameterNames } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import { useAuthContext, User } from '@admin/contexts/AuthContext';
import signInService from '@admin/services/loginService';
import appendQuery from '@common/utils/url/appendQuery';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { Redirect, Route, RouteProps } from 'react-router';

export interface ProtectedRouteProps extends RouteProps {
  allowAnonymousUsers?: boolean;
  path: string;
  protectionAction?: (user: User) => boolean;
}

/**
 * Creates a <Route> that firstly checks the user's authentication
 * status and then renders the protected component if the user has been
 * successfully authorized, redirects the user to the sign-in page
 * if in need of authentication, or renders a Forbidden page if not
 * authorized.
 */
const ProtectedRoute = ({
  allowAnonymousUsers = false,
  protectionAction,
  ...rest
}: ProtectedRouteProps) => {
  const { user } = useAuthContext();

  const { errorPages } = useErrorControl();

  let hasAccess = allowAnonymousUsers;

  if (user) {
    hasAccess = protectionAction
      ? protectionAction(user)
      : user.permissions.canAccessSystem;
  }

  if (!allowAnonymousUsers && !user?.validToken) {
    return (
      <Redirect
        to={appendQuery(signInService.getSignInLink(), {
          [QueryParameterNames.ReturnUrl]: encodeURI(window.location.href),
        })}
      />
    );
  }

  if (!hasAccess) {
    errorPages.forbidden();
    return null;
  }

  return <Route {...rest} />;
};

export default ProtectedRoute;
