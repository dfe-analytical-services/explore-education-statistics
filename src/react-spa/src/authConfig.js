/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { LogLevel, ProtocolMode } from '@azure/msal-browser';

/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 */

const realm = 'https://ees.local:5031/auth/realms/ees-realm';

export const msalConfig = {
  auth: {
    clientId: 'ees-admin-client-new',
    authority: realm,
    redirectUri: 'http://localhost:3000',
    postLogoutRedirectUri: 'http://localhost:3000',
    knownAuthorities: [realm],
    protocolMode: ProtocolMode.OIDC,
    authorityMetadata: JSON.stringify({
      authorization_endpoint: `${realm}/protocol/openid-connect/auth`,
      token_endpoint: `${realm}/protocol/openid-connect/token`,
      issuer: realm,
      userinfo_endpoint: `${realm}/protocol/openid-connect/userinfo`,
    }),
  },
  cache: {
    cacheLocation: 'sessionStorage', // This configures where your cache will be stored
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Info:
            console.info(message);
            return;
          case LogLevel.Verbose:
            console.debug(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
          default:
            return;
        }
      },
    },
  },
};

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit:
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const initialLoginRequest = {
  scopes: ['access-admin-api'],
};

export const adminApiLoginRequest = {
  scopes: ['access-admin-api'],
};

export const themesAndTopicsConfig = {
  themesEndpoint: 'https://localhost:5021/api/themes',
  registerEndpoint: 'https://localhost:5021/api/sign-in',
};
