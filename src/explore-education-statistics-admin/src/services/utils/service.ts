import {
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
} from '@common/contexts/NetworkActivityContext';
import Client from '@common/services/api/Client';

const client = new Client({
  baseURL: '/api/',
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export default client;
