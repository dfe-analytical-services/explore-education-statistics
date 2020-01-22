import { QueryParameterNames } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import ErrorBoundary, {
  ErrorControlContext,
} from '@admin/components/ErrorBoundary';
import LoginContext from '@admin/components/Login';
import signInService from '@admin/services/sign-in/service';
import permissionService from '@admin/services/permissions/service';
import React, { useContext, useEffect, useState } from 'react';
import { Redirect, Route, RouteProps } from 'react-router';
import ProtectedRoutes from './ProtectedRoutes';

interface ProtectedRouteProps extends RouteProps {
  allowAnonymousUsers?: boolean;
  protectionAction?: () => Promise<boolean>;
}

const basicAccessCheck = () => permissionService.canAccessSystem();

const AuthenticationCheckingComponent = ({
  component,
  allowAnonymousUsers = false,
  protectionAction,
  ...props
}: ProtectedRouteProps) => {
  const { user } = useContext(LoginContext);

  const { handleApiErrors, handleManualErrors } = useContext(
    ErrorControlContext,
  );

  const [protectedByAction, setProtectedByAction] = useState<boolean>();

  useEffect(() => {
    if (user) {
      const accessCheck = protectionAction || basicAccessCheck;

      accessCheck()
        .then(result => setProtectedByAction(!result))
        .catch(handleApiErrors);
    } else {
      const denyAccessToNonLoggedInUsers = !allowAnonymousUsers;
      setProtectedByAction(denyAccessToNonLoggedInUsers);
    }
  }, [protectionAction, handleApiErrors, allowAnonymousUsers, user]);

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
