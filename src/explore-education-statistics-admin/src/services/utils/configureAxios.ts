import adminApi from '@admin/services/utils/service';
import { dataApi } from '@common/services/api';
import { acquireTokenSilent } from '@admin/auth/msal';

export default function configureAxios() {
  dataApi.baseURL = '/api/data';

  const clients = [dataApi, adminApi];

  clients.forEach(client => {
    client.addRequestInterceptor({
      onRequest: async config => {
        const { accessToken } = await acquireTokenSilent();
        if (accessToken) {
          // eslint-disable-next-line no-param-reassign
          config.headers.Authorization = `Bearer ${accessToken}`;
        }

        return config;
      },
    });
  });
}
