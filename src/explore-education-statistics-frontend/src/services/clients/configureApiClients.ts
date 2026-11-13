import {
  contentApi,
  dataApi,
  frontendApi,
  publicApi,
} from '@common/services/api';
import getPublicEnv from '@frontend/utils/publicEnv';

/**
 * Set the base URLs of the shared API clients from the
 * runtime environment. Must run before any requests are made.
 */
export default function configureApiClients() {
  contentApi.baseURL = getPublicEnv('CONTENT_API_BASE_URL');
  dataApi.baseURL = getPublicEnv('DATA_API_BASE_URL');
  frontendApi.baseURL = `${getPublicEnv('PUBLIC_URL')}/api`;
  publicApi.baseURL = `${getPublicEnv('PUBLIC_API_BASE_URL')}/v1`;
}
