import authService from '@admin/components/api-authorization/AuthorizeService';
import adminApi from '@admin/services/util/service';
import { contentApi, dataApi, functionApi } from '@common/services/api';
import Client from '@common/services/api/Client';

export const clients: Client[] = [adminApi, contentApi, dataApi, functionApi];

const configureAxios = () => {
  clients.forEach(client => {
    client.axios.interceptors.request.use(
      async config => {
        const token = await authService.getAccessToken();

        if (token) {
          return {
            ...config,
            headers: {
              ...config.headers,
              Authorization: `Bearer ${token}`,
            },
          };
        }

        return config;
      },
      error => Promise.reject(error),
    );

    client.axios.interceptors.response.use(
      response => response,
      error => Promise.reject(error),
    );
  });
};

export default configureAxios;
