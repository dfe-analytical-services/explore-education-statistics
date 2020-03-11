import authService from '@admin/components/api-authorization/AuthorizeService';
import { useErrorControl } from '@admin/contexts/ErrorControlContext';
import client from '@admin/services/util/service';
import { contentApi, dataApi, functionApi } from '@common/services/api';
import Client from '@common/services/api/Client';
import { AxiosInstance } from 'axios';
import React, { ReactNode, useEffect, useState } from 'react';

const clients: Client[] = [client, contentApi, dataApi, functionApi];

const configureAxios = (axiosInstance: AxiosInstance) => {
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
    error => Promise.reject(error),
  );
  return axiosInstance;
};

interface ConfigureAxiosProps {
  children: ReactNode;
}

const ConfigureAxios = ({ children }: ConfigureAxiosProps) => {
  const [loading, setLoading] = useState(true);
  const { handleApiErrors } = useErrorControl();

  useEffect(() => {
    clients.forEach(api => {
      configureAxios(api.axios);
    });

    setLoading(false);
  }, [handleApiErrors]);

  return <>{loading ? null : children}</>;
};

export default ConfigureAxios;
