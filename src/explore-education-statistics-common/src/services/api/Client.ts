import { AxiosInstance, AxiosPromise, AxiosRequestConfig } from 'axios';

class Client {
  public readonly api: AxiosInstance;

  public constructor(api: AxiosInstance) {
    this.api = api;
  }

  public get<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.get(url, config));
  }

  public post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.post(url, data, config));
  }

  public put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.put(url, data, config));
  }

  public patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.patch(url, data, config));
  }

  public delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.delete(url, config));
  }

  private unboxResponse<T>(promise: AxiosPromise<T>) {
    return promise.then(({ data }) => data);
  }
}

export default Client;
