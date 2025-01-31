import { exponentialBackoffPolicy } from '@admin/services/hubs/utils/retryPolicies';
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
      const authenticationResult = await acquireTokenSilent();
      return authenticationResult.accessToken;
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
