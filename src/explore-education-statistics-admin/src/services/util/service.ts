import AdminClient from '@admin/services/api/AdminClient';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';

export const baseURL = '/api/';

const axiosInstance = axios.create({
  baseURL,
  paramsSerializer: commaSeparated,
});

axiosInstance.interceptors.response.use(
  response => response,
  error => Promise.reject(error.response),
);

const client = new AdminClient(axiosInstance);

export default client;
