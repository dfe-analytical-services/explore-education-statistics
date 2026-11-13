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

// Base URLs aren't set here, as they differ between apps.
// Each app configures them at startup (e.g. the frontend's
// `configureApiClients` and the admin's `configureAxios`).

export const contentApi = new Client({
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export const dataApi = new Client({
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export const frontendApi = new Client({
  requestInterceptors: [networkActivityRequestInterceptor],
  responseInterceptors: [networkActivityResponseInterceptor],
});

export const publicApi = new Client({
  requestInterceptors: [
    eesRequestSourceInterceptor,
    networkActivityRequestInterceptor,
  ],
  responseInterceptors: [networkActivityResponseInterceptor],
});
