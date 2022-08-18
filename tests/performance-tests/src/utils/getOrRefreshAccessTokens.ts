import refreshAuthTokens from '../auth/refreshAuthTokens';
import { AuthTokens } from '../auth/getAuthDetails';

// Note that this would be best invoked from within httpClient.ts prior to any HTTP requests going out.  Currently
// it is invoked directly from the tests themselves.
export default function getOrRefreshAccessTokens(
  supportsRefreshTokens: boolean,
  userName: string,
  adminUrl: string,
  authTokens: AuthTokens,
): string {
  if (!supportsRefreshTokens) {
    return authTokens.accessToken;
  }

  const refreshedTokens = refreshAuthTokens({
    userName,
    adminUrl,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: authTokens.refreshToken,
    supportsRefreshTokens,
  });

  if (!refreshedTokens) {
    throw new Error('Unable to obtain an accessToken - exiting test');
  }

  return refreshedTokens.authTokens.accessToken;
}
