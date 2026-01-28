/* eslint-disable no-console */
/* eslint-disable camelcase */
import http from 'k6/http';
import { AuthDetails } from './getAuthTokens';
import getEnvironmentAndUsersFromFile from '../utils/environmentAndUsers';

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

interface RefreshTokenParams {
  userName: string;
  refreshToken: string;
}

export default function refreshAuthTokens({
  userName,
  refreshToken,
}: RefreshTokenParams): AuthDetails | undefined {
  const { openIdConnect, adminUrl } = environmentAndUsers.environment;
  const { refreshTokenUrl, clientId } = openIdConnect;

  const requestBody = {
    client_id: clientId,
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
  };

  const params = {
    headers: {
      Origin: adminUrl,
    },
  };

  const response = http.post(refreshTokenUrl, requestBody, params);

  if (response.status !== 200) {
    console.log(
      `Unable to refresh access token. Got response ${JSON.stringify(
        response.json(),
      )}`,
    );
    return undefined;
  }

  const authTokens = response.json() as unknown as {
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
