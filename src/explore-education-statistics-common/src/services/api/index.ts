import {
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
} from '@common/contexts/NetworkActivityContext';
import Client, { RequestInterceptor } from './Client';

const eesRequestSourceInterceptor: RequestInterceptor = {
  onRequest: config => {
    config.headers.set('ees-request-source', 'EES');
    return config;
  },
};

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

export const frontendApi = new Client({
  baseURL: `${process.env.PUBLIC_URL}api`,
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export const publicApi = new Client({
  baseURL: `${process.env.PUBLIC_API_BASE_URL}/v1`,
  requestInterceptors: [
    eesRequestSourceInterceptor,
    networkActivityRequestInterceptor,
  ],
  responseInterceptors: [networkActivityResponseInterceptor],
});
