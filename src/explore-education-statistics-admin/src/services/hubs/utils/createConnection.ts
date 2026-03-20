import { exponentialBackoffPolicy } from '@admin/services/hubs/utils/retryPolicies';
import logger from '@common/services/logger';
import {
  HubConnection,
  HubConnectionBuilder,
  IRetryPolicy,
} from '@microsoft/signalr';
import { acquireTokenSilent } from '@admin/auth/msal';

export interface CreateConnectionOptions {
  accessToken?: () => string | Promise<string>;
  /**
   * Defaults to exponential backoff policy.
   */
  retryPolicy?: IRetryPolicy;
}

export default function createConnection(
  url: string,
  options: CreateConnectionOptions = {},
): HubConnection {
  const {
    accessToken = async () => {
      try {
        const authenticationResult = await acquireTokenSilent();
        return authenticationResult.accessToken;
      } catch (error) {
        logger.info(
          `createConnection: failed to retrieve access token - ${error}`,
        );
        return '';
      }
    },
    retryPolicy = exponentialBackoffPolicy(),
  } = options;

  return new HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: accessToken,
    })
    .withAutomaticReconnect(retryPolicy)
    .build();
}
