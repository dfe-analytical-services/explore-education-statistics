import authService from '@admin/components/api-authorization/AuthorizeService';
import permissionService from '@admin/services/permissions/service';
import { Authentication, User } from '@admin/services/sign-in/types';
import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

const AuthContext = createContext<Authentication>({
  user: undefined,
});

interface Props {
  children: ReactNode;
}

interface State {
  ready: boolean;
  user?: User;
}

export const AuthContextProvider = ({ children }: Props) => {
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
        const validToken = new Date() < new Date(userProfile.expires_at * 1000);

        const permissions = validToken
          ? await permissionService.getGlobalPermissions()
          : {
            canAccessSystem: false,
            canAccessPrereleasePages: false,
            canAccessAnalystPages: false,
            canAccessUserAdministrationPages: false,
            canAccessMethodologyAdministrationPages: false,
          };

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

  useEffect(() => {
    const subscriptionId = authService.subscribe(() => {
      setAuthState({
        ready: false,
        user: undefined,
      });

      populateAuthenticationState();
    });

    populateAuthenticationState();

    return () => {
      authService.unsubscribe(subscriptionId);
    };
  }, [populateAuthenticationState]);

  const loginContext: Authentication = useMemo(
    () => ({
      user: authState.user,
    }),
    [authState.user],
  );

  return authState.ready ? (
    <AuthContext.Provider value={loginContext}>
      {children}
    </AuthContext.Provider>
  ) : null;
};

export function useAuthContext() {
  return useContext(AuthContext);
}
