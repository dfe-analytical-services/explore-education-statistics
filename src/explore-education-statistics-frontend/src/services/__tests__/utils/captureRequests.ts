import Client, { RequestInterceptor } from '@common/services/api/Client';
import Axios, { InternalAxiosRequestConfig } from 'axios';

export interface CapturedRequest {
  method?: string;
  url: URL;
  data: unknown;
}

/**
 * Stub out the network for a {@link Client} and record each request
 * as axios would send it, including the serialised query string.
 */
export function captureRequests(client: Client): {
  requests: CapturedRequest[];
  release: () => void;
} {
  const requests: CapturedRequest[] = [];

  const interceptor: RequestInterceptor = {
    onRequest: (config: InternalAxiosRequestConfig) => {
      // eslint-disable-next-line no-param-reassign
      config.adapter = async adapterConfig => {
        requests.push({
          method: adapterConfig.method,
          url: new URL(Axios.getUri(adapterConfig)),
          data: adapterConfig.data,
        });

        return {
          data: {},
          status: 200,
          statusText: 'OK',
          headers: {},
          config: adapterConfig,
        };
      };

      return config;
    },
  };

  client.addRequestInterceptor(interceptor);

  return {
    requests,
    release: () => client.removeRequestInterceptor(interceptor),
  };
}
