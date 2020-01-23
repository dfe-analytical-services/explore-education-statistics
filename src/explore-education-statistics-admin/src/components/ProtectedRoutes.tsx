import authService from '@admin/components/api-authorization/AuthorizeService';
import permissionService from '@admin/services/permissions/service';
import LoginContext from '@admin/components/Login';
import { Authentication, User } from '@admin/services/sign-in/types';
import React, { ReactNode, useCallback, useEffect, useState } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  ready: boolean;
  user?: User;
}

/**
 * A component that surrounds all authentication-aware Routes and provides the logged-in user details
 *
 * @param children
 * @constructor
 */
const ProtectedRoutes = ({ children }: Props) => {
  const [authState, setAuthState] = useState<State>({
    ready: false,
    user: undefined,
  });

  const populateAuthenticationState = useCallback(async () => {
    const authenticated = await authService.isAuthenticated();

    if (authenticated) {
      try {
        const userProfile = await authService.getUser();
        const { profile } = userProfile;
        const userId = profile.sub;
        const permissions = await permissionService.getGlobalPermissions();

        const user: User = {
          id: userId,
          name: profile.given_name,
          permissions,
          validToken: new Date() < new Date(userProfile.expires_at * 1000),
        };

        setAuthState({
          ready: true,
          user,
        });
      } catch (_) {
        setAuthState({
          ready: false,
          user: undefined,
        });
      }
    } else {
      setAuthState({
        ready: true,
        user: undefined,
      });
    }
  }, []);

  const handleAuthenticationStateChanged = useCallback(() => {
    setAuthState({
      ready: false,
      user: undefined,
    });
    populateAuthenticationState();
  }, [populateAuthenticationState]);

  useEffect(() => {
    const subscriptionId = authService.subscribe(
      handleAuthenticationStateChanged,
    );

    populateAuthenticationState();

    return () => {
      authService.unsubscribe(subscriptionId);
    };
  }, [populateAuthenticationState, handleAuthenticationStateChanged]);

  const authenticationContext: Authentication = {
    user: authState.user,
  };

  return authState.ready ? (
    <LoginContext.Provider value={authenticationContext}>
      {children}
    </LoginContext.Provider>
  ) : null;
};

export default ProtectedRoutes;
