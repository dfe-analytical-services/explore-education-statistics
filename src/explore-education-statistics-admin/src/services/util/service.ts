import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';
import createAxiosInstanceWithAuthorization from './axios-configurer';

export const baseURL = '/api/';

const axiosInstance = axios.create({
  baseURL,
  paramsSerializer: commaSeparated,
});

const client = new Client(createAxiosInstanceWithAuthorization(axiosInstance));

export default client;
