import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import Client from './Client';

interface BaseUrls {
  content: string;
  data: string;
  notification: string;
}

export const baseUrl: BaseUrls = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL || '/api/data',
  notification: process.env.NOTIFICATION_API_BASE_URL,
};

export const contentApi = new Client(
  axios.create({
    baseURL: baseUrl.content,
    paramsSerializer: commaSeparated,
  }),
);

export const dataApi = new Client(
  axios.create({
    baseURL: baseUrl.data,
    paramsSerializer: commaSeparated,
  }),
);

export const notificationApi = new Client(
  axios.create({
    baseURL: baseUrl.notification,
    paramsSerializer: commaSeparated,
  }),
);

const urlToApi: { [key in keyof BaseUrls]: Client } = {
  content: contentApi,
  data: dataApi,
  notification: notificationApi,
};

/**
 * Explicitly set base URLs for clients if unable to
 * rely on environment variables to correctly set them.
 */
export function setApiBaseUrls(baseUrls: BaseUrls) {
  Object.assign(baseUrl, baseUrls);

  Object.entries(urlToApi).forEach(([key, client]) => {
    // eslint-disable-next-line no-param-reassign
    client.axios.defaults.baseURL = baseUrls[key as keyof BaseUrls];
  });
}
