/* eslint-disable camelcase */
import { check, fail } from 'k6';
import http from 'k6/http';
import { Rate } from 'k6/metrics';
import { Options } from 'k6/options';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';

export const options: Options = {
  noConnectionReuse: true,
  vus: 1,
  insecureSkipTLSVerify: true,
};

export const errorRate = new Rate('errors');

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);
const { adminUrl } = environmentAndUsers.environment;

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
const { authTokens, userName } = environmentAndUsers.users.find(
  user => user.userName === 'bau1',
)!;

const performTest = () => {
  const responseWithOriginalAccessToken = http.get(`${adminUrl}/api/themes`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${authTokens.accessToken}`,
    },
  });

  if (
    !check(responseWithOriginalAccessToken, {
      'response with original access token was 200': res => res.status === 200,
    })
  ) {
    fail('Failed to successfully use original accessToken');
  }

  const refreshedTokens1 = refreshAuthTokens({
    userName,
    adminUrl,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: authTokens.refreshToken,
    supportsRefreshTokens: true,
  });

  if (
    !check(refreshedTokens1, {
      'response with refreshed tokens was successful': tokens => !!tokens,
    })
  ) {
    fail(
      'Failed to successfully refresh original accessToken with first refreshToken',
    );
  }

  if (!refreshedTokens1) {
    return;
  }

  if (
    !check(refreshedTokens1, {
      'response with refreshed tokens contained a new accessToken': tokens =>
        tokens.authTokens.accessToken.length > 0,
      'response with refreshed tokens contained a new refreshToken': tokens =>
        tokens.authTokens.refreshToken.length > 0,
    })
  ) {
    fail(
      'Failed to successfully get tokens back from initial refresh token response',
    );
  }

  const responseWithRefreshedAccessToken = http.get(`${adminUrl}/api/themes`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${refreshedTokens1.authTokens.accessToken}`,
    },
  });

  if (
    !check(responseWithRefreshedAccessToken, {
      'response with refreshed access token was 200': res => res.status === 200,
    })
  ) {
    fail('Failed to successfully use refreshed accessToken');
  }

  const refreshedTokens2 = refreshAuthTokens({
    userName,
    adminUrl,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: refreshedTokens1.authTokens.refreshToken,
    supportsRefreshTokens: true,
  });

  if (
    !check(refreshedTokens2, {
      'response with re-refreshed tokens was successful': tokens => !!tokens,
    })
  ) {
    fail(
      'Failed to successfully re-refresh accessToken with second refreshToken',
    );
  }

  if (!refreshedTokens2) {
    return;
  }

  if (
    !check(refreshedTokens2, {
      'response with re-refreshed tokens contained a new accessToken': tokens =>
        tokens.authTokens.accessToken.length > 0,
      'response with re-refreshed tokens contained a new refreshToken': tokens =>
        tokens.authTokens.refreshToken.length > 0,
    })
  ) {
    fail(
      'Failed to successfully get tokens back from second refresh token response',
    );
  }

  const responseWithRefreshedAccessToken2 = http.get(`${adminUrl}/api/themes`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${refreshedTokens2.authTokens.accessToken}`, // or `Bearer ${clientAuthResp.accessToken}`
    },
  });

  if (
    !check(responseWithRefreshedAccessToken2, {
      'response with re-refreshed access token was 200': res =>
        res.status === 200,
    })
  ) {
    fail('Failed to successfully use re-refreshed accessToken');
  }
};

export default performTest;
