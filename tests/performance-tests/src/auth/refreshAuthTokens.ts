import http from 'k6/http';
import { AuthTokens } from './getAuthTokens';

interface RefreshTokenParams {
  baseUrl: string;
  clientId: string;
  clientSecret: string;
  refreshToken: string;
}

export default function refreshAuthTokens({
  baseUrl,
  clientId,
  clientSecret,
  refreshToken,
}: RefreshTokenParams): AuthTokens | undefined {
  const requestBody = {
    client_id: clientId,
    client_secret: clientSecret,
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
  };

  const response = http.post(`${baseUrl}/connect/token`, requestBody);

  if (response.status !== 200) {
    console.log(
      `Unable to refresh access token.  Got response ${JSON.stringify(
        response.json(),
      )}`,
    );
    return undefined;
  }

  const json = (response.json() as unknown) as AuthTokens;

  return {
    access_token: json.access_token,
    refresh_token: json.refresh_token,
  };
}
