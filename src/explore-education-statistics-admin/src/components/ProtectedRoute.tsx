import { useAuthContext } from '@admin/contexts/AuthContext';
import appendQuery from '@common/utils/url/appendQuery';
import React from 'react';
import { Redirect, Route, RouteProps } from 'react-router';
import { signInRoute } from '@admin/routes/routes';
import { GlobalPermissions } from '@admin/services/authService';
import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';

export interface ProtectedRouteProps extends RouteProps {
  path: string;
  protectionAction?: (permissions: GlobalPermissions) => boolean;
}

/**
 * Creates a <Route> that firstly checks the user's authentication
 * status and then renders the protected component if the user has been
 * successfully authorized, redirects the user to the sign-in page
 * if in need of authentication, or renders a Forbidden page if not
 * authorized.
 */
const ProtectedRoute = ({
  protectionAction = permissions => permissions.canAccessSystem,
  ...rest
}: ProtectedRouteProps) => {
  const { user } = useAuthContext();

  if (!user) {
    return (
      <Redirect
        to={appendQuery(signInRoute.path, {
          returnUrl: encodeURI(window.location.pathname),
        })}
      />
    );
  }

  if (!protectionAction(user.permissions)) {
    return <ForbiddenPage />;
  }

  return <Route {...rest} />;
};

export default ProtectedRoute;
