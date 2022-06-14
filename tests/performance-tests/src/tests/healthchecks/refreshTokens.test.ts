/* eslint-disable camelcase */

import { check, fail } from 'k6';
import http from 'k6/http';
import { Rate } from 'k6/metrics';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import { AuthTokens } from '../../auth/getAuthTokens';

const BASE_URL = 'https://host.docker.internal:5021';

export const options = {
  noConnectionReuse: true,
  vus: 1,
  insecureSkipTLSVerify: true,
};

export const errorRate = new Rate('errors');

export function setup() {
  const tokenJson = __ENV.AUTH_TOKENS_AS_JSON as string;
  return JSON.parse(tokenJson) as AuthTokens;
}

export default function ({ access_token, refresh_token }: AuthTokens) {
  const responseWithOriginalAccessToken = http.get(`${BASE_URL}/api/themes`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${access_token}`,
    },
  });

  if (
    !check(responseWithOriginalAccessToken, {
      'response with original access token was 200': res => res.status === 200,
    })
  ) {
    fail('Failed to successfully use original access_token');
  }

  const refreshedTokens1 = refreshAuthTokens({
    baseUrl: BASE_URL,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: refresh_token,
  });

  if (
    !check(refreshedTokens1, {
      'response with refreshed tokens was successful': res => res != null,
    })
  ) {
    fail(
      'Failed to successfully refresh original access_token with first refresh_token',
    );
  }

  if (
    !check(refreshedTokens1!, {
      'response with refreshed tokens contained a new access_token': res =>
        res.access_token.length > 0,
      'response with refreshed tokens contained a new refresh_token': res =>
        res.refresh_token.length > 0,
    })
  ) {
    fail(
      'Failed to successfully get tokens back from initial refresh token response',
    );
  }

  const responseWithRefreshedAccessToken = http.get(`${BASE_URL}/api/themes`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${refreshedTokens1!.access_token}`, // or `Bearer ${clientAuthResp.access_token}`
    },
  });

  if (
    !check(responseWithRefreshedAccessToken, {
      'response with refreshed access token was 200': res => res.status === 200,
    })
  ) {
    fail('Failed to successfully use refreshed access_token');
  }

  const refreshedTokens2 = refreshAuthTokens({
    baseUrl: BASE_URL,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: refreshedTokens1!.refresh_token,
  });

  if (
    !check(refreshedTokens2, {
      'response with re-refreshed tokens was successful': res => res != null,
    })
  ) {
    fail(
      'Failed to successfully re-refresh access_token with second refresh_token',
    );
  }

  if (
    !check(refreshedTokens2!, {
      'response with re-refreshed tokens contained a new access_token': res =>
        res.access_token.length > 0,
      'response with re-refreshed tokens contained a new refresh_token': res =>
        res.refresh_token.length > 0,
    })
  ) {
    fail(
      'Failed to successfully get tokens back from second refresh token response',
    );
  }

  const responseWithRefreshedAccessToken2 = http.get(`${BASE_URL}/api/themes`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${refreshedTokens2!.access_token}`, // or `Bearer ${clientAuthResp.access_token}`
    },
  });

  if (
    !check(responseWithRefreshedAccessToken2, {
      'response with re-refreshed access token was 200': res =>
        res.status === 200,
    })
  ) {
    fail('Failed to successfully use re-refreshed access_token');
  }
}
