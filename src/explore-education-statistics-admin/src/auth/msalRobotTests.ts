/**
 *
 * Adds a way for the robot tests to get the access token and enable logging in.
 *
 * MSAL v5 encrypts all the access information.
 *
 * with this enabled, the access token is also pushed into local storage (unencrypted)
 * in the specified storage key.
 *
 * The robot tests will store this with the local storage information.
 *
 *
 */
import storageService from '@common/services/storageService';

const cachedAuthenticationStorageKey = 'robotTests';

const robotTestAccessEnabled =
  process.env.NODE_ENV === 'development' || process.env.NODE_ENV === 'test';

export async function cacheAuthenticationInformation(accessToken: string) {
  if (!robotTestAccessEnabled) {
    return;
  }

  await storageService.set(cachedAuthenticationStorageKey, accessToken);
}

export function clearCachedAuthenticationInformation() {
  if (!robotTestAccessEnabled) {
    return;
  }
  storageService.removeSync(cachedAuthenticationStorageKey);
}
