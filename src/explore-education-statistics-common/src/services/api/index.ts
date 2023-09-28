import {
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
} from '@common/contexts/NetworkActivityContext';
import Client from './Client';

export const contentApi = new Client({
  baseURL: process.env.CONTENT_API_BASE_URL,
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export const dataApi = new Client({
  baseURL: process.env.DATA_API_BASE_URL,
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});
