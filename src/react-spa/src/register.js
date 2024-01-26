import { themesAndTopicsConfig, graphConfig } from './authConfig';

/**
 * Attaches a given access token to a MS Graph API call. Returns information about the user
 * @param accessToken
 */
export async function callRegister(accessToken) {
  const headers = new Headers();
  const bearer = `Bearer ${accessToken}`;

  headers.append('Authorization', bearer);

  const options = {
    method: 'POST',
    headers: headers,
  };

  return fetch(themesAndTopicsConfig.registerEndpoint, options).catch(error =>
    console.log(error),
  );
}
