import {
  AuthenticationResult,
  Configuration,
  ProtocolMode,
  PublicClientApplication,
  LogLevel,
} from '@azure/msal-browser';
import { OidcConfig } from '@admin/config';
import logger from '@common/services/logger';
import {
  dashboardRoute,
  signInRoute,
  signedOutRoute,
} from '@admin/routes/routes';

const msalLoggingEnabled = false;

export interface PostLoginState {
  returnUrl?: string;
}

let msalInstance: PublicClientApplication;
let adminApiScope: string;

const robotTestAccessTokenStorageKey = 'robotTests.accessToken';

export function getRobotTestAccessToken(): string | undefined {
  if (process.env.APP_ENV === 'Production') {
    return undefined;
  }

  const localStorageItem =
    window.localStorage.getItem(robotTestAccessTokenStorageKey) ?? undefined;
  if (!localStorageItem) return undefined;
  try {
    const parsed = JSON.parse(localStorageItem) as { accessToken?: string };

    return parsed.accessToken;
  } catch (_) {
    return undefined;
  }
}

function cacheRobotTestAccessToken(accessToken: string) {
  if (!accessToken) {
    return;
  }

  // Keep this disabled in Production. If you have a more specific robot-test
  // config flag, use that here instead.
  if (process.env.APP_ENV === 'Production') {
    return;
  }

  window.localStorage.setItem(
    robotTestAccessTokenStorageKey,
    JSON.stringify({ accessToken }),
  );
}

function clearRobotTestAccesToken() {
  window.localStorage.removeItem(robotTestAccessTokenStorageKey);
}

export async function createMsalInstance(
  config: OidcConfig,
): Promise<PublicClientApplication> {
  const msalConfig: Configuration = {
    auth: {
      clientId: config.clientId,
      authority: config.authority,
      redirectUri: '/redirect', // TODO: get the config site url, going to need to pass the overall config object
      postLogoutRedirectUri: signedOutRoute.fullPath,
      knownAuthorities: config.knownAuthorities,
      authorityMetadata:
        config.authorityMetadata &&
        JSON.stringify({
          authorization_endpoint:
            config.authorityMetadata.authorizationEndpoint,
          token_endpoint: config.authorityMetadata.tokenEndpoint,
          issuer: config.authorityMetadata.issuer,
          userinfo_endpoint: config.authorityMetadata.userInfoEndpoint,
          end_session_endpoint: config.authorityMetadata.endSessionEndpoint,
        }),
    },
    cache: {
      cacheLocation: 'localStorage',
    },
    system: {
      protocolMode: ProtocolMode.OIDC,
      loggerOptions: {
        logLevel: LogLevel.Verbose,
        loggerCallback: (level, message, containsPii) => {
          if (!msalLoggingEnabled || containsPii) {
            return;
          }
          switch (level) {
            case LogLevel.Trace || LogLevel.Verbose:
              logger.debug(message);
              break;
            case LogLevel.Info:
              logger.info(message);
              break;
            case LogLevel.Warning:
              logger.warn(message);
              break;
            case LogLevel.Error:
              logger.error(message);
              break;
            default:
          }
        },
        piiLoggingEnabled: false,
      },
    },
  };

  adminApiScope = config.adminApiScope;
  msalInstance = new PublicClientApplication(msalConfig);

  await msalInstance.initialize();
  return msalInstance;
}

export function getMsalInstance() {
  if (!msalInstance) {
    throw new Error(
      'msalInstance is not yet created. Ensure it is being used ' +
        'within the scope of ConfiguredMsalProvider',
    );
  }
  return msalInstance;
}

export function handleLogin(returnUrl?: string) {
  const postLoginState: PostLoginState = {
    returnUrl,
  };

  getMsalInstance()
    .loginRedirect({
      scopes: [adminApiScope],
      redirectUri: '/redirect', // TODO: Get the config site url
      redirectStartPage: dashboardRoute.fullPath,
      state: JSON.stringify(postLoginState),
    })
    .catch(error => {
      logger.info(
        `Error encountered when redirecting to Identity Provider login - ${error}`,
      );
      logger.info('Returning to login page.');
      window.location.href = signInRoute.fullPath;
    });
}

export function handleLogout() {
  // if we don't we'd be logged in constantly
  clearRobotTestAccesToken();

  getMsalInstance()
    .logoutRedirect({
      idTokenHint: getMsalInstance().getAllAccounts()[0].idToken,
      account: getMsalInstance().getAllAccounts()[0],
      postLogoutRedirectUri: signedOutRoute.fullPath,
    })
    .catch(error => {
      logger.info(
        `Error encountered when processing post-redirection from Identity 
        Provider login - ${error}`,
      );
      logger.info('Returning to login page.');
      window.location.href = signInRoute.fullPath;
    });
}

export function acquireTokenSilent(): Promise<AuthenticationResult> {
  const instance = getMsalInstance();
  const accounts = instance.getAllAccounts();

  if (!accounts || accounts.length === 0) {
    // only do this if there is no one logged in, as we're going to set it later
    const robotTestAccessToken = getRobotTestAccessToken();

    if (robotTestAccessToken) {
      return Promise.resolve({
        accessToken: robotTestAccessToken,
        account: {
          homeAccountId: 'robot-tests',
          environment: 'robot-tests',
          tenantId: 'robot-tests',
          username: 'robot-tests',
          localAccountId: 'robot-tests',
          name: 'Robot Tests',
        },
        authority: '',
        uniqueId: 'robot-tests',
        tenantId: 'robot-tests',
        scopes: [adminApiScope],
        idToken: '',
        idTokenClaims: {},
        fromCache: true,
        expiresOn: null,
        tokenType: 'Bearer',
        correlationId: '',
      });
    }

    handleLogin(window.location.pathname);
    return Promise.reject(new Error('No account available'));
  }

  return instance
    .acquireTokenSilent({
      scopes: [adminApiScope],
      account: accounts[0],
    })
    .then(authenticationResult => {
      cacheRobotTestAccessToken(authenticationResult.accessToken);
      return authenticationResult;
    });
}
