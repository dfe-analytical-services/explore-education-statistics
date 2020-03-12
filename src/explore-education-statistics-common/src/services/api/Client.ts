import {
  AxiosError,
  AxiosInstance,
  AxiosPromise,
  AxiosRequestConfig,
} from 'axios';

export type AxiosErrorHandler = (error: AxiosError) => void;

export interface ClientRequestConfig extends AxiosRequestConfig {
  onError?: AxiosErrorHandler;
}

class Client {
  public readonly axios: AxiosInstance;

  public errorHandler?: AxiosErrorHandler;

  public constructor(axios: AxiosInstance) {
    this.axios = axios;
  }

  public get<T = unknown>(
    url: string,
    config?: ClientRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.axios.get(url, config), config);
  }

  public post<T = unknown>(
    url: string,
    data?: unknown,
    config?: ClientRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.axios.post(url, data, config), config);
  }

  public put<T = unknown>(
    url: string,
    data?: unknown,
    config?: ClientRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.axios.put(url, data, config), config);
  }

  public patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: ClientRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.axios.patch(url, data, config), config);
  }

  public delete<T = unknown>(
    url: string,
    config?: ClientRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.axios.delete(url, config), config);
  }

  private unboxResponse<T>(
    promise: AxiosPromise<T>,
    config?: ClientRequestConfig,
  ) {
    const response = promise.then(({ data }) => data);

    if (config?.onError) {
      response.catch(config.onError);
    } else if (this.errorHandler) {
      response.catch(this.errorHandler);
    }

    return response;
  }
}

export default Client;
