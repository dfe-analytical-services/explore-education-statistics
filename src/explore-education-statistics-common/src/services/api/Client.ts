import { CancellablePromise } from '@common/types/promise';
import Axios, {
  AxiosError,
  AxiosInstance,
  AxiosPromise,
  AxiosRequestConfig,
} from 'axios';

export type AxiosErrorHandler = (error: AxiosError) => void;

class Client {
  public readonly axios: AxiosInstance;

  public constructor(axios: AxiosInstance) {
    this.axios = axios;
  }

  public get<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return Client.handlePromise(this.axios.get(url, config));
  }

  public post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return Client.handlePromise(this.axios.post(url, data, config));
  }

  public put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return Client.handlePromise(this.axios.put(url, data, config));
  }

  public patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return Client.handlePromise(this.axios.patch(url, data, config));
  }

  public delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return Client.handlePromise(this.axios.delete(url, config));
  }

  private static handlePromise<T>(
    promise: AxiosPromise<T>,
  ): CancellablePromise<T> {
    const cancelToken = Axios.CancelToken.source();

    const cancellablePromise = promise.then(
      ({ data }) => data,
    ) as CancellablePromise<T>;

    cancellablePromise.cancel = cancelToken.cancel;

    return cancellablePromise;
  }
}

export default Client;
