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
      `Unable to refresh access token. Got response ${JSON.stringify(
        response.json(),
      )}`,
    );
    return undefined;
  }

  const json = (response.json() as unknown) as AuthDetails;

  return {
    adminUrl,
    userName,
    authTokens: {
      accessToken: json.authTokens.accessToken,
      refreshToken: json.authTokens.refreshToken,
      expiryDate: new Date(),
    },
  };
}
