import authService from '@admin/components/api-authorization/AuthorizeService';
import { LoginContext } from '@admin/components/Login';
import loginService from '@admin/services/sign-in/service';
import { Authentication, User } from '@admin/services/sign-in/types';
import React, { ReactNode, useEffect, useState } from 'react';

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

  const onLoad = async () => {
    const subscriptionId = authService.subscribe(() =>
      setAuthState({
        ready: false,
        user: undefined,
      }),
    );

    const authenticated = await authService.isAuthenticated();

    if (authenticated) {
      await loginService
        .getUserDetails()
        .then(user => {
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

    return () => {
      authService.unsubscribe(subscriptionId);
    };
  };

  useEffect(() => {
    onLoad();
  }, []);

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
    <>Hey there</>
  );
};

export default ProtectedRoutes;
