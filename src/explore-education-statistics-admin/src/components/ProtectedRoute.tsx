import { QueryParameterNames } from '@admin/components/api-authorization/ApiAuthorizationConstants';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { ErrorControlContext } from '@admin/contexts/ErrorControlContext';
import signInService from '@admin/services/sign-in/service';
import { User } from '@admin/services/sign-in/types';
import React, { useContext } from 'react';
import { Redirect, Route, RouteComponentProps, RouteProps } from 'react-router';

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
  const { user } = useAuthContext();

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
 */
const ProtectedRoute = ({
  component,
  allowAnonymousUsers = false,
  protectionAction,
  ...rest
}: ProtectedRouteProps) => {
  return (
    <Route
      {...rest}
      render={(props: RouteComponentProps) => (
        <AuthenticationCheckingComponent
          {...props}
          component={component}
          allowAnonymousUsers={allowAnonymousUsers}
          protectionAction={protectionAction}
        />
      )}
    />
  );
};

export default ProtectedRoute;
