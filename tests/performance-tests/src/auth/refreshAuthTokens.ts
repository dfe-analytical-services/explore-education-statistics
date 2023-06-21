/* eslint-disable no-console */
/* eslint-disable camelcase */
import http from 'k6/http';
import { AuthDetails } from './getAuthDetails';

interface RefreshTokenParams {
  userName: string;
  adminUrl: string;
  clientId: string;
  clientSecret: string;
  refreshToken: string;
  supportsRefreshTokens: boolean;
}

export default function refreshAuthTokens({
  userName,
  adminUrl,
  clientId,
  clientSecret,
  refreshToken,
  supportsRefreshTokens,
}: RefreshTokenParams): AuthDetails | undefined {
  if (!supportsRefreshTokens) {
    throw new Error(`Environment ${adminUrl} does not support refresh tokens`);
  }

  const requestBody = {
    client_id: clientId,
    client_secret: clientSecret,
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
  };

  const response = http.post(`${adminUrl}/connect/token`, requestBody);

  if (response.status !== 200) {
    console.log(
      `Unable to refresh access token. Got response ${response.json()}`,
    );
    return undefined;
  }

  const authTokens = (response.json() as unknown) as {
    access_token: string;
    refresh_token: string;
    expires_in: Date;
  };

  return {
    userName,
    authTokens: {
      accessToken: authTokens.access_token,
      refreshToken: authTokens.refresh_token,
      expiryDate: authTokens.expires_in,
    },
  };
}
