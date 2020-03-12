import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';

export const baseURL = '/api/';

const axiosInstance = axios.create({
  baseURL,
  paramsSerializer: commaSeparated,
});

const client = new Client(axiosInstance);

export default client;
