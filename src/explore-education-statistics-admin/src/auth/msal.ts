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

export async function createMsalInstance(
  config: OidcConfig,
): Promise<PublicClientApplication> {
  const msalConfig: Configuration = {
    auth: {
      clientId: config.clientId,
      authority: config.authority,
      redirectUri: dashboardRoute.path,
      postLogoutRedirectUri: signedOutRoute.path,
      knownAuthorities: config.knownAuthorities,
      protocolMode: ProtocolMode.OIDC,
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
      storeAuthStateInCookie: false,
    },
    system: {
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
      redirectUri: dashboardRoute.path,
      redirectStartPage: dashboardRoute.path,
      state: JSON.stringify(postLoginState),
    })
    .catch(error => {
      logger.info(
        `Error encountered when redirecting to Identity Provider login - ${error}`,
      );
      logger.info('Returning to login page.');
      window.location.href = signInRoute.path;
    });
}

export function handleLogout() {
  getMsalInstance()
    .logoutRedirect({
      account: getMsalInstance().getAllAccounts()[0],
      postLogoutRedirectUri: signedOutRoute.path,
    })
    .catch(error => {
      logger.info(
        `Error encountered when processing post-redirection from Identity 
        Provider login - ${error}`,
      );
      logger.info('Returning to login page.');
      window.location.href = signInRoute.path;
    });
}

export function acquireTokenSilent(): Promise<AuthenticationResult> {
  const instance = getMsalInstance();
  const accounts = instance.getAllAccounts();

  if (!accounts || accounts.length === 0) {
    handleLogin(window.location.pathname);
    return Promise.reject(new Error('No account available'));
  }

  return instance.acquireTokenSilent({
    scopes: [adminApiScope],
    account: accounts[0],
  });
}
