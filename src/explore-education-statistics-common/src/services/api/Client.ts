import { CancellablePromise } from '@common/types/promise';
import Axios, {
  AxiosInstance,
  AxiosPromise,
  AxiosRequestConfig,
  CustomParamsSerializer,
} from 'axios';
import qs from 'qs';

const defaultParamsSerializer: CustomParamsSerializer = params =>
  qs.stringify(params, { arrayFormat: 'comma' });

export interface ClientOptions {
  baseURL?: string;
  paramsSerializer?: CustomParamsSerializer;
}

export default class Client {
  public readonly axios: AxiosInstance;

  public constructor({
    baseURL,
    paramsSerializer = defaultParamsSerializer,
  }: ClientOptions) {
    this.axios = Axios.create({
      baseURL,
      paramsSerializer,
    });
  }

  public get<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return this.handlePromise(this.axios.get(url, config));
  }

  public post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return this.handlePromise(this.axios.post(url, data, config));
  }

  public put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return this.handlePromise(this.axios.put(url, data, config));
  }

  public patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return this.handlePromise(this.axios.patch(url, data, config));
  }

  public delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): CancellablePromise<T> {
    return this.handlePromise(this.axios.delete(url, config));
  }

  private handlePromise<T>(promise: AxiosPromise<T>): CancellablePromise<T> {
    const cancelToken = Axios.CancelToken.source();

    const cancellablePromise = promise.then(
      ({ data }) => data,
    ) as CancellablePromise<T>;

    cancellablePromise.cancel = cancelToken.cancel;

    return cancellablePromise;
  }
}
