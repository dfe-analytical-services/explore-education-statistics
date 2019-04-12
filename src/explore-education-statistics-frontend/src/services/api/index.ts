import Client from '@common/services/api/Client';
import { commaSeparated } from '@common/services/util/paramSerializers';
import axios from 'axios';

export const baseUrl = {
  content: process.env.CONTENT_API_BASE_URL,
  data: process.env.DATA_API_BASE_URL,
  function: process.env.FUNCTION_API_BASE_URL,
};

export const contentApi = new Client(
  axios.create({
    baseURL: `${baseUrl.content}/api/`,
    paramsSerializer: commaSeparated,
  }),
);

export const dataApi = new Client(
  axios.create({
    baseURL: `${baseUrl.data}/api/`,
    paramsSerializer: commaSeparated,
  }),
);

export const functionApi = new Client(
  axios.create({
    baseURL: `${baseUrl.function}/api/`,
    paramsSerializer: commaSeparated,
  }),
);
