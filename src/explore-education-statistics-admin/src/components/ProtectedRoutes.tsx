import authService from '@admin/components/api-authorization/AuthorizeService';
import { LoginContext } from '@admin/components/Login';
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
      await authService
        .getUser()
        .then(userProfile => {
          const { profile } = userProfile;
          const user: User = {
            id: profile.sub,
            name: profile.given_name,
            permissions: profile.role
              ? (profile.role as string).split(',')
              : [],
            validToken: new Date() < new Date(userProfile.expires_at * 1000),
          };

          setAuthState({
            ready: true,
            user,
          });
        })
        .catch(() => {
          setAuthState({
            ready: false,
            user: undefined,
          });
        });
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

  const handleLoad = useCallback(async () => {
    const subscriptionId = authService.subscribe(
      handleAuthenticationStateChanged,
    );

    populateAuthenticationState();

    return () => {
      authService.unsubscribe(subscriptionId);
    };
  }, [populateAuthenticationState, handleAuthenticationStateChanged]);

  useEffect(() => {
    handleLoad();
  }, [handleLoad]);

  const authenticationContext: Authentication = {
    user: authState.user,
  };

  return authState.ready ? (
    <>
      <LoginContext.Provider value={authenticationContext}>
        {children}
      </LoginContext.Provider>
    </>
  ) : (
    <></>
  );
};

export default ProtectedRoutes;
