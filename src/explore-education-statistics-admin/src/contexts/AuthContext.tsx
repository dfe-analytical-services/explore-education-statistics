import authService from '@admin/components/api-authorization/AuthorizeService';
import permissionService, {
  GlobalPermissions,
} from '@admin/services/permissionService';
import logger from '@common/services/logger';
import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

export interface User {
  id: string;
  name: string;
  validToken?: boolean;
  permissions: GlobalPermissions;
}

const defaultPermissions: GlobalPermissions = {
  canAccessSystem: false,
  canAccessPrereleasePages: false,
  canAccessAnalystPages: false,
  canAccessUserAdministrationPages: false,
  canAccessAllImports: false,
  canAccessMethodologyAdministrationPages: false,
  canManageAllTaxonomy: false,
};

export interface AuthContextState {
  user?: User;
}

export const AuthContext = createContext<AuthContextState | undefined>(
  undefined,
);

interface Props {
  children: ReactNode;
}

interface State {
  ready: boolean;
  user?: User;
}

export const AuthContextProvider = ({ children }: Props) => {
  const [state, setState] = useState<State>({ ready: false });

  useEffect(() => {
    const populateAuthenticationState = async () => {
      const authenticated = await authService.isAuthenticated();

      if (authenticated) {
        try {
          const userProfile = await authService.getUser();
          const { profile } = userProfile;
          const userId = profile.sub;

          const validToken =
            new Date() < new Date(userProfile.expires_at * 1000);

          const permissions = validToken
            ? await permissionService.getGlobalPermissions()
            : defaultPermissions;

          setState({
            ready: true,
            user: {
              id: userId,
              name: profile.given_name,
              validToken,
              permissions,
            },
          });
        } catch (err) {
          logger.error(err);

          setState({ ready: false });
        }
      } else {
        setState({ ready: true });
      }
    };

    let subscriptionId: number;

    populateAuthenticationState().then(() => {
      subscriptionId = authService.subscribe(async () => {
        await populateAuthenticationState();
      });
    });

    return () => {
      if (typeof subscriptionId !== 'undefined') {
        authService.unsubscribe(subscriptionId);
      }
    };
  }, []);

  const contextState: AuthContextState = useMemo(() => {
    return {
      user: state.user,
    };
  }, [state.user]);

  return state.ready ? (
    <AuthContext.Provider value={contextState}>{children}</AuthContext.Provider>
  ) : null;
};

export function useAuthContext(): AuthContextState {
  const context = useContext(AuthContext);
  return context ?? {};
}
