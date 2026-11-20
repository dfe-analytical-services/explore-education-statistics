import getPublicEnv from '@frontend/utils/publicEnv';
import {
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
} from '@common/contexts/NetworkActivityContext';
import Client from '@common/services/api/Client';

const notificationApi = new Client({
  baseURL: getPublicEnv('NOTIFICATION_API_BASE_URL'),
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export default notificationApi;
