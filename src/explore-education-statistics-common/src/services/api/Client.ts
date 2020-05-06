import {
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
  ): Promise<T> {
    return Client.unboxResponse(this.axios.get(url, config));
  }

  public post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return Client.unboxResponse(this.axios.post(url, data, config));
  }

  public put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return Client.unboxResponse(this.axios.put(url, data, config));
  }

  public patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return Client.unboxResponse(this.axios.patch(url, data, config));
  }

  public delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return Client.unboxResponse(this.axios.delete(url, config));
  }

  private static unboxResponse<T>(promise: AxiosPromise<T>) {
    return promise.then(({ data }) => data);
  }
}

export default Client;
