import { useAuthContext } from '@admin/contexts/AuthContext';
import appendQuery from '@common/utils/url/appendQuery';
import React, { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router';
import { signInRoute } from '@admin/routes/routes';
import { GlobalPermissions } from '@admin/services/authService';
import ForbiddenPage from '@admin/pages/errors/ForbiddenPage';

interface ProtectedRouteComponentProps {
  protectionAction?: (permissions: GlobalPermissions) => boolean;
  children: ReactNode;
}

/**
 * Creates a <Route> that first checks the user's authentication
 * status and then renders the protected component if the user has been
 * successfully authorized, redirects the user to the sign-in page
 * if in need of authentication, or renders a Forbidden page if not
 * authorized.
 */
const ProtectedRoute = ({
  protectionAction = permissions => permissions.canAccessSystem,
  children,
}: ProtectedRouteComponentProps) => {
  const { user } = useAuthContext();
  const location = useLocation();

  if (!user) {
    return (
      <Navigate
        replace
        to={appendQuery(signInRoute.fullPath, {
          returnUrl: encodeURI(`${location.pathname}${location.search}`),
        })}
      />
    );
  }

  if (!protectionAction(user.permissions)) {
    return <ForbiddenPage />;
  }

  return children;
};

export default ProtectedRoute;
