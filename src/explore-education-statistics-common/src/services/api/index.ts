import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import Client from './Client';

export const contentApi = new Client(
  axios.create({
    baseURL: process.env.CONTENT_API_BASE_URL,
    paramsSerializer: commaSeparated,
  }),
);

export const dataApi = new Client(
  axios.create({
    baseURL: process.env.DATA_API_BASE_URL,
    paramsSerializer: commaSeparated,
  }),
);
