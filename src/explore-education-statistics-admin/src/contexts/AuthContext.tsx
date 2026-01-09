import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { useMsal } from '@azure/msal-react';
import authService, {
  GlobalPermissions,
  SignInResponse,
} from '@admin/services/authService';
import logger from '@common/services/logger';
import { acquireTokenSilent, PostLoginState } from '@admin/auth/msal';
import {
  expiredInviteRoute,
  noInvitationRoute,
  signInRoute,
} from '@admin/routes/routes';
import { useHistory, useLocation } from 'react-router';
import permissionService from '@admin/services/permissionService';

export interface User {
  id: string;
  name: string;
  permissions: GlobalPermissions;
}

export interface AuthContextState {
  user?: User;
}

export const AuthContext = createContext<AuthContextState | undefined>(
  undefined,
);

interface Props {
  children?: ReactNode;
  verboseLogging?: boolean;
}

interface State {
  /**
  This "readyToRenderChildren" flag controls whether this component is ready to render its children
  or not. This will be set to true only when this component has established
  that it is dealing with one of the following cases:

  1) No logged-in user at all (i.e. a user with no tokens in local storage),
  in which case the unauthenticated user can view pages that are visible to
  unauthenticated users and any children of this AuthContext that use
  "useAuth()" will be provided with a null "user". Any attempt to view a
  component within a ProtectedRoute after this will be redirected to the
  login page (by the ProtectedRoute itself).

  2) An authenticated user (i.e. a user with tokens in local storage) who has
  returned from a successful post-login redirect from the Identity Provider,
  has logged in or registered successfully with our local login /
  registration endpoint, and who has been redirected to the desired
  post-IdP-login page (as provided by the response from the Identity Provider
  if one was requested).

  3) An authenticated user (i.e. a user with tokens in local storage) who has
  loaded the SPA but not via a post-login redirect from the Identity
  Provider, and has logged in or registered successfully with our local login
  / registration endpoint. At this point, this flag is set to true, the user
  remains on the current route and will see the current page.

  4) An authenticated user (i.e. a user with tokens in local storage) who is
  unable to request a valid access token (generally as a result of the local
  access token expiring and the refresh token also expiring), and so is
  redirected to the login page, at which point this flag is set to true so
  that they can see the login page successfully.

  5) An authenticated user (i.e. a user with tokens in local storage) who is
  unable to successfully call the local login / registration endpoint, by not
  having a local user record and having either no invites or only expired
  invites. At this point they will be redirected to either the "No Invites"
  page or the "Expired Invites" page, and this flag will be set to true so
  that they can successfully see the page. */
  readyToRenderChildren: boolean;

  /**
   * The user profile and their permissions, if a user has successfully
   * logged in or registered using the local login / registration endpoint.
   */
  user?: User;

  /**
   * The post-login "state" object returned from the Identity Provider
   * post-login redirect. If the user has loaded the SPA via a redirect from the
   * Identity Provider, this "state" object will hold a "returnUrl" that we
   * requested be provided when the user originally clicked on the "Sign in"
   * button and was sent to the Identity Provider to log in.
   */
  loginRedirectState?: PostLoginState;

  /**
   * A path to redirect the user to prior to rendering the children of this
   * component, if any redirect is required.
   */
  redirect?: string;
}

export const AuthContextProvider = ({
  children,
  verboseLogging = false,
}: Props) => {
  const [state, setState] = useState<State>({
    readyToRenderChildren: false,
  });

  const {
    instance: msalInstance,
    accounts,
    inProgress: authenticationInProgress,
  } = useMsal();

  const loginInProgress = authenticationInProgress !== 'none';

  const history = useHistory();
  const location = useLocation();

  const log = useCallback(
    (message: string) => {
      if (verboseLogging) {
        logger.info(message);
      }
    },
    [verboseLogging],
  );

  // Register a listener for the post-login callback when returning from
  // the external Identity Provider login. This will allow us to access the
  // "state" object from the post-login response and extract the returnUrl (if
  // any) that we set during the loginRedirect() call in msal.ts.
  useEffect(() => {
    msalInstance.handleRedirectPromise().then(loginResult => {
      if (loginResult) {
        const loginRedirectState = loginResult.state
          ? (JSON.parse(loginResult.state) as PostLoginState)
          : undefined;
        if (loginRedirectState) {
          setState(previousState => ({
            ...previousState,
            loginRedirectState,
          }));
        }
      }
    });
    // We want to register this callback handler once and only once.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Check to see if we have an explicit request to redirect to a particular
  // route. This can occur if a login failure occurs or if login is successful
  // and an explicit redirect URL has been specified post-login.
  useEffect(() => {
    if (state.redirect) {
      log(
        `AuthContext: redirect to ${state.redirect} requested. Setting user ` +
          'ready to use service.',
      );
      setState(previousState => ({
        ...previousState,
        readyToRenderChildren: true,
      }));
      log(`AuthContext: redirecting to ${state.redirect}.`);
      history.push(state.redirect);
    }
  }, [history, state.redirect, log]);

  useEffect(() => {
    (async () => {
      function setUnauthenticated() {
        setState({
          user: undefined,
          readyToRenderChildren: true,
        });
      }

      function requestRedirect(path: string) {
        log(`AuthContext: Requesting that user is redirected to ${path}.`);
        setState(previousState => ({
          ...previousState,
          redirect: path,
        }));
      }

      function forceSignIn() {
        requestRedirect(signInRoute.path as string);
      }

      // If we're ready to allow the user (or no user) to continue to see pages
      // in the service, there is nothing more to do in this effect.
      if (state.readyToRenderChildren) {
        log(
          'AuthContext: User is already ready to view pages. Exiting out of ' +
            'effect.',
        );
        return;
      }

      // If MSAL is currently working on initialising or actively signing in the
      // user, wait until it's completed its initialisation and optionally
      // completed its user authentication flow, if a user currently is signed
      // in.
      if (loginInProgress) {
        log(
          'AuthContext: MSAL login or initialisation is in progress. ' +
            'Exiting out of effect until it is complete.',
        );
        return;
      }

      // If we are here, it means that MSAL has finished initialising, and
      // optionally signed in a user if one is logged in.
      //
      // If we have no active accounts at this point, post-initialisation, this
      // means that there is no user signed in. We therefore ensure that there
      // is no user in the AuthContext, and set "ready" to true so that children
      // can be rendered. If any of this component's children are using the
      // "useAuth()" hook, they will see at this point that there is no
      // logged-in user.
      if (!accounts.length || !Object.keys(accounts[0]).length) {
        log(
          'AuthContext: MSAL login or initialisation has completed and ' +
            'there is no logged-in user. Setting user as "none" and continuing.',
        );
        setUnauthenticated();
        return;
      }

      // If we are here, MSAL has finished initialising, and there is an active
      // user account present in local storage. We now attempt to silently
      // acquire a valid access token using the details in local storage.
      log('AuthContext: Acquiring access token silently for current user.');

      let authenticationResult;

      try {
        authenticationResult = await acquireTokenSilent();
        log(
          'AuthContext: Successfully retrieved access tokens for current user.',
        );
      } catch (error) {
        log(
          `AuthContext: Error whilst acquiring valid access token - ${error}` +
            'Redirecting to login page.',
        );
        forceSignIn();
        return;
      }

      // If we were unable to successfully acquire a valid access token for the
      // user, they will have to reauthenticate. We will therefore redirect them
      // to the login page.
      if (
        !authenticationResult.account ||
        !Object.keys(authenticationResult.account).length
      ) {
        log(
          'AuthContext: Unable to acquire a valid access token silently for ' +
            'current user.',
        );
        forceSignIn();
        return;
      }

      let signInResponse: SignInResponse;

      try {
        // If we are here, we have a successfully authenticated user from the
        // Identity Provider and valid access tokens.
        //
        // We now initiate a call to the local login / registration endpoint in
        // the Admin API to determine if the user is an existing or an invited
        // user, or on the other hand if they're a person with no invitations or
        // expired invitations.
        log('AuthContext: Calling internal login endpoint.');

        signInResponse = await authService.signIn();
      } catch (error) {
        log(
          'AuthContext: Error whilst calling internal login / registration ' +
            `endpoint - ${error}`,
        );
        log('Redirecting to login page.');
        forceSignIn();
        return;
      }

      log(
        `AuthContext: Local login / registration endpoint called with result ` +
          `"${signInResponse.loginResult}".`,
      );

      const { loginResult } = signInResponse;

      switch (loginResult) {
        // If the person logged in is not an existing user and has no invites,
        // redirect them to the "No Invite" page.
        case 'NoInvite':
          log(
            'AuthContext: User has no invites to use the service. Redirecting ' +
              'to "No Invites" page.',
          );
          requestRedirect(noInvitationRoute.path as string);
          return;
        case 'ExpiredInvite':
          log(
            'AuthContext: User has expired invite for the service. Redirecting ' +
              'to "Expired Invite" page.',
          );
          requestRedirect(expiredInviteRoute.path as string);
          return;
        // If we're here, the logged-in user is known to the service and is
        // ready to use it. Set the user details so that child components can
        // use them.
        default: {
          log(
            'AuthContext: User has valid credentials to use the service. Adding ' +
              'user details to the AuthContext state.',
          );
          const { userProfile } = signInResponse;
          const permissions = await permissionService.getGlobalPermissions();

          setState(previousState => ({
            ...previousState,
            user: userProfile && {
              id: userProfile.id,
              name: userProfile.firstName,
              permissions,
            },
          }));
          break;
        }
      }

      // If we've been given a post-login redirect from MSAL (as requested if
      // we sent the user to the Identity Provider to sign in), send them to
      // the appropriate redirect URL now if it is not the current URL.
      const loginRedirectUrl = state.loginRedirectState?.returnUrl;
      if (loginRedirectUrl && loginRedirectUrl !== location.pathname) {
        log(
          'AuthContext: Post-login redirect URL found from Identity Provider ' +
            'response and not on current URL. Redirecting user.',
        );
        requestRedirect(loginRedirectUrl);
        return;
      }

      // Otherwise the user can continue to stay on the URL that they are
      // currently on and see the page. We set the "ready" flag to true so
      // that this component will render its children. If any of those
      // children use the "useAuth()" hook, they will have details of the
      // logged-in user available. If they are ProtectedRoutes, they will be
      // able to use the permissions that were retrieved during the local
      // login / registration endpoint call to test whether the currently
      // logged-in user should be able to see those routes or not.
      log(
        'AuthContext: User can now view current page. Setting "ready" to true.',
      );
      setState(previousState => ({
        ...previousState,
        readyToRenderChildren: true,
      }));
    })();
  }, [
    msalInstance,
    accounts,
    loginInProgress,
    history,
    location,
    state.readyToRenderChildren,
    state.loginRedirectState,
    log,
  ]);

  const contextState: AuthContextState = useMemo(() => {
    return {
      user: state.user,
    };
  }, [state.user]);

  return state.readyToRenderChildren ? (
    <AuthContext.Provider value={contextState}>{children}</AuthContext.Provider>
  ) : null;
};

export function useAuthContext(): AuthContextState {
  const context = useContext(AuthContext);
  return context ?? {};
}

interface AuthContextTestProviderProps {
  children: ReactNode;
  user: User;
}

export function AuthContextTestProvider({
  children,
  user,
}: AuthContextTestProviderProps) {
  return (
    // eslint-disable-next-line react/jsx-no-constructed-context-values
    <AuthContext.Provider value={{ user }}>{children}</AuthContext.Provider>
  );
}
