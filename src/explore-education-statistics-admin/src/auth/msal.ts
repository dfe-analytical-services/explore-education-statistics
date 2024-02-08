import {
  AuthenticationResult,
  ProtocolMode,
  PublicClientApplication,
} from '@azure/msal-browser';
import { Configuration } from '@azure/msal-browser/src/config/Configuration';
import { OidcConfig } from '@admin/config';
import { produce } from 'immer';
import logger from '@common/services/logger';

export interface PostLoginState {
  returnUrl?: string;
}

let msalInstance: PublicClientApplication;
let adminApiScope: string;

export async function createMsalInstance(
  config: OidcConfig,
): Promise<PublicClientApplication> {
  const msalConfig = produce(
    draft => {
      if (config.authorityMetadata) {
        draft.auth.authorityMetadata = JSON.stringify({
          authorization_endpoint:
            config.authorityMetadata.authorizationEndpoint,
          token_endpoint: config.authorityMetadata.tokenEndpoint,
          issuer: config.authorityMetadata.issuer,
          userinfo_endpoint: config.authorityMetadata.userInfoEndpoint,
        });
      }
    },
    {
      auth: {
        clientId: config.clientId,
        authority: config.authority,
        redirectUri: '/dashboard',
        postLogoutRedirectUri: '/signed-out',
        knownAuthorities: config.knownAuthorities,
        protocolMode: ProtocolMode.OIDC,
      },
      cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: false,
      },
      // A "loggerCallback" argument is available here if we wish to capture
      // detailed messages about MSAL lifecycle event processing.
    } as Configuration,
  )();

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
      redirectUri: '/dashboard',
      redirectStartPage: '/dashboard',
      state: JSON.stringify(postLoginState),
    })
    .catch(error => {
      logger.info(
        `Error encountered when redirecting to Identity Provider login - ${error}`,
      );
      logger.info('Returning to login page.');
      window.location.href = '/sign-in';
    });
}

export function handleLogout() {
  getMsalInstance()
    .logoutRedirect({
      account: getMsalInstance().getAllAccounts()[0],
      postLogoutRedirectUri: '/sign-in',
    })
    .then(() => {
      window.location.href = '/sign-in';
    })
    .catch(error => {
      logger.info(
        `Error encountered when processing post-redirection from Identity 
        Provider login - ${error}`,
      );
      logger.info('Returning to login page.');
      window.location.href = '/sign-in';
    });
}

export function acquireTokenSilent(): Promise<AuthenticationResult> {
  return getMsalInstance().acquireTokenSilent({
    scopes: [adminApiScope],
    account: msalInstance.getAllAccounts()[0],
  });
}
