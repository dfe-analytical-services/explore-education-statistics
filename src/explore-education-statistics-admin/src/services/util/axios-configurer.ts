import authService from '@admin/components/api-authorization/AuthorizeService';
import { AxiosInstance } from 'axios';

const configureAxiosWithAuthorization = (axiosInstance: AxiosInstance) => {
  axiosInstance.interceptors.request.use(
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

  axiosInstance.interceptors.response.use(
    response => response,
    error => Promise.reject(error.response),
  );
  return axiosInstance;
};

export default configureAxiosWithAuthorization;
