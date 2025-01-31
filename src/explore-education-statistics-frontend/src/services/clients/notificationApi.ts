import {
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
} from '@common/contexts/NetworkActivityContext';
import Client from '@common/services/api/Client';

const notificationApi = new Client({
  baseURL: process.env.NOTIFICATION_API_BASE_URL,
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export default notificationApi;
