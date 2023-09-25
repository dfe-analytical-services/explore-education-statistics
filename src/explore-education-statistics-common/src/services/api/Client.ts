import Axios, {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  CustomParamsSerializer,
  InternalAxiosRequestConfig,
} from 'axios';
import omit from 'lodash/omit';
import qs from 'qs';

export type RequestConfig = Omit<AxiosRequestConfig, 'method' | 'url'>;

interface ClientRequestConfig extends RequestConfig {
  rawResponse?: false;
}

interface ClientRawResponseRequestConfig extends RequestConfig {
  rawResponse: true;
}

export type ClientResponse<TValue> = Omit<
  AxiosResponse<TValue>,
  'config' | 'request'
>;

export interface RequestInterceptor {
  onRequest: (
    config: InternalAxiosRequestConfig,
  ) => InternalAxiosRequestConfig | Promise<InternalAxiosRequestConfig>;
  onError?: (error: unknown) => unknown;
}

export interface ResponseInterceptor {
  onResponse: (
    response: AxiosResponse<unknown>,
  ) => AxiosResponse<unknown> | Promise<AxiosResponse<unknown>>;
  onError?: (error: unknown) => unknown;
}

const defaultParamsSerializer: CustomParamsSerializer = params =>
  qs.stringify(params, { arrayFormat: 'comma' });

export interface ClientOptions {
  baseURL?: string;
  requestInterceptors?: RequestInterceptor[];
  responseInterceptors?: ResponseInterceptor[];
  paramsSerializer?: CustomParamsSerializer;
}

export default class Client {
  private readonly axios: AxiosInstance;

  private readonly requestInterceptors: Map<RequestInterceptor, number> =
    new Map();

  private readonly responseInterceptors: Map<ResponseInterceptor, number> =
    new Map();

  public constructor({
    baseURL,
    requestInterceptors,
    responseInterceptors,
    paramsSerializer = defaultParamsSerializer,
  }: ClientOptions) {
    this.axios = Axios.create({
      baseURL,
      paramsSerializer,
    });

    requestInterceptors?.forEach(this.addRequestInterceptor.bind(this));
    responseInterceptors?.forEach(this.addResponseInterceptor.bind(this));
  }

  public get<TValue = unknown>(
    url: string,
    config?: ClientRequestConfig,
  ): Promise<TValue>;

  public get<TValue = unknown>(
    url: string,
    config: ClientRawResponseRequestConfig,
  ): Promise<ClientResponse<TValue>>;

  public get<TValue = unknown>(
    url: string,
    config: RequestConfig = {},
  ): Promise<TValue> {
    return this.request({ ...config, url, method: 'GET' });
  }

  public post<TValue = unknown>(
    url: string,
    data?: unknown,
    config?: ClientRequestConfig,
  ): Promise<TValue>;

  public post<TValue = unknown>(
    url: string,
    data: unknown,
    config: ClientRawResponseRequestConfig,
  ): Promise<ClientResponse<TValue>>;

  public post<TValue = unknown>(
    url: string,
    data: unknown,
    config: RequestConfig = {},
  ): Promise<TValue> {
    return this.request({ ...config, url, data, method: 'POST' });
  }

  public put<TValue = unknown>(
    url: string,
    data?: unknown,
    config?: ClientRequestConfig,
  ): Promise<TValue>;

  public put<TValue = unknown>(
    url: string,
    data: unknown,
    config: ClientRawResponseRequestConfig,
  ): Promise<ClientResponse<TValue>>;

  public put<TValue = unknown>(
    url: string,
    data: unknown,
    config: RequestConfig = {},
  ): Promise<TValue> {
    return this.request({ ...config, url, data, method: 'PUT' });
  }

  public patch<TValue = unknown>(
    url: string,
    data?: unknown,
    config?: ClientRequestConfig,
  ): Promise<TValue>;

  public patch<TValue = unknown>(
    url: string,
    data: unknown,
    config: ClientRawResponseRequestConfig,
  ): Promise<ClientResponse<TValue>>;

  public patch<TValue = unknown>(
    url: string,
    data: unknown,
    config: RequestConfig = {},
  ): Promise<TValue> {
    return this.request({ ...config, url, data, method: 'PATCH' });
  }

  public delete<TValue = unknown>(
    url: string,
    config?: ClientRequestConfig,
  ): Promise<TValue>;

  public delete<TValue = unknown>(
    url: string,
    config: ClientRawResponseRequestConfig,
  ): Promise<ClientResponse<TValue>>;

  public delete<TValue = unknown>(
    url: string,
    config: RequestConfig = {},
  ): Promise<TValue> {
    return this.request({ ...config, url, method: 'DELETE' });
  }

  public set baseURL(url: string) {
    this.axios.defaults.baseURL = url;
  }

  public get baseURL(): string {
    return this.axios.defaults.baseURL ?? '';
  }

  public addRequestInterceptor(interceptor: RequestInterceptor): void {
    if (this.requestInterceptors.has(interceptor)) {
      return;
    }

    const id = this.axios.interceptors.request.use(
      interceptor.onRequest,
      interceptor.onError,
    );
    this.requestInterceptors.set(interceptor, id);
  }

  public addResponseInterceptor(interceptor: ResponseInterceptor): void {
    if (this.responseInterceptors.has(interceptor)) {
      return;
    }

    const id = this.axios.interceptors.response.use(
      interceptor.onResponse,
      interceptor.onError,
    );
    this.responseInterceptors.set(interceptor, id);
  }

  public removeRequestInterceptor(interceptor: RequestInterceptor): void {
    const id = this.requestInterceptors.get(interceptor);

    if (id) {
      this.axios.interceptors.request.eject(id);
    }
  }

  public removeResponseInterceptor(interceptor: ResponseInterceptor): void {
    const id = this.responseInterceptors.get(interceptor);

    if (id) {
      this.axios.interceptors.response.eject(id);
    }
  }

  private async request<TValue>(
    config: RequestConfig & {
      url: string;
      method: AxiosRequestConfig['method'];
    },
  ): Promise<TValue> {
    const response = await this.axios(config);

    if ('rawResponse' in config && config.rawResponse) {
      return omit(response, ['config', 'request']) as TValue;
    }

    return response.data;
  }
}
