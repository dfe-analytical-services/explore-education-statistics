import Client from '@common/services/api/Client';
import axios from 'axios';
import qs from 'qs';

export const baseURL = '/api/';

const axiosInstance = axios.create({
  baseURL,
  paramsSerializer: params => qs.stringify(params, { arrayFormat: 'comma' }),
});

const client = new Client(axiosInstance);

export default client;
