import refreshAuthTokens from '../auth/refreshAuthTokens';
import { AuthTokens } from '../auth/getAuthTokens';

// Note that this would be best invoked from within httpClient.ts prior to any HTTP requests going out.  Currently
// it is invoked directly from the tests themselves.
export default function getOrRefreshAccessTokens(
  userName: string,
  authTokens: AuthTokens,
): string {
  const refreshedTokens = refreshAuthTokens({
    userName,
    refreshToken: authTokens.refreshToken,
  });

  if (!refreshedTokens) {
    throw new Error('Unable to obtain an accessToken - exiting test');
  }

  return refreshedTokens.authTokens.accessToken;
}
