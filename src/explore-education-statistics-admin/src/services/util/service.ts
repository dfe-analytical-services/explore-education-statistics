import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';

const baseURL = '/api/';

const axiosInstance = axios.create({
  baseURL,
  paramsSerializer: commaSeparated,
});

axiosInstance.interceptors.response.use(
  response => response,
  error => Promise.reject(error.response),
);

const client = new Client(axiosInstance);

export default client;
