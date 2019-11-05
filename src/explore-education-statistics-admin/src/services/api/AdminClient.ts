import Client from '@common/services/api/Client';
import { AxiosInstance, AxiosRequestConfig } from 'axios';
import authService from '@admin/components/api-authorization/AuthorizeService';

class AdminClient extends Client {
  public constructor(api: AxiosInstance) {
    super(api);
  }

  public get<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return AdminClient.addSecurityHeaders(config).then(extendedConfig =>
      super.get(url, extendedConfig),
    );
  }

  public post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return AdminClient.addSecurityHeaders(config).then(extendedConfig =>
      super.post(url, data, extendedConfig),
    );
  }

  public put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return AdminClient.addSecurityHeaders(config).then(extendedConfig =>
      super.put(url, data, extendedConfig),
    );
  }

  public patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return AdminClient.addSecurityHeaders(config).then(extendedConfig =>
      super.patch(url, data, extendedConfig),
    );
  }

  public delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return AdminClient.addSecurityHeaders(config).then(extendedConfig =>
      super.delete(url, extendedConfig),
    );
  }

  private static async addSecurityHeaders(
    config?: AxiosRequestConfig,
  ): Promise<AxiosRequestConfig> {
    const token = await authService.getAccessToken();

    const c: AxiosRequestConfig = config || {};

    const { headers, ...rest } = c;

    return {
      headers: {
        Authorization: `Bearer ${token}`,
        ...headers,
      },
      ...rest,
    };
  }
}

export default AdminClient;
