import http from 'k6/http';
import { AuthDetails } from './getAuthDetails';

interface RefreshTokenParams {
  userName: string;
  adminUrl: string;
  clientId: string;
  clientSecret: string;
  refreshToken: string;
}

export default function refreshAuthTokens({
  userName,
  adminUrl,
  clientId,
  clientSecret,
  refreshToken,
}: RefreshTokenParams): AuthDetails | undefined {
  const requestBody = {
    client_id: clientId,
    client_secret: clientSecret,
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
  };

  const response = http.post(`${adminUrl}/connect/token`, requestBody);

  if (response.status !== 200) {
    /* eslint-disable-next-line no-console */
    console.log(
      `Unable to refresh access token. Got response ${response.json()}`,
    );
    return undefined;
  }

  /* eslint-disable camelcase */
  const authTokens = (response.json() as unknown) as {
    access_token: string;
    refresh_token: string;
    expires_in: Date;
  };
  /* eslint-enable camelcase */

  return {
    adminUrl,
    userName,
    authTokens: {
      accessToken: authTokens.access_token,
      refreshToken: authTokens.refresh_token,
      expiryDate: authTokens.expires_in,
    },
  };
}
