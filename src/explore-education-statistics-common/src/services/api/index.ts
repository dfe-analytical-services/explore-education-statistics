import axios from 'axios';

import qs from 'qs';
import Client from './Client';

export const contentApi = new Client(
  axios.create({
    baseURL: process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL,
    paramsSerializer: params => qs.stringify(params, { arrayFormat: 'comma' }),
  }),
);

export const dataApi = new Client(
  axios.create({
    baseURL: process.env.NEXT_PUBLIC_DATA_API_BASE_URL,
    paramsSerializer: params => qs.stringify(params, { arrayFormat: 'comma' }),
  }),
);
