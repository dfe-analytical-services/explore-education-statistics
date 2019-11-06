import authService from '@admin/components/api-authorization/AuthorizeService';
import { LoginContext } from '@admin/components/Login';
import loginService from '@admin/services/sign-in/service';
import { Authentication, User } from '@admin/services/sign-in/types';
import React, { ReactNode, useCallback, useEffect, useState } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  ready: boolean;
  user?: User;
}

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

          const user: User = {
            id: userProfile.sub,
            name: userProfile.given_name,
            permissions: userProfile.role ? (userProfile.role as string).split(',') : [],
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
