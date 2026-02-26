import adminApi from '@admin/services/utils/service';
import { dataApi } from '@common/services/api';
import { acquireTokenSilent } from '@admin/auth/msal';
import logger from '@common/services/logger';

export default function configureAxios() {
  dataApi.baseURL = '/api/data';

  const clients = [dataApi, adminApi];

  clients.forEach(client => {
    client.addRequestInterceptor({
      onRequest: async config => {
        try {
          const { accessToken } = await acquireTokenSilent();
          if (accessToken) {
            // eslint-disable-next-line no-param-reassign
            config.headers.Authorization = `Bearer ${accessToken}`;
          }
        } catch (error) {
          logger.info(
            `configureAxios: skipping request due to auth error - ${error}`,
          );
        }

        return config;
      },
    });
  });
}
