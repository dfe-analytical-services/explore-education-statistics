import http from 'k6/http';
import { AuthTokens } from './getAuthTokens';

interface RefreshTokenParams {
  url: string;
  clientId: string;
  clientSecret: string;
  refreshToken: string;
}

export default function refreshAuthTokens({
  url,
  clientId,
  clientSecret,
  refreshToken,
}: RefreshTokenParams): AuthTokens {
  const requestBody = {
    client_id: clientId,
    client_secret: clientSecret,
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
  };

  const response = http.post(url, requestBody);

  const json = (response.json() as unknown) as AuthTokens;

  return {
    access_token: json.access_token,
    refresh_token: json.refresh_token,
  };
}
