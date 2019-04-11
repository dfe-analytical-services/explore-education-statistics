import { AxiosInstance, AxiosPromise, AxiosRequestConfig } from 'axios';

class Client {
  constructor(public api: AxiosInstance) {
    this.api=api;
  }

  public get<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.unboxResponse(this.api.get(url, config));
  }

  public post<T = any>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.post(url, data, config));
  }

  public put<T = any>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.put(url, data, config));
  }

  public patch<T = any>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.unboxResponse(this.api.patch(url, data, config));
  }

  public delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.unboxResponse(this.api.delete(url, config));
  }

  private unboxResponse(promise: AxiosPromise<any>) {
    return promise.then(({ data }) => data);
  }
}

export default Client;
