import axios from 'axios';
import { commaSeparated } from '../util/paramSerializers';
import Client from './Client';

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL || '/api/data',
  function: process.env.FUNCTION_API_BASE_URL,
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

export const functionApi = new Client(
  axios.create({
    baseURL: baseUrl.function,
    paramsSerializer: commaSeparated,
  }),
);
